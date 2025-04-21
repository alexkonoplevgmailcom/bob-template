using Abstractions.DTO;
using Abstractions.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BFB.Template.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BankAccountController : ControllerBase
{
    private readonly IBankAccountService _bankAccountService;
    private readonly ILogger<BankAccountController> _logger;

    public BankAccountController(IBankAccountService bankAccountService, ILogger<BankAccountController> logger)
    {
        _bankAccountService = bankAccountService ?? throw new ArgumentNullException(nameof(bankAccountService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogInformation("Retrieving all bank accounts");
        var accounts = await _bankAccountService.GetAllBankAccountsAsync();
        return Ok(accounts);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var account = await _bankAccountService.GetBankAccountByIdAsync(id);
        if (account == null)
        {
            _logger.LogWarning("Bank account with ID {Id} not found", id);
            return NotFound();
        }

        _logger.LogInformation("Retrieved bank account with ID {Id}", id);
        return Ok(account);
    }

    [HttpPost]
    public async Task<IActionResult> Create(BankAccount account)
    {
        try
        {
            var createdAccount = await _bankAccountService.CreateBankAccountAsync(account);
            _logger.LogInformation("Created new bank account with ID {Id}", createdAccount.Id);
            return CreatedAtAction(nameof(GetById), new { id = createdAccount.Id }, createdAccount);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid input when creating bank account");
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, BankAccount account)
    {
        try
        {
            var result = await _bankAccountService.UpdateBankAccountAsync(id, account);
            if (!result)
            {
                _logger.LogWarning("Cannot update: Bank account with ID {Id} not found", id);
                return NotFound();
            }

            _logger.LogInformation("Updated bank account with ID {Id}", id);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid input when updating bank account with ID {Id}", id);
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _bankAccountService.DeleteBankAccountAsync(id);
        if (!result)
        {
            _logger.LogWarning("Cannot delete: Bank account with ID {Id} not found", id);
            return NotFound();
        }

        _logger.LogInformation("Deleted bank account with ID {Id}", id);
        return NoContent();
    }
}