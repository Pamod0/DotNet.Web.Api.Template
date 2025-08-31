using ASP.NET_Core_Identity.DTOs.Decision;
using ASP.NET_Core_Identity.DTOs.Meeeting;
using ASP.NET_Core_Identity.Models;

namespace ASP.NET_Core_Identity.Services.Interfaces
{
    public interface IMeetingService
    {
        Task<MeetingDto?> GetMeetingByIdAsync(Guid id);
        Task<PagedResponse<IEnumerable<MeetingDto>>> GetAllMeetingsAsync(PagedRequest request);
        Task<IEnumerable<MeetingDropdownDto>> GetMeetingDropdownAsync();
        Task<MeetingDto> CreateMeetingAsync(AddMeetingDto addMeetingDto);
        Task<bool> UpdateMeetingAsync(UpdateMeetingDto updateMeetingDto);
        Task<bool> DeleteMeetingAsync(Guid id);
        Task<bool> MeetingExistsAsync(Guid id);
        Task<MeetingDto?> GetLatestMeetingAsync();
        Task DeleteFileAsync(Guid fileId);
    }
}
