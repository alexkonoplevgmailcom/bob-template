using Abstractions.DTO;
using Abstractions.Exceptions;
using Abstractions.Interfaces;
using Microsoft.Extensions.Logging;

namespace BFB.BusinessServices;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(
        ICustomerRepository customerRepository,
        ILogger<CustomerService> logger)
    {
        _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving all customers");
            return await _customerRepository.GetAllCustomersAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all customers");
            throw new DataAccessException("Failed to retrieve customers", ex);
        }
    }

    public async Task<Customer?> GetCustomerByIdAsync(int id)
    {
        try
        {
            _logger.LogInformation("Retrieving customer with ID: {CustomerId}", id);
            var customer = await _customerRepository.GetCustomerByIdAsync(id);
            
            if (customer == null)
            {
                throw new ResourceNotFoundException("Customer", id);
            }
            
            return customer;
        }
        catch (ResourceNotFoundException)
        {
            // Re-throw ResourceNotFoundException as is
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer with ID: {CustomerId}", id);
            throw new DataAccessException($"Failed to retrieve customer with ID {id}", ex);
        }
    }

    public async Task<Customer> CreateCustomerAsync(Customer customer)
    {
        // Add business validation
        if (string.IsNullOrWhiteSpace(customer.FirstName))
        {
            throw new BusinessValidationException("First name cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(customer.LastName))
        {
            throw new BusinessValidationException("Last name cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(customer.Email))
        {
            throw new BusinessValidationException("Email cannot be empty");
        }

        // Set default values
        customer.CreatedDate = DateTime.UtcNow;
        customer.IsActive = true;

        try
        {
            _logger.LogInformation("Creating new customer: {FirstName} {LastName}", customer.FirstName, customer.LastName);
            return await _customerRepository.CreateCustomerAsync(customer);
        }
        catch (BusinessValidationException)
        {
            // Re-throw BusinessValidationException as is
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer: {FirstName} {LastName}", customer.FirstName, customer.LastName);
            throw new DataAccessException("Failed to create customer", ex);
        }
    }

    public async Task<bool> UpdateCustomerAsync(int id, Customer customer)
    {
        try
        {
            // Verify that the customer exists
            var existingCustomer = await _customerRepository.GetCustomerByIdAsync(id);
            if (existingCustomer == null)
            {
                throw new ResourceNotFoundException("Customer", id);
            }

            // Add business validation
            if (string.IsNullOrWhiteSpace(customer.FirstName))
            {
                throw new BusinessValidationException("First name cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(customer.LastName))
            {
                throw new BusinessValidationException("Last name cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(customer.Email))
            {
                throw new BusinessValidationException("Email cannot be empty");
            }

            // Preserve creation date from the existing customer
            customer.CreatedDate = existingCustomer.CreatedDate;

            _logger.LogInformation("Updating customer with ID: {CustomerId}", id);
            var result = await _customerRepository.UpdateCustomerAsync(id, customer);
            
            if (!result)
            {
                throw new DataAccessException($"Failed to update customer with ID {id}");
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
            _logger.LogError(ex, "Error updating customer with ID: {CustomerId}", id);
            throw new DataAccessException($"Failed to update customer with ID {id}", ex);
        }
    }

    public async Task<bool> DeleteCustomerAsync(int id)
    {
        try
        {
            _logger.LogInformation("Deleting customer with ID: {CustomerId}", id);
            var result = await _customerRepository.DeleteCustomerAsync(id);
            
            if (!result)
            {
                throw new ResourceNotFoundException("Customer", id);
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
            _logger.LogError(ex, "Error deleting customer with ID: {CustomerId}", id);
            throw new DataAccessException($"Failed to delete customer with ID {id}", ex);
        }
    }
}