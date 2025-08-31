using System.ComponentModel.DataAnnotations;

namespace DotNet.Web.Api.Template.Models.Ticket
{
    public class TicketSerial
    {
        [Key]
        public int Id { get; set; }
        public string SerialNumber { get; set; } = string.Empty;
        public string TicketId { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string? OrderId { get; set; }
        public bool IsUsed { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation property
        public Ticket? Ticket { get; set; }
    }
}
