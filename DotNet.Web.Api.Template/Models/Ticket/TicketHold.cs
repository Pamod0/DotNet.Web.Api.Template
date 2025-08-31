using System.ComponentModel.DataAnnotations;

namespace DotNet.Web.Api.Template.Models.Ticket
{
    public class TicketHold
    {
        [Key]
        public int Id { get; set; }
        public string OrderId { get; set; } = string.Empty;
        public string Hash { get; set; } = string.Empty;
        public string TicketId { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int RequestedCount { get; set; }
        public int HoldCount { get; set; }
        public DateTime HoldTime { get; set; }
        public bool IsConfirmed { get; set; }
        public bool IsReleased { get; set; }

        // Navigation property
        public Ticket? Ticket { get; set; }
    }
}
