using PqsTracker.Dtos;

namespace PqsTracker.Services;

public interface ITraineeService
{
    Task<List<TraineeSummaryDto>> GetAllAsync();
    Task<ServiceResult<TraineeDetailDto>> GetByIdAsync(int id);
    Task<ServiceResult<TraineeDetailDto>> CreateAsync(TraineeCreateDto dto);
}
