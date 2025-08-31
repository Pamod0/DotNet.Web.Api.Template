using ASP.NET_Core_Identity.DTOs.Meeeting;
using ASP.NET_Core_Identity.Models.Decisions;
using ASP.NET_Core_Identity.Models.FileUploads;
using Task = System.Threading.Tasks.Task;

namespace ASP.NET_Core_Identity.Repositories.Interfaces
{
    public interface IMeetingRepository
    {
        Task<Meeting?> GetMeetingByIdAsync(Guid id, bool includeRelated = false);
        IQueryable<Meeting> GetAllMeetingsQueryable();
        Task<IEnumerable<Meeting>> GetAllMeetingsAsync();
        Task<IEnumerable<MeetingDropdownDto>> GetMeetingDropwdownAsync();
        Task<Meeting> AddMeetingAsync(Meeting meeting);
        Task UpdateMeetingAsync(Meeting meeting);
        Task DeleteMeetingAsync(Guid id);
        Task<bool> MeetingExists(Guid id);
        Task<int> SaveChangesAsync();

        // MeetingDepartment
        Task AddMeetingDepartmentAsync(MeetingDepartment meetingDepartment);
        Task AddMeetingDepartmentsAsync(ICollection<MeetingDepartment> meetingDepartments);
        void RemoveMeetingDepartments(IEnumerable<MeetingDepartment> meetingDepartments);
        void RemoveMeetingDepartments(ICollection<MeetingDepartment> meetingDepartments); // overloaded version
        Task<IEnumerable<MeetingDepartment>> GetMeetingDepartmentsByMeetingIdAsync(Guid meetingId);

        // SupportDocument
        Task AddSupportDocumentAsync(SupportDocument document);
        Task<SupportDocument?> GetSupportDocumentByIdAsync(Guid id);
        Task<IEnumerable<SupportDocument>> GetSupportDocumentsAsync(Guid meetingId);
        void RemoveSupportDocument(SupportDocument document);
        Task<Meeting?> GetLatestMeetingAsync();
    }
}
