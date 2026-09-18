using PqsTracker.Dtos;

namespace PqsTracker.Services;

public interface ISignOffService
{
    Task<ServiceResult<SignOffDto>> CreateAsync(SignOffCreateDto dto);
    Task<ServiceResult<SignOffDto>> RevokeAsync(int id, SignOffRevokeDto dto);
}
