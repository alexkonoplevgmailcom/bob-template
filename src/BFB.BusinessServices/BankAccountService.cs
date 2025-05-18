using Abstractions.DTO;
using Abstractions.Exceptions;
using Abstractions.Interfaces;
using Microsoft.Extensions.Logging;

namespace BFB.BusinessServices;

public class BankAccountService : IBankAccountService
{
    private readonly IBankAccountRepository _bankAccountRepository;
    private readonly IBankBranchRepository _bankBranchRepository;
    private readonly ILogger<BankAccountService> _logger;

    public BankAccountService(
        IBankAccountRepository bankAccountRepository,
        IBankBranchRepository bankBranchRepository,
        ILogger<BankAccountService> logger)
    {
        _bankAccountRepository = bankAccountRepository ?? throw new ArgumentNullException(nameof(bankAccountRepository));
        _bankBranchRepository = bankBranchRepository ?? throw new ArgumentNullException(nameof(bankBranchRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<BankAccount>> GetAllBankAccountsAsync()
    {
        try
        {
            var accounts = await _bankAccountRepository.GetAllBankAccountsAsync();
            
            // Enhance accounts with branch information
            await EnrichAccountsWithBranchInformation(accounts);
            
            return accounts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all bank accounts");
            throw new DataAccessException("Failed to retrieve bank accounts", ex);
        }
    }

    public async Task<BankAccount?> GetBankAccountByIdAsync(int id)
    {
        try
        {
            var account = await _bankAccountRepository.GetBankAccountByIdAsync(id);
            
            if (account == null)
            {
                throw new ResourceNotFoundException("Bank Account", id);
            }
            
            // Enhance the account with branch information
            await EnrichAccountWithBranchInformation(account);
            
            return account;
        }
        catch (ResourceNotFoundException)
        {
            // Re-throw ResourceNotFoundException as is
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving bank account with ID: {AccountId}", id);
            throw new DataAccessException($"Failed to retrieve bank account with ID {id}", ex);
        }
    }

    public async Task<BankAccount> CreateBankAccountAsync(BankAccount bankAccountDto)
    {
        // Add business validation
        if (string.IsNullOrWhiteSpace(bankAccountDto.AccountNumber))
        {
            throw new BusinessValidationException("Account number cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(bankAccountDto.OwnerName))
        {
            throw new BusinessValidationException("Owner name cannot be empty");
        }

        if (bankAccountDto.Balance < 0)
        {
            throw new BusinessValidationException("Initial balance cannot be negative");
        }

        try
        {
            // Validate BranchId if provided
            if (bankAccountDto.BranchId > 0)
            {
                var branch = await _bankBranchRepository.GetBranchByIdAsync(bankAccountDto.BranchId);
                if (branch == null)
                {
                    throw new ResourceNotFoundException("Branch", bankAccountDto.BranchId);
                }
                
                // Ensure BankId matches the branch's BankId
                bankAccountDto.BankId = branch.BankId;
            }

            // Set default values
            bankAccountDto.CreatedDate = DateTime.UtcNow;
            bankAccountDto.IsActive = true;

            var createdAccount = await _bankAccountRepository.CreateBankAccountAsync(bankAccountDto);
            
            // Enhance the newly created account with branch information
            await EnrichAccountWithBranchInformation(createdAccount);
            
            return createdAccount;
        }
        catch (ResourceNotFoundException)
        {
            // Re-throw ResourceNotFoundException as is
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating bank account: {AccountNumber}", bankAccountDto.AccountNumber);
            throw new DataAccessException("Failed to create bank account", ex);
        }
    }

    public async Task<bool> UpdateBankAccountAsync(int id, BankAccount bankAccountDto)
    {
        // Add business validation
        if (string.IsNullOrWhiteSpace(bankAccountDto.AccountNumber))
        {
            throw new BusinessValidationException("Account number cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(bankAccountDto.OwnerName))
        {
            throw new BusinessValidationException("Owner name cannot be empty");
        }

        try
        {
            // Verify that the account exists
            var existingAccount = await _bankAccountRepository.GetBankAccountByIdAsync(id);
            if (existingAccount == null)
            {
                throw new ResourceNotFoundException("Bank Account", id);
            }

            // Validate BranchId if it's changing
            if (bankAccountDto.BranchId > 0 && bankAccountDto.BranchId != existingAccount.BranchId)
            {
                var branch = await _bankBranchRepository.GetBranchByIdAsync(bankAccountDto.BranchId);
                if (branch == null)
                {
                    throw new ResourceNotFoundException("Branch", bankAccountDto.BranchId);
                }
                
                // Ensure BankId matches the branch's BankId
                bankAccountDto.BankId = branch.BankId;
            }

            // Preserve creation date from the existing account
            bankAccountDto.CreatedDate = existingAccount.CreatedDate;

            var result = await _bankAccountRepository.UpdateBankAccountAsync(id, bankAccountDto);
            if (!result)
            {
                throw new DataAccessException($"Failed to update bank account with ID {id}");
            }
            
            return true;
        }
        catch (ResourceNotFoundException)
        {
            // Re-throw ResourceNotFoundException as is
            throw;
        }
        catch (BusinessValidationException)
        {
            // Re-throw BusinessValidationException as is
            throw;
        }
        catch (DataAccessException)
        {
            // Re-throw DataAccessException as is
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating bank account with ID: {AccountId}", id);
            throw new DataAccessException($"Failed to update bank account with ID {id}", ex);
        }
    }

    public async Task<bool> DeleteBankAccountAsync(int id)
    {
        try
        {
            var result = await _bankAccountRepository.DeleteBankAccountAsync(id);
            if (!result)
            {
                throw new ResourceNotFoundException("Bank Account", id);
            }
            return true;
        }
        catch (ResourceNotFoundException)
        {
            // Re-throw ResourceNotFoundException as is
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting bank account with ID: {AccountId}", id);
            throw new DataAccessException($"Failed to delete bank account with ID {id}", ex);
        }
    }
    
    // Helper methods to enrich account information with branch details
    private async Task EnrichAccountWithBranchInformation(BankAccount account)
    {
        if (account.BranchId <= 0)
        {
            return;
        }
        
        try
        {
            var branch = await _bankBranchRepository.GetBranchByIdAsync(account.BranchId);
            if (branch != null)
            {
                // We could extend the BankAccount DTO to include branch details,
                // but for now we're just ensuring the BankId is consistent
                if (account.BankId != branch.BankId)
                {
                    _logger.LogWarning("BankId mismatch for account {AccountId}: Account BankId={AccountBankId}, Branch BankId={BranchBankId}",
                        account.Id, account.BankId, branch.BankId);
                    
                    account.BankId = branch.BankId;
                }
            }
            else
            {
                _logger.LogWarning("Branch with ID {BranchId} not found for account {AccountId}", account.BranchId, account.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving branch information for account {AccountId}", account.Id);
        }
    }
    
    private async Task EnrichAccountsWithBranchInformation(IEnumerable<BankAccount> accounts)
    {
        foreach (var account in accounts)
        {
            await EnrichAccountWithBranchInformation(account);
        }
    }
}