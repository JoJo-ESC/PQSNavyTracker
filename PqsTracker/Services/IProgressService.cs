using PqsTracker.Dtos;

namespace PqsTracker.Services;

public interface IProgressService
{
    Task<ServiceResult<ProgressDto>> GetProgressAsync(int traineeId, int qualificationId);
}
