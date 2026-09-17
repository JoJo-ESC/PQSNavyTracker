using PqsTracker.Dtos;

namespace PqsTracker.Services;

public interface IQualificationService
{
    Task<List<QualificationSummaryDto>> GetAllAsync();
    Task<ServiceResult<QualificationDetailDto>> GetByIdAsync(int id);
    Task<QualificationDetailDto> CreateAsync(QualificationCreateDto dto);
    Task<ServiceResult<QualificationDetailDto>> UpdateAsync(int id, QualificationUpdateDto dto);
    Task<ServiceResult> DeleteAsync(int id);
}
