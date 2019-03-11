using System;
namespace FurCoNZ.Models
{
    public class Ticket
    {
        public int Id { get; set; }

        public int AttendeeAccountId { get; set; }
        public virtual User AttendeeAccount { get; set; }

        public int OrderId { get; set; }
        public virtual Order Order { get; set; }

        public int TicketTypeId { get; set; }
        public virtual TicketType TicketType { get; set; }

        public string TicketName { get; set; }

        public string PreferredName { get; set; }
        public string PreferredPronouns { get; set; }

        public string LegalName { get; set; }
        public DateTime DateOfBirth { get; set; }

        public string EmailAddress { get; set; }

        public string Address { get; set; }
        public string Suburb { get; set; }
        public string CityTown { get; set; }
        public string Country { get; set; }

        public string PhoneNumber { get; set; }

        public FoodMenu MealRequirements { get; set; }
        public string KnownAllergens { get; set; }

        public string CabinGrouping { get; set; }
        public CabinActivityType CabinNoisePreference { get; set; }

        public string AdditionalNotes { get; set; }
    }

    /// <summary>
    /// Describes the type of menu that the attendee wishes for. This is heavily dependent on the venue.
    /// </summary>
    public enum FoodMenu
    {
        Regular = 0,
        Vegetarian = 1,
        Vegan = 2,
        DairyFree = 3,
        GlutenFree = 4
    }

    /// <summary>
    /// Describes the type of cabin that the attendee wishes for. This is heavily dependent on the venue.
    /// </summary>
    public enum CabinActivityType
    {
        NoPreference = 0,
        Social = 1,
        Quiet = 2
    }
}
