using Microsoft.AspNetCore.Mvc;
using PqsTracker.Dtos;
using PqsTracker.Services;

namespace PqsTracker.Controllers;

[ApiController]
[Route("api/trainees")]
public class TraineesController(
    ITraineeService traineeService,
    IProgressService progressService,
    ISignOffService signOffService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<TraineeSummaryDto>>> GetAll()
    {
        return Ok(await traineeService.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TraineeDetailDto>> GetById(int id)
    {
        var result = await traineeService.GetByIdAsync(id);
        return result.ErrorType switch
        {
            ServiceErrorType.NotFound => NotFound(result.Error),
            _ => Ok(result.Value)
        };
    }

    [HttpPost]
    public async Task<ActionResult<TraineeDetailDto>> Create(TraineeCreateDto dto)
    {
        var result = await traineeService.CreateAsync(dto);
        return result.ErrorType switch
        {
            ServiceErrorType.Validation => BadRequest(result.Error),
            _ => CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
        };
    }

    [HttpGet("{traineeId:int}/progress/{qualificationId:int}")]
    public async Task<ActionResult<ProgressDto>> GetProgress(int traineeId, int qualificationId)
    {
        var result = await progressService.GetProgressAsync(traineeId, qualificationId);
        return result.ErrorType switch
        {
            ServiceErrorType.NotFound => NotFound(result.Error),
            _ => Ok(result.Value)
        };
    }

    [HttpGet("{traineeId:int}/signoffs/{qualificationId:int}")]
    public async Task<ActionResult<List<SignOffAuditDto>>> GetSignOffHistory(int traineeId, int qualificationId)
    {
        var result = await signOffService.GetHistoryAsync(traineeId, qualificationId);
        return result.ErrorType switch
        {
            ServiceErrorType.NotFound => NotFound(result.Error),
            _ => Ok(result.Value)
        };
    }
}
