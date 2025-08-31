using System.ComponentModel.DataAnnotations;

namespace DotNet.Web.Api.Template.DTOs.Decision
{
    public class CreateMeetingDTO
    {
        [Required(ErrorMessage = "Meeting Date is required.")]
        public DateOnly MeetingDate { get; set; }

        [Required(ErrorMessage = "Start Time is required.")]
        public TimeOnly StartTime { get; set; }

        [Required(ErrorMessage = "End Time is required.")]
        public TimeOnly EndTime { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string Description { get; set; }

        // List of Department IDs that the meeting pertains to
        // The client will send IDs, not full Department objects.
        public List<string> DepartmentIds { get; set; } = new List<string>();

        // For file uploads, the DTO for initial creation typically doesn't directly
        // contain the file content. Instead, the API endpoint will receive IFormFile(s)
        // separately or as part of a multipart/form-data request.
        // The URL for MeetingMinutes is populated AFTER the file is uploaded.
        // So, this field might not be in the initial Add DTO, or it might be optional
        // if the file upload is a separate step or can be done later.
        // For simplicity, let's assume the file is uploaded later or its URL isn't
        // part of the initial POST body, but sent via a separate mechanism (e.g., multipart form-data).
        // If you were to send the URL as part of the initial POST, it would look like this:
        // public string MeetingMinutesUrl { get; set; } // Optional if file upload is a separate step

        public bool SendNotificationToParticipants { get; set; }
    }
}
