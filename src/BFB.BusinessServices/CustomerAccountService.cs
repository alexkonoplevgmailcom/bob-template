using Abstractions.DTO;
using Abstractions.Exceptions;
using Abstractions.Interfaces;
using Microsoft.Extensions.Logging;

namespace BFB.BusinessServices;

public class CustomerAccountService : ICustomerAccountService
{
    private readonly ICustomerAccountRepository _accountRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly ILogger<CustomerAccountService> _logger;

    public CustomerAccountService(
        ICustomerAccountRepository accountRepository,
        ICustomerRepository customerRepository,
        ILogger<CustomerAccountService> logger)
    {
        _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<CustomerAccount>> GetAllCustomerAccountsAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving all customer accounts");
            return await _accountRepository.GetAllAccountsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all customer accounts");
            throw new DataAccessException("Failed to retrieve customer accounts", ex);
        }
    }

    public async Task<CustomerAccount?> GetCustomerAccountByIdAsync(int id)
    {
        try
        {
            _logger.LogInformation("Retrieving customer account with ID: {AccountId}", id);
            var account = await _accountRepository.GetAccountByIdAsync(id);
            
            if (account == null)
            {
                throw new ResourceNotFoundException("Customer Account", id);
            }
            
            return account;
        }
        catch (ResourceNotFoundException)
        {
            // Re-throw ResourceNotFoundException as is
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer account with ID: {AccountId}", id);
            throw new DataAccessException($"Failed to retrieve customer account with ID {id}", ex);
        }
    }

    public async Task<IEnumerable<CustomerAccount>> GetAccountsByCustomerIdAsync(int customerId)
    {
        try
        {
            // Check if customer exists
            var customer = await _customerRepository.GetCustomerByIdAsync(customerId);
            if (customer == null)
            {
                throw new ResourceNotFoundException("Customer", customerId);
            }
            
            _logger.LogInformation("Retrieving accounts for customer with ID: {CustomerId}", customerId);
            return await _accountRepository.GetAccountsByCustomerIdAsync(customerId);
        }
        catch (ResourceNotFoundException)
        {
            // Re-throw ResourceNotFoundException as is
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving accounts for customer with ID: {CustomerId}", customerId);
            throw new DataAccessException($"Failed to retrieve accounts for customer with ID {customerId}", ex);
        }
    }

    public async Task<CustomerAccount> CreateCustomerAccountAsync(CustomerAccount customerAccount)
    {
        // Add business validation
        if (string.IsNullOrWhiteSpace(customerAccount.AccountNumber))
        {
            throw new BusinessValidationException("Account number cannot be empty");
        }

        if (customerAccount.Balance < 0)
        {
            throw new BusinessValidationException("Initial balance cannot be negative");
        }

        try
        {
            // Validate Customer exists
            var customer = await _customerRepository.GetCustomerByIdAsync(customerAccount.CustomerId);
            if (customer == null)
            {
                throw new ResourceNotFoundException("Customer", customerAccount.CustomerId);
            }

            // Set default values
            customerAccount.CreatedDate = DateTime.UtcNow;
            customerAccount.IsActive = true;

            _logger.LogInformation("Creating new customer account: {AccountNumber} for customer ID: {CustomerId}", 
                customerAccount.AccountNumber, customerAccount.CustomerId);
            
            return await _accountRepository.CreateAccountAsync(customerAccount);
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
            _logger.LogError(ex, "Error creating customer account: {AccountNumber} for customer ID: {CustomerId}", 
                customerAccount.AccountNumber, customerAccount.CustomerId);
            throw new DataAccessException("Failed to create customer account", ex);
        }
    }

    public async Task<bool> UpdateCustomerAccountAsync(int id, CustomerAccount customerAccount)
    {
        try {
            // Verify that the account exists
            var existingAccount = await _accountRepository.GetAccountByIdAsync(id);
            if (existingAccount == null)
            {
                throw new ResourceNotFoundException("Customer Account", id);
            }

            // Add business validation
            if (string.IsNullOrWhiteSpace(customerAccount.AccountNumber))
            {
                throw new BusinessValidationException("Account number cannot be empty");
            }

            // Validate Customer exists if changing customer
            if (customerAccount.CustomerId != existingAccount.CustomerId)
            {
                var customer = await _customerRepository.GetCustomerByIdAsync(customerAccount.CustomerId);
                if (customer == null)
                {
                    throw new ResourceNotFoundException("Customer", customerAccount.CustomerId);
                }
            }

            // Preserve creation date from the existing account
            customerAccount.CreatedDate = existingAccount.CreatedDate;

            _logger.LogInformation("Updating customer account with ID: {AccountId}", id);
            var result = await _accountRepository.UpdateAccountAsync(id, customerAccount);
            
            if (!result)
            {
                throw new DataAccessException($"Failed to update customer account with ID {id}");
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
            _logger.LogError(ex, "Error updating customer account with ID: {AccountId}", id);
            throw new DataAccessException($"Failed to update customer account with ID {id}", ex);
        }
    }

    public async Task<bool> DeleteCustomerAccountAsync(int id)
    {
        try
        {
            _logger.LogInformation("Deleting customer account with ID: {AccountId}", id);
            var result = await _accountRepository.DeleteAccountAsync(id);
            
            if (!result)
            {
                throw new ResourceNotFoundException("Customer Account", id);
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
            _logger.LogError(ex, "Error deleting customer account with ID: {AccountId}", id);
            throw new DataAccessException($"Failed to delete customer account with ID {id}", ex);
        }
    }
}