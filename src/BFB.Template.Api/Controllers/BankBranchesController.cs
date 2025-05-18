using Abstractions.DTO;
using Abstractions.Interfaces;
using BFB.Template.Api.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace BFB.Template.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BankBranchesController : ControllerBase
{
    private readonly IBankBranchRepository _bankBranchRepository;
    private readonly ILogger<BankBranchesController> _logger;

    public BankBranchesController(IBankBranchRepository bankBranchRepository, ILogger<BankBranchesController> logger)
    {
        _bankBranchRepository = bankBranchRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BankBranch>>> GetAllBranches()
    {
        try
        {
            var branches = await _bankBranchRepository.GetAllBranchesAsync();
            return Ok(branches);
        }
        catch (Exception ex)
        {
            return this.CreateInternalServerErrorResponse("Error retrieving all bank branches", ex);
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BankBranch>> GetBranchById(int id)
    {
        try
        {
            var branch = await _bankBranchRepository.GetBranchByIdAsync(id);
            if (branch == null)
            {
                return this.CreateNotFoundResponse($"Branch with ID {id} not found");
            }
            return Ok(branch);
        }
        catch (Exception ex)
        {
            return this.CreateInternalServerErrorResponse($"Error retrieving branch {id}", ex);
        }
    }

    [HttpGet("bank/{bankId}")]
    public async Task<ActionResult<IEnumerable<BankBranch>>> GetBranchesByBankId(int bankId)
    {
        try
        {
            var branches = await _bankBranchRepository.GetBranchesByBankIdAsync(bankId);
            return Ok(branches);
        }
        catch (Exception ex)
        {
            return this.CreateInternalServerErrorResponse($"Error retrieving branches for bank {bankId}", ex);
        }
    }

    [HttpPost]
    public async Task<ActionResult<BankBranch>> CreateBranch(BankBranch branch)
    {
        try
        {
            var createdBranch = await _bankBranchRepository.CreateBranchAsync(branch);
            return CreatedAtAction(nameof(GetBranchById), new { id = createdBranch.Id }, createdBranch);
        }
        catch (ArgumentException ex)
        {
            return this.CreateBadRequestResponse(ex.Message);
        }
        catch (Exception ex)
        {
            return this.CreateInternalServerErrorResponse("Error creating branch", ex);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBranch(int id, BankBranch branch)
    {
        try
        {
            if (id != branch.Id)
            {
                return this.CreateBadRequestResponse("ID in route does not match ID in branch object");
            }

            var success = await _bankBranchRepository.UpdateBranchAsync(id, branch);
            if (!success)
            {
                return this.CreateNotFoundResponse($"Branch with ID {id} not found");
            }
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return this.CreateBadRequestResponse(ex.Message);
        }
        catch (Exception ex)
        {
            return this.CreateInternalServerErrorResponse($"Error updating branch {id}", ex);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBranch(int id)
    {
        try
        {
            var success = await _bankBranchRepository.DeleteBranchAsync(id);
            if (!success)
            {
                return this.CreateNotFoundResponse($"Branch with ID {id} not found");
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            return this.CreateInternalServerErrorResponse($"Error deleting branch {id}", ex);
        }
    }
}