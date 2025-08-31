using System.ComponentModel.DataAnnotations;

namespace DotNet.Web.Api.Template.Models.Ticket
{
    public class Ticket
    {
        [Key]
        public string TicketId { get; set; } = string.Empty;
        public string DestinationCode { get; set; } = string.Empty;
        public DateTime ValidityStart { get; set; }
        public DateTime ValidityEnd { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public bool Active { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
