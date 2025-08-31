using ASP.NET_Core_Identity.Data;
using ASP.NET_Core_Identity.DTOs.Meeeting;
using ASP.NET_Core_Identity.Models.Decisions;
using ASP.NET_Core_Identity.Models.FileUploads;
using ASP.NET_Core_Identity.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace ASP.NET_Core_Identity.Repositories
{
    public class MeetingRepository : IMeetingRepository
    {
        private readonly ApplicationDbContext _context;

        public MeetingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Meeting?> GetMeetingByIdAsync(Guid id, bool includeRelated = false)
        {
            IQueryable<Meeting> query = _context.Meetings;

            if (includeRelated)
            {
                query = query
                    .Include(m => m.SupportDocuments)
                    .Include(m => m.MeetingDepartments)
                        .ThenInclude(md => md.Department)
                    .Include(m => m.Decisions);
            }

            return await query.FirstOrDefaultAsync(m => m.Id == id);
        }



        public IQueryable<Meeting> GetAllMeetingsQueryable()
        {
            return _context.Meetings
                          .Where(m => !m.IsDeleted)
                          .Include(m => m.MeetingDepartments)
                              .ThenInclude(md => md.Department)
                          .Include(m => m.SupportDocuments)
                          .AsQueryable();
        }

        public async Task<IEnumerable<Meeting>> GetAllMeetingsAsync()
        {
            return await _context.Meetings
                          .Where(m => !m.IsDeleted)
                          .Include(m => m.MeetingDepartments)
                              .ThenInclude(md => md.Department)
                          .Include(m => m.SupportDocuments)
                           .ToListAsync();
        }

        public async Task<IEnumerable<MeetingDropdownDto>> GetMeetingDropwdownAsync()
        {
            return await _context.Meetings
                          .Where(m => !m.IsDeleted)
                          .OrderByDescending(m => m.MeetingDate)
                          .Select(m => new MeetingDropdownDto
                          {
                              Id = m.Id,
                              MeetingDate = m.MeetingDate,
                              Description = m.Description
                          })
                          .ToListAsync();
        }

        public async Task<Meeting> AddMeetingAsync(Meeting meeting)
        {
            _context.Meetings.Add(meeting);
            await _context.SaveChangesAsync();
            return meeting;
        }

        public async Task UpdateMeetingAsync(Meeting meeting)
        {
            _context.Entry(meeting).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteMeetingAsync(Guid id)
        {
            var meeting = await _context.Meetings.FindAsync(id);
            try
            {
                if (meeting != null)
                {
                    _context.Meetings.Remove(meeting);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                // Log the exception (you can use any logging framework you prefer)
                Console.WriteLine($"Error deleting meeting: {ex.Message}");
                throw; // Re-throw the exception after logging it
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<bool> MeetingExists(Guid id)
        {
            return await _context.Meetings.AnyAsync(e => e.Id == id);
        }

        public async Task AddMeetingDepartmentAsync(MeetingDepartment meetingDepartment)
        {
            await _context.MeetingDepartments.AddAsync(meetingDepartment);
        }

        public void RemoveMeetingDepartments(IEnumerable<MeetingDepartment> meetingDepartments)
        {
            _context.MeetingDepartments.RemoveRange(meetingDepartments);
        }

        public async Task<IEnumerable<MeetingDepartment>> GetMeetingDepartmentsByMeetingIdAsync(Guid meetingId)
        {
            return await _context.MeetingDepartments
                                 .Where(md => md.MeetingId == meetingId)
                                 .ToListAsync();
        }

        public async Task AddSupportDocumentAsync(SupportDocument document)
        {
            await _context.SupportDocuments.AddAsync(document);
        }

        public async Task<SupportDocument?> GetSupportDocumentByIdAsync(Guid id)
        {
            return await _context.SupportDocuments.FindAsync(id);
        }

        public async Task<IEnumerable<SupportDocument>> GetSupportDocumentsAsync(Guid meetingId)
        {
            return await _context.SupportDocuments
                                 .Where(sd => sd.MeetingId == meetingId && !sd.IsDeleted)
                                 .ToListAsync();
        }

        public void RemoveSupportDocument(SupportDocument document)
        {
            _context.SupportDocuments.Remove(document);
        }

        // MeetingDepartment specific methods
        public void RemoveMeetingDepartments(ICollection<MeetingDepartment> meetingDepartments)
        {
            _context.MeetingDepartments.RemoveRange(meetingDepartments);
        }

        public async Task AddMeetingDepartmentsAsync(ICollection<MeetingDepartment> meetingDepartments)
        {
            await _context.MeetingDepartments.AddRangeAsync(meetingDepartments);
        }
        public async Task<Meeting?> GetLatestMeetingAsync()
        {
            return await _context.Meetings
                .Where(m => !m.IsDeleted)
                .OrderBy(m => m.MeetingDate)
                .ThenByDescending(m => m.StartTime)
                .FirstOrDefaultAsync();
        }
    }
}
