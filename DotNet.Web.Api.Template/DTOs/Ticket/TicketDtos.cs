namespace DotNet.Web.Api.Template.DTOs.Ticket
{
    public class TicketDtos
    {
        // Price Sync DTOs
        public class PriceSyncRequest
        {
            public List<string> TicketId { get; set; } = new();
        }

        public class PriceSyncResponse
        {
            public int Status { get; set; }
            public List<TicketPriceInfo> Tickets { get; set; } = new();
        }

        public class TicketPriceInfo
        {
            public string TicketId { get; set; } = string.Empty;
            public string? ErrorCode { get; set; }
            public string Msg { get; set; } = string.Empty;
            public string? DestinationCode { get; set; }
            public string? ValidityStart { get; set; }
            public string? ValidityEnd { get; set; }
            public string? CurrencyCode { get; set; }
            public string? Price { get; set; }
            public string? Qty { get; set; }
            public string? Active { get; set; }
        }

        // Ticket Hold DTOs
        public class TicketHoldRequest
        {
            public string OrderId { get; set; } = string.Empty;
            public string TotalRequested { get; set; } = string.Empty;
            public List<TicketHoldItem> Tickets { get; set; } = new();
        }

        public class TicketHoldItem
        {
            public string Date { get; set; } = string.Empty;
            public string TicketId { get; set; } = string.Empty;
            public string Count { get; set; } = string.Empty;
        }

        public class TicketHoldResponse
        {
            public string? ErrorCode { get; set; }
            public int Status { get; set; }
            public string Msg { get; set; } = string.Empty;
            public string OrderId { get; set; } = string.Empty;
            public string Hash { get; set; } = string.Empty;
            public string TotalRequested { get; set; } = string.Empty;
            public string TotalHold { get; set; } = string.Empty;
            public List<TicketHoldResponseItem> Tickets { get; set; } = new();
        }

        public class TicketHoldResponseItem
        {
            public string Date { get; set; } = string.Empty;
            public string TicketId { get; set; } = string.Empty;
            public string Requested { get; set; } = string.Empty;
            public string Hold { get; set; } = string.Empty;
        }

        // Ticket Confirmation DTOs
        public class TicketConfirmationRequest
        {
            public string OrderId { get; set; } = string.Empty;
            public string Hash { get; set; } = string.Empty;
            public string PaymentStatus { get; set; } = string.Empty;
        }

        public class TicketConfirmationResponse
        {
            public int Status { get; set; }
            public string OrderId { get; set; } = string.Empty;
            public string? ErrorCode { get; set; }
            public string Msg { get; set; } = string.Empty;
            public string? TotalConfirm { get; set; }
            public List<ConfirmedTicketItem>? Tickets { get; set; }
            public string? TotalReleased { get; set; }
            public List<ReleasedTicketItem>? ReleasedTickets { get; set; }
        }

        public class ConfirmedTicketItem
        {
            public string TicketId { get; set; } = string.Empty;
            public string Date { get; set; } = string.Empty;
            public string Confirm { get; set; } = string.Empty;
            public List<string> Serials { get; set; } = new();
        }

        public class ReleasedTicketItem
        {
            public string TicketId { get; set; } = string.Empty;
            public string Date { get; set; } = string.Empty;
            public string Released { get; set; } = string.Empty;
        }
    }
}
