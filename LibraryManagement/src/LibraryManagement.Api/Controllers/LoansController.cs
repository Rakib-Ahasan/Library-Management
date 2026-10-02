using LibraryManagement.Application.Common;
using LibraryManagement.Application.Dtos;
using LibraryManagement.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class LoansController : ControllerBase
{
    private readonly ILoanService _loanService;

    public LoansController(ILoanService loanService)
    {
        _loanService = loanService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<LoanResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<LoanResponse>>> GetPaged([FromQuery] PagedQuery query, [FromQuery] bool activeOnly, CancellationToken ct)
    {
        return Ok(await _loanService.GetPagedAsync(query, activeOnly, ct));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(LoanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LoanResponse>> GetById(int id, CancellationToken ct)
    {
        return Ok(await _loanService.GetByIdAsync(id, ct));
    }

    [HttpPost("borrow")]
    [ProducesResponseType(typeof(LoanResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<LoanResponse>> Borrow(BorrowRequest request, CancellationToken ct)
    {
        var response = await _loanService.BorrowAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpPost("{id}/return")]
    [ProducesResponseType(typeof(LoanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LoanResponse>> Return(int id, CancellationToken ct)
    {
        return Ok(await _loanService.ReturnAsync(id, ct));
    }

    [HttpGet("overdue")]
    [ProducesResponseType(typeof(IReadOnlyList<LoanResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LoanResponse>>> GetOverdue(CancellationToken ct)
    {
        return Ok(await _loanService.GetOverdueAsync(ct));
    }
}
