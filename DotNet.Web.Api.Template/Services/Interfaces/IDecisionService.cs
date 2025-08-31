using ASP.NET_Core_Identity.DTOs.Decision;
using ASP.NET_Core_Identity.Models;
using Microsoft.AspNetCore.JsonPatch;

namespace ASP.NET_Core_Identity.Services.Interfaces
{
    public interface IDecisionService
    {
        Task<DecisionDto?> GetDecisionByIdAsync(Guid id);
        Task<PagedResponse<IEnumerable<DecisionDto>>> GetAllDecisionsAsync(PagedRequest request);
        Task<PagedResponse<IEnumerable<DecisionDto>>> GetAllCompletedDecisionsAsync(PagedRequest request);
        Task<DecisionDto> CreateDecisionAsync(AddDecisionDto addDecisionDto);
        Task<bool> UpdateDecisionAsync(Guid id, DecisionUpdateDto decisionUpdateDto);
        Task<bool> UpdateDecisionPartialAsync(Guid id, JsonPatchDocument<UpdateDecisionDto> patchDoc);
        Task<bool> DeleteDecisionAsync(Guid id);
        Task<TaskCompletionDto?> GetTaskCompletionForDecisionAsync(Guid decisionId);
        Task DeleteFileAsync(Guid fileId);
    }
}
