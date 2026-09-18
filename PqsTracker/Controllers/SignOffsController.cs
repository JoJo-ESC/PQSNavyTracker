using Microsoft.AspNetCore.Mvc;
using PqsTracker.Dtos;
using PqsTracker.Services;

namespace PqsTracker.Controllers;

[ApiController]
[Route("api/signoffs")]
public class SignOffsController(ISignOffService signOffService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<SignOffDto>> Create(SignOffCreateDto dto)
    {
        var result = await signOffService.CreateAsync(dto);
        return result.ErrorType switch
        {
            ServiceErrorType.NotFound => NotFound(result.Error),
            ServiceErrorType.Validation => BadRequest(result.Error),
            _ => StatusCode(StatusCodes.Status201Created, result.Value)
        };
    }

    [HttpPost("{id:int}/revoke")]
    public async Task<ActionResult<SignOffDto>> Revoke(int id, SignOffRevokeDto dto)
    {
        var result = await signOffService.RevokeAsync(id, dto);
        return result.ErrorType switch
        {
            ServiceErrorType.NotFound => NotFound(result.Error),
            ServiceErrorType.Validation => BadRequest(result.Error),
            _ => Ok(result.Value)
        };
    }
}
