using Microsoft.AspNetCore.Mvc;
using PqsTracker.Dtos;
using PqsTracker.Services;

namespace PqsTracker.Controllers;

[ApiController]
[Route("api/qualifications")]
public class QualificationsController(IQualificationService qualificationService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<QualificationSummaryDto>>> GetAll()
    {
        return Ok(await qualificationService.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<QualificationDetailDto>> GetById(int id)
    {
        var result = await qualificationService.GetByIdAsync(id);
        return result.ErrorType switch
        {
            ServiceErrorType.NotFound => NotFound(result.Error),
            _ => Ok(result.Value)
        };
    }

    [HttpPost]
    public async Task<ActionResult<QualificationDetailDto>> Create(QualificationCreateDto dto)
    {
        var created = await qualificationService.CreateAsync(dto);
        // 201 Created, with a Location header pointing at GetById and the
        // created resource as the body — standard REST convention for POST.
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<QualificationDetailDto>> Update(int id, QualificationUpdateDto dto)
    {
        var result = await qualificationService.UpdateAsync(id, dto);
        return result.ErrorType switch
        {
            ServiceErrorType.NotFound => NotFound(result.Error),
            _ => Ok(result.Value)
        };
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await qualificationService.DeleteAsync(id);
        return result.ErrorType switch
        {
            ServiceErrorType.NotFound => NotFound(result.Error),
            _ => NoContent()
        };
    }
}
