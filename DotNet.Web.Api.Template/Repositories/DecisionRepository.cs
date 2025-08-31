using DotNet.Web.Api.Template.Data;
using DotNet.Web.Api.Template.Models.Decisions;
using DotNet.Web.Api.Template.Models.FileUploads;
using DotNet.Web.Api.Template.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace DotNet.Web.Api.Template.Repositories
{
    public class DecisionRepository : IDecisionRepository
    {
        private readonly ApplicationDbContext _context;

        public DecisionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Decision>> GetAllDecisionsAsync()
        {
            return await _context.Decisions
                                 .Include(d => d.Tasks)
                                     .ThenInclude(t => t.TaskDepartments)
                                        .ThenInclude(td => td.Department)
                                 .Include(d => d.SupportDocuments)
                                 .ToListAsync();
        }
        public IQueryable<Decision> GetAllDecisionsQueryable()
        {
            return _context.Decisions
                           .Where(d => d.Status == DecisionStatus.Pending || 
                                    d.Status == DecisionStatus.InProgress || 
                                    d.Status == DecisionStatus.Overdue)
                           .Include(d => d.Tasks)
                               .ThenInclude(t => t.TaskDepartments)
                                   .ThenInclude(td => td.Department)
                           .Include(d => d.SupportDocuments)
                           .Include(d => d.DecisionDepartments)
                               .ThenInclude(dd => dd.Department)
                           .AsQueryable();
        }

        public IQueryable<Decision> GetAllCompletedDecisionsQueryable()
        {
            return _context.Decisions
                           .Where(d => d.Status == DecisionStatus.Completed)
                           .Include(d => d.Tasks)
                               .ThenInclude(t => t.TaskDepartments)
                                   .ThenInclude(td => td.Department)
                           .Include(d => d.SupportDocuments)
                           .Include(d => d.DecisionDepartments)
                               .ThenInclude(dd => dd.Department)
                           .AsQueryable();
        }

        public async Task<Decision?> GetDecisionByIdAsync(Guid id)
        {
            return await _context.Decisions
                                 .Include(d => d.Tasks)
                                     .ThenInclude(t => t.TaskDepartments)
                                        .ThenInclude(td => td.Department)
                                 .Include(d => d.SupportDocuments)
                                 .Include(d => d.DecisionDepartments)
                                     .ThenInclude(dd => dd.Department)
                                 .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task AddDecisionAsync(Decision decision)
        {
            _context.Decisions.Add(decision);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateDecisionAsync(Decision decision)
        {
            _context.Entry(decision).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteDecisionAsync(Guid id)
        {
            var decision = await _context.Decisions.FindAsync(id);
            if (decision != null)
            {
                _context.Decisions.Remove(decision);
                await _context.SaveChangesAsync();
            }
        }

        public async Task SoftDeleteDecisionAsync(Guid id)
        {
            var decision = await _context.Decisions.FindAsync(id);

            if (decision != null)
            {
                decision.IsDeleted = true;

                // Mark the entity as modified
                _context.Entry(decision).State = EntityState.Modified;

                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> DecisionExists(Guid id)
        {
            return await _context.Decisions.AnyAsync(e => e.Id == id);
        }

        public async Task<Decision?> GetDecisionWithTasksAsync(Guid decisionId)
        {
            return await _context.Decisions
                                 .Include(d => d.Tasks)
                                 .FirstOrDefaultAsync(d => d.Id == decisionId);
        }

        public async Task ProcessExpiredDeadlinesAsync()
        {
            var expiredDecisions = await _context.Decisions
                .Where(d => d.Deadline < DateTime.UtcNow && d.Status != DecisionStatus.Overdue)
                .ToListAsync();

            foreach (var decision in expiredDecisions)
            {
                // Update the decision status
                decision.Status = DecisionStatus.Overdue;
                _context.Update(decision);

                // Trigger notification
                //await _notificationService.NotifyDeadlineExpiredAsync(decision);
            }

            if (expiredDecisions.Any())
            {
                await _context.SaveChangesAsync();
            }
        }

        public async Task<SupportDocument?> GetSupportDocumentByIdAsync(Guid id)
        {
            return await _context.SupportDocuments.FindAsync(id);
        }

        public void RemoveSupportDocument(SupportDocument document)
        {
            _context.SupportDocuments.Remove(document);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
