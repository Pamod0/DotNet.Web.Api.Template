namespace DotNet.Web.Api.Template.DTOs.Meeeting
{
    public class MeetingDropdownDto
    {
        public Guid Id { get; set; }
        public DateOnly MeetingDate { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
