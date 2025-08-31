using DotNet.Web.Api.Template.DTOs.Decision;
using DotNet.Web.Api.Template.Models;
using Microsoft.AspNetCore.JsonPatch;

namespace DotNet.Web.Api.Template.Services.Interfaces
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
