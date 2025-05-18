using Abstractions.DTO;
using Abstractions.Exceptions;
using Abstractions.Interfaces;
using Microsoft.Extensions.Logging;

namespace BFB.BusinessServices;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IBankAccountRepository _bankAccountRepository;
    private readonly ILogger<TransactionService> _logger;

    public TransactionService(
        ITransactionRepository transactionRepository,
        IBankAccountRepository bankAccountRepository,
        ILogger<TransactionService> logger)
    {
        _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        _bankAccountRepository = bankAccountRepository ?? throw new ArgumentNullException(nameof(bankAccountRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsByAccountIdAsync(int accountId)
    {
        try
        {
            // Verify the account exists
            var account = await _bankAccountRepository.GetBankAccountByIdAsync(accountId);
            if (account == null)
            {
                _logger.LogWarning("Attempted to retrieve transactions for non-existent account ID: {AccountId}", accountId);
                throw new ResourceNotFoundException("Bank Account", accountId);
            }

            _logger.LogInformation("Retrieving transactions for account ID: {AccountId}", accountId);
            return await _transactionRepository.GetTransactionsByAccountIdAsync(accountId);
        }
        catch (ResourceNotFoundException)
        {
            // Re-throw ResourceNotFoundException as is
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transactions for account ID: {AccountId}", accountId);
            throw new DataAccessException($"Failed to retrieve transactions for account ID {accountId}", ex);
        }
    }

    public async Task<Transaction?> GetTransactionByIdAsync(int transactionId)
    {
        try
        {
            _logger.LogInformation("Retrieving transaction with ID: {TransactionId}", transactionId);
            var transaction = await _transactionRepository.GetTransactionByIdAsync(transactionId);
            
            if (transaction == null)
            {
                throw new ResourceNotFoundException("Transaction", transactionId);
            }
            
            return transaction;
        }
        catch (ResourceNotFoundException)
        {
            // Re-throw ResourceNotFoundException as is
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transaction with ID: {TransactionId}", transactionId);
            throw new DataAccessException($"Failed to retrieve transaction with ID {transactionId}", ex);
        }
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsByDateRangeAsync(int accountId, DateTime startDate, DateTime endDate)
    {
        try
        {
            // Validate parameters
            if (startDate > endDate)
            {
                throw new BusinessValidationException("Start date must be before or equal to end date");
            }

            // Verify the account exists
            var account = await _bankAccountRepository.GetBankAccountByIdAsync(accountId);
            if (account == null)
            {
                _logger.LogWarning("Attempted to retrieve transactions for non-existent account ID: {AccountId}", accountId);
                throw new ResourceNotFoundException("Bank Account", accountId);
            }

            _logger.LogInformation("Retrieving transactions for account ID: {AccountId} from {StartDate} to {EndDate}", 
                accountId, startDate, endDate);
            return await _transactionRepository.GetTransactionsByDateRangeAsync(accountId, startDate, endDate);
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transactions for account ID: {AccountId} from {StartDate} to {EndDate}", 
                accountId, startDate, endDate);
            throw new DataAccessException($"Failed to retrieve transactions for account ID {accountId} in the specified date range", ex);
        }
    }

    public async Task<Transaction> CreateTransactionAsync(Transaction transaction)
    {
        try
        {
            // Validate transaction
            if (transaction.AccountId <= 0)
            {
                throw new BusinessValidationException("Account ID is required");
            }

            if (transaction.Amount == 0)
            {
                throw new BusinessValidationException("Transaction amount cannot be zero");
            }

            if (string.IsNullOrWhiteSpace(transaction.TransactionType))
            {
                throw new BusinessValidationException("Transaction type is required");
            }

            // Verify the account exists
            var account = await _bankAccountRepository.GetBankAccountByIdAsync(transaction.AccountId);
            if (account == null)
            {
                _logger.LogWarning("Attempted to create transaction for non-existent account ID: {AccountId}", transaction.AccountId);
                throw new ResourceNotFoundException("Bank Account", transaction.AccountId);
            }

            // For withdrawals and transfers, check if account has sufficient funds
            if ((transaction.TransactionType.Equals("Withdrawal", StringComparison.OrdinalIgnoreCase) || 
                transaction.TransactionType.Equals("Transfer", StringComparison.OrdinalIgnoreCase)) && 
                transaction.Amount < 0 && 
                Math.Abs(transaction.Amount) > account.Balance)
            {
                _logger.LogWarning("Insufficient funds for transaction on account ID: {AccountId}. Balance: {Balance}, Transaction amount: {Amount}", 
                    transaction.AccountId, account.Balance, transaction.Amount);
                throw new BusinessValidationException("Insufficient funds for this transaction");
            }

            // Set default values
            transaction.Timestamp = DateTime.UtcNow;
            
            _logger.LogInformation("Creating new {TransactionType} transaction for account ID: {AccountId} with amount: {Amount}", 
                transaction.TransactionType, transaction.AccountId, transaction.Amount);
            
            var createdTransaction = await _transactionRepository.CreateTransactionAsync(transaction);
            
            // Update account balance
            account.Balance += transaction.Amount;
            await _bankAccountRepository.UpdateBankAccountAsync(account.Id, account);
            
            return createdTransaction;
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating {TransactionType} transaction for account ID: {AccountId} with amount: {Amount}", 
                transaction.TransactionType, transaction.AccountId, transaction.Amount);
            throw new DataAccessException($"Failed to create transaction for account ID {transaction.AccountId}", ex);
        }
    }
}