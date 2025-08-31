namespace ASP.NET_Core_Identity.Configurations
{
    public class FileStorageSettings
    {
        public string UploadsRootPath { get; set; } = string.Empty;
        public string MeetingMinutesFolderName { get; set; } = "meeting-minutes";
    }
}
