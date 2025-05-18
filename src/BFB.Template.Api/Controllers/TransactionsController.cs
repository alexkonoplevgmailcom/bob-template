using Abstractions.DTO;
using Abstractions.Interfaces;
using BFB.Template.Api.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace BFB.Template.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;
    private readonly ILogger<TransactionsController> _logger;

    public TransactionsController(
        ITransactionService transactionService,
        ILogger<TransactionsController> logger)
    {
        _transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Transaction>> GetTransactionById(int id)
    {
        try
        {
            var transaction = await _transactionService.GetTransactionByIdAsync(id);
            if (transaction == null)
            {
                return this.CreateNotFoundResponse($"Transaction with ID {id} not found");
            }
            return Ok(transaction);
        }
        catch (Exception ex)
        {
            return this.CreateInternalServerErrorResponse($"Error retrieving transaction with ID {id}", ex);
        }
    }

    [HttpGet("account/{accountId}")]
    public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactionsByAccountId(int accountId)
    {
        try
        {
            var transactions = await _transactionService.GetTransactionsByAccountIdAsync(accountId);
            return Ok(transactions);
        }
        catch (ArgumentException ex)
        {
            return this.CreateNotFoundResponse(ex.Message);
        }
        catch (Exception ex)
        {
            return this.CreateInternalServerErrorResponse($"Error retrieving transactions for account ID {accountId}", ex);
        }
    }

    [HttpGet("account/{accountId}/date-range")]
    public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactionsByDateRange(
        int accountId, 
        [FromQuery] DateTime startDate, 
        [FromQuery] DateTime endDate)
    {
        try
        {
            var transactions = await _transactionService.GetTransactionsByDateRangeAsync(accountId, startDate, endDate);
            return Ok(transactions);
        }
        catch (ArgumentException ex)
        {
            return this.CreateBadRequestResponse(ex.Message);
        }
        catch (Exception ex)
        {
            return this.CreateInternalServerErrorResponse(
                $"Error retrieving transactions for account ID {accountId} from {startDate} to {endDate}",
                ex);
        }
    }

    [HttpPost]
    public async Task<ActionResult<Transaction>> CreateTransaction(Transaction transaction)
    {
        try
        {
            var createdTransaction = await _transactionService.CreateTransactionAsync(transaction);
            return CreatedAtAction(nameof(GetTransactionById), new { id = createdTransaction.Id }, createdTransaction);
        }
        catch (ArgumentException ex)
        {
            return this.CreateBadRequestResponse(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return this.CreateBadRequestResponse(ex.Message);
        }
        catch (Exception ex)
        {
            return this.CreateInternalServerErrorResponse($"Error creating transaction for account ID {transaction.AccountId}", ex);
        }
    }
}