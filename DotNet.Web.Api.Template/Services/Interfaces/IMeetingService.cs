using DotNet.Web.Api.Template.DTOs.Decision;
using DotNet.Web.Api.Template.DTOs.Meeeting;
using DotNet.Web.Api.Template.Models;

namespace DotNet.Web.Api.Template.Services.Interfaces
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
