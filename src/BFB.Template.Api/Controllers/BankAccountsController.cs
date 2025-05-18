using Abstractions.DTO;
using Abstractions.Interfaces;
using BFB.Template.Api.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace BFB.Template.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BankAccountsController : ControllerBase
{
    private readonly IBankAccountService _bankAccountService;
    private readonly ILogger<BankAccountsController> _logger;

    public BankAccountsController(IBankAccountService bankAccountService, ILogger<BankAccountsController> logger)
    {
        _bankAccountService = bankAccountService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BankAccount>>> GetAllAccounts()
    {
        try
        {
            var accounts = await _bankAccountService.GetAllBankAccountsAsync();
            return Ok(accounts);
        }
        catch (Exception ex)
        {
            return this.CreateInternalServerErrorResponse("Error retrieving all bank accounts", ex);
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BankAccount>> GetAccountById(int id)
    {
        try
        {
            var account = await _bankAccountService.GetBankAccountByIdAsync(id);
            if (account == null)
            {
                return this.CreateNotFoundResponse($"Bank account with ID {id} not found");
            }
            return Ok(account);
        }
        catch (Exception ex)
        {
            return this.CreateInternalServerErrorResponse($"Error retrieving bank account {id}", ex);
        }
    }

    [HttpPost]
    public async Task<ActionResult<BankAccount>> CreateAccount(BankAccount account)
    {
        try
        {
            var createdAccount = await _bankAccountService.CreateBankAccountAsync(account);
            return CreatedAtAction(nameof(GetAccountById), new { id = createdAccount.Id }, createdAccount);
        }
        catch (ArgumentException ex)
        {
            return this.CreateBadRequestResponse(ex.Message);
        }
        catch (Exception ex)
        {
            return this.CreateInternalServerErrorResponse("Error creating bank account", ex);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAccount(int id, BankAccount account)
    {
        try
        {
            if (id != account.Id)
            {
                return this.CreateBadRequestResponse("ID in route does not match ID in account object");
            }

            var success = await _bankAccountService.UpdateBankAccountAsync(id, account);
            if (!success)
            {
                return this.CreateNotFoundResponse($"Bank account with ID {id} not found");
            }
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return this.CreateBadRequestResponse(ex.Message);
        }
        catch (Exception ex)
        {
            return this.CreateInternalServerErrorResponse($"Error updating bank account {id}", ex);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAccount(int id)
    {
        try
        {
            var success = await _bankAccountService.DeleteBankAccountAsync(id);
            if (!success)
            {
                return this.CreateNotFoundResponse($"Bank account with ID {id} not found");
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            return this.CreateInternalServerErrorResponse($"Error deleting bank account {id}", ex);
        }
    }
}