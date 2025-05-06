using Abstractions.DTO;
using Abstractions.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BFB.Template.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BankBranchController : ControllerBase
{
    private readonly IBankBranchRepository _branchRepository;
    private readonly ILogger<BankBranchController> _logger;

    public BankBranchController(IBankBranchRepository branchRepository, ILogger<BankBranchController> logger)
    {
        _branchRepository = branchRepository ?? throw new ArgumentNullException(nameof(branchRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogInformation("Retrieving all bank branches from MongoDB");
        var branches = await _branchRepository.GetAllBranchesAsync();
        return Ok(branches);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var branch = await _branchRepository.GetBranchByIdAsync(id);
        if (branch == null)
        {
            _logger.LogWarning("Bank branch with ID {Id} not found in MongoDB", id);
            return NotFound();
        }

        _logger.LogInformation("Retrieved bank branch with ID {Id} from MongoDB", id);
        return Ok(branch);
    }

    [HttpGet("bank/{bankId}")]
    public async Task<IActionResult> GetByBankId(int bankId)
    {
        var branches = await _branchRepository.GetBranchesByBankIdAsync(bankId);
        _logger.LogInformation("Retrieved {Count} bank branches for bank ID {BankId} from MongoDB", 
            branches.Count(), bankId);
        return Ok(branches);
    }

    [HttpPost]
    public async Task<IActionResult> Create(BankBranch branch)
    {
        try
        {
            var createdBranch = await _branchRepository.CreateBranchAsync(branch);
            _logger.LogInformation("Created new bank branch with ID {Id} in MongoDB", createdBranch.Id);
            return CreatedAtAction(nameof(GetById), new { id = createdBranch.Id }, createdBranch);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid input when creating bank branch");
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, BankBranch branch)
    {
        try
        {
            var result = await _branchRepository.UpdateBranchAsync(id, branch);
            if (!result)
            {
                _logger.LogWarning("Cannot update: Bank branch with ID {Id} not found in MongoDB", id);
                return NotFound();
            }

            _logger.LogInformation("Updated bank branch with ID {Id} in MongoDB", id);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid input when updating bank branch with ID {Id}", id);
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _branchRepository.DeleteBranchAsync(id);
        if (!result)
        {
            _logger.LogWarning("Cannot delete: Bank branch with ID {Id} not found in MongoDB", id);
            return NotFound();
        }

        _logger.LogInformation("Deleted bank branch with ID {Id} from MongoDB", id);
        return NoContent();
    }
}