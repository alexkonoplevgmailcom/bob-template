using Abstractions.DTO;
using Abstractions.Interfaces;
using BFB.Template.Api.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace BFB.Template.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerAccountsController : ControllerBase
{
    private readonly ICustomerAccountService _customerAccountService;
    private readonly ILogger<CustomerAccountsController> _logger;

    public CustomerAccountsController(ICustomerAccountService customerAccountService, ILogger<CustomerAccountsController> logger)
    {
        _customerAccountService = customerAccountService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerAccount>>> GetAllAccounts()
    {
        try
        {
            var accounts = await _customerAccountService.GetAllCustomerAccountsAsync();
            return Ok(accounts);
        }
        catch (Exception ex)
        {
            return this.CreateInternalServerErrorResponse("Error retrieving all customer accounts", ex);
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerAccount>> GetAccountById(int id)
    {
        try
        {
            var account = await _customerAccountService.GetCustomerAccountByIdAsync(id);
            if (account == null)
            {
                return this.CreateNotFoundResponse($"Customer account with ID {id} not found");
            }
            return Ok(account);
        }
        catch (Exception ex)
        {
            return this.CreateInternalServerErrorResponse($"Error retrieving customer account {id}", ex);
        }
    }

    [HttpGet("customer/{customerId}")]
    public async Task<ActionResult<IEnumerable<CustomerAccount>>> GetAccountsByCustomerId(int customerId)
    {
        try
        {
            var accounts = await _customerAccountService.GetAccountsByCustomerIdAsync(customerId);
            return Ok(accounts);
        }
        catch (Exception ex)
        {
            return this.CreateInternalServerErrorResponse($"Error retrieving accounts for customer {customerId}", ex);
        }
    }

    [HttpPost]
    public async Task<ActionResult<CustomerAccount>> CreateAccount(CustomerAccount account)
    {
        try
        {
            var createdAccount = await _customerAccountService.CreateCustomerAccountAsync(account);
            return CreatedAtAction(nameof(GetAccountById), new { id = createdAccount.Id }, createdAccount);
        }
        catch (ArgumentException ex)
        {
            return this.CreateBadRequestResponse(ex.Message);
        }
        catch (Exception ex)
        {
            return this.CreateInternalServerErrorResponse("Error creating customer account", ex);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAccount(int id, CustomerAccount account)
    {
        try
        {
            if (id != account.Id)
            {
                return this.CreateBadRequestResponse("ID in route does not match ID in account object");
            }

            var success = await _customerAccountService.UpdateCustomerAccountAsync(id, account);
            if (!success)
            {
                return this.CreateNotFoundResponse($"Customer account with ID {id} not found");
            }
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return this.CreateBadRequestResponse(ex.Message);
        }
        catch (Exception ex)
        {
            return this.CreateInternalServerErrorResponse($"Error updating customer account {id}", ex);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAccount(int id)
    {
        try
        {
            var success = await _customerAccountService.DeleteCustomerAccountAsync(id);
            if (!success)
            {
                return this.CreateNotFoundResponse($"Customer account with ID {id} not found");
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            return this.CreateInternalServerErrorResponse($"Error deleting customer account {id}", ex);
        }
    }
}