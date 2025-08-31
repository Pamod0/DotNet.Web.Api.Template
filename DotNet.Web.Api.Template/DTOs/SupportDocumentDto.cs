namespace ASP.NET_Core_Identity.DTOs
{
    public class SupportDocumentDto
    {
        public Guid Id { get; set; }
        public required string FileName { get; init; }
        public required string FilePath { get; init; }
        public required string ContentType { get; init; }
        public required long FileSize { get; init; }
        public string? Description { get; init; }
    }
}
