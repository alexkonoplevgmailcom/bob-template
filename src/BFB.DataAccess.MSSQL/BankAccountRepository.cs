using Abstractions.DTO;
using Abstractions.Interfaces;
using BFB.DataAccess.MSSQL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace BFB.DataAccess.MSSQL;

public class BankAccountRepository : IBankAccountRepository
{
    private readonly BankDbContext _dbContext;
    private readonly IMemoryCache _memoryCache;
    private readonly RetryPolicyService _retryPolicyService;
    private readonly ILogger<BankAccountRepository> _logger;
    
    // Cache keys
    private const string AllBankAccountsCacheKey = "AllBankAccounts";
    private const string BankAccountByIdCacheKeyPrefix = "BankAccount_";
    
    // Cache expiration time
    private static readonly TimeSpan CacheExpirationTime = TimeSpan.FromMinutes(10);
    
    public BankAccountRepository(
        BankDbContext dbContext, 
        IMemoryCache memoryCache, 
        RetryPolicyService retryPolicyService,
        ILogger<BankAccountRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        _retryPolicyService = retryPolicyService ?? throw new ArgumentNullException(nameof(retryPolicyService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<BankAccount>> GetAllBankAccountsAsync()
    {
        // Try to get from cache first
        if (_memoryCache.TryGetValue(AllBankAccountsCacheKey, out IEnumerable<BankAccount>? cachedAccounts) && cachedAccounts != null)
        {
            _logger.LogDebug("Retrieved all bank accounts from cache");
            return cachedAccounts;
        }
        
        // If not in cache, get from database with retry policy
        var bankAccounts = await _retryPolicyService.ExecuteWithRetryAsync(async () => 
        {
            _logger.LogDebug("Retrieving all bank accounts from database");
            var accounts = await _dbContext.Accounts.ToListAsync();
            return accounts.Select(MapToDto).ToList();
        }, "GetAllBankAccounts");
        
        // Store in cache
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(CacheExpirationTime);
            
        _memoryCache.Set(AllBankAccountsCacheKey, bankAccounts, cacheEntryOptions);
        
        return bankAccounts;
    }

    public async Task<BankAccount?> GetBankAccountByIdAsync(int id)
    {
        string cacheKey = $"{BankAccountByIdCacheKeyPrefix}{id}";
        
        // Try to get from cache first
        if (_memoryCache.TryGetValue(cacheKey, out BankAccount? cachedAccount) && cachedAccount != null)
        {
            _logger.LogDebug("Retrieved bank account {Id} from cache", id);
            return cachedAccount;
        }
        
        // If not in cache, get from database with retry policy
        var bankAccount = await _retryPolicyService.ExecuteWithRetryAsync(async () => 
        {
            _logger.LogDebug("Retrieving bank account {Id} from database", id);
            var account = await _dbContext.Accounts.FindAsync(id);
            return account != null ? MapToDto(account) : null;
        }, $"GetBankAccountById_{id}");
        
        if (bankAccount == null)
            return null;
        
        // Store in cache
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(CacheExpirationTime);
            
        _memoryCache.Set(cacheKey, bankAccount, cacheEntryOptions);
        
        return bankAccount;
    }

    public async Task<BankAccount> CreateBankAccountAsync(BankAccount bankAccountDto)
    {
        var accountEntity = MapToEntity(bankAccountDto);
        
        await _dbContext.Accounts.AddAsync(accountEntity);
        await _dbContext.SaveChangesAsync();
        
        var createdBankAccount = MapToDto(accountEntity);
        
        // Invalidate the list cache since we added a new item
        _memoryCache.Remove(AllBankAccountsCacheKey);
        
        // Add new item to cache
        string cacheKey = $"{BankAccountByIdCacheKeyPrefix}{createdBankAccount.Id}";
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(CacheExpirationTime);
        _memoryCache.Set(cacheKey, createdBankAccount, cacheEntryOptions);
        
        return createdBankAccount;
    }

    public async Task<bool> UpdateBankAccountAsync(int id, BankAccount bankAccountDto)
    {
        var accountEntity = await _dbContext.Accounts.FindAsync(id);
        
        if (accountEntity == null)
            return false;
            
        // Update entity properties
        accountEntity.AccountNumber = bankAccountDto.AccountNumber;
        accountEntity.OwnerName = bankAccountDto.OwnerName;
        accountEntity.Balance = bankAccountDto.Balance;
        accountEntity.AccountTypeId = (int)bankAccountDto.Type;
        accountEntity.IsActive = bankAccountDto.IsActive;
        
        _dbContext.Accounts.Update(accountEntity);
        await _dbContext.SaveChangesAsync();
        
        // Invalidate caches
        string cacheKey = $"{BankAccountByIdCacheKeyPrefix}{id}";
        _memoryCache.Remove(cacheKey);
        _memoryCache.Remove(AllBankAccountsCacheKey);
        
        // Add updated item to cache
        var updatedBankAccount = MapToDto(accountEntity);
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(CacheExpirationTime);
        _memoryCache.Set(cacheKey, updatedBankAccount, cacheEntryOptions);
        
        return true;
    }

    public async Task<bool> DeleteBankAccountAsync(int id)
    {
        var accountEntity = await _dbContext.Accounts.FindAsync(id);
        
        if (accountEntity == null)
            return false;
            
        _dbContext.Accounts.Remove(accountEntity);
        await _dbContext.SaveChangesAsync();
        
        // Invalidate caches
        string cacheKey = $"{BankAccountByIdCacheKeyPrefix}{id}";
        _memoryCache.Remove(cacheKey);
        _memoryCache.Remove(AllBankAccountsCacheKey);
        
        return true;
    }

    // Maps from the database entity to the DTO
    private static BankAccount MapToDto(Account account)
    {
        return new BankAccount
        {
            Id = account.Id,
            AccountNumber = account.AccountNumber,
            OwnerName = account.OwnerName,
            Balance = account.Balance,
            Type = (AccountType)account.AccountTypeId,
            CreatedDate = account.CreatedDate,
            IsActive = account.IsActive
        };
    }

    // Maps from the DTO to a database entity
    private static Account MapToEntity(BankAccount bankAccount)
    {
        return new Account
        {
            Id = bankAccount.Id,
            AccountNumber = bankAccount.AccountNumber,
            OwnerName = bankAccount.OwnerName,
            Balance = bankAccount.Balance,
            AccountTypeId = (int)bankAccount.Type,
            CreatedDate = bankAccount.CreatedDate,
            IsActive = bankAccount.IsActive
        };
    }
}