using DotNet.Web.Api.Template.Models.Decisions;
using DotNet.Web.Api.Template.Models.FileUploads;
using Task = System.Threading.Tasks.Task;

namespace DotNet.Web.Api.Template.Repositories.Interfaces
{
    public interface IDecisionRepository
    {
        Task<IEnumerable<Decision>> GetAllDecisionsAsync();
        IQueryable<Decision> GetAllDecisionsQueryable();
        IQueryable<Decision> GetAllCompletedDecisionsQueryable();
        Task<Decision?> GetDecisionByIdAsync(Guid id);
        Task AddDecisionAsync(Decision decision);
        System.Threading.Tasks.Task UpdateDecisionAsync(Decision decision);
        Task DeleteDecisionAsync(Guid id);
        Task<bool> DecisionExists(Guid id);
        Task<Decision?> GetDecisionWithTasksAsync(Guid decisionId);
        Task ProcessExpiredDeadlinesAsync();
        Task<SupportDocument?> GetSupportDocumentByIdAsync(Guid id);
        void RemoveSupportDocument(SupportDocument document);
        Task<int> SaveChangesAsync();
    }
}
