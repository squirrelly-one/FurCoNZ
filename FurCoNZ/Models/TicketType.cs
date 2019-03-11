using System;
namespace FurCoNZ.Models
{
    public class TicketType
    {
        public TicketType()
        {
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int PriceCents { get; set; }
        public int TotalAvailable { get; set; }
    }
}
