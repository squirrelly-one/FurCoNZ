using System;
namespace FurCoNZ.Web.Models
{
    public class TicketType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int PriceCents { get; set; }
        public int TotalAvailable { get; set; }
        public DateTimeOffset SoldOutAt { get; set; }
        public bool HiddenFromPublic { get; set; }
    }
}
