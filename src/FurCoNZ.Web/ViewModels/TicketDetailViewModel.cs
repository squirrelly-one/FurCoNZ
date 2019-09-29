using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using FurCoNZ.Web.Models;

namespace FurCoNZ.Web.ViewModels
{
    public class TicketDetailViewModel : IValidatableObject
    {
        public int Id { get; set; }

        [Required]
        public TicketTypeViewModel TicketType { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string EmailAddress { get; set; }

        [Display(Name = "Badge Name", Description = "This is the name displayed on your badge. This is typically your \"furry name\" or some other alias.")]
        public string BadgeName { get; set; }

        // Identification name is required for legal reasons, however this can be distressing in some circumstances.
        // We need to ensure this is never publically used if a preferred full name is supplied, and is only available to:
        // * FurcoNZ Staff, where necessary (such as registration)
        // * NZ officials, when legally required
        // * Where required for safety reasons (such as medical emergencies)
        [Display(Name = "Full Name (as it appears on your offical identification)")]
        public string IdentificationFullName { get; set; }

        // https://www.w3.org/International/questions/qa-personal-names
        [Display(Name = "Preferred Full Name")]
        public string PreferredFullName { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth", Description = "Please enter this as \"yyyy-mm-dd\". That way, it's less ambiguous for our international visitors!")]
        public DateTime DateOfBirth { get; set; }

        // TODO: A real dietary requirements field

        [DataType(DataType.MultilineText)]
        [Display(Name = "Known Allergies / Medical Conditions")]
        public string KnownAllergies { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Cabin Preferences")]
        public string CabinPreferences { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Other Notes")]
        public string OtherNotes { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return new List<ValidationResult>();
        }

        public TicketDetailViewModel()
        { }

        public TicketDetailViewModel(Ticket ticket)
        {
            Id = ticket.Id;

            // TODO: Ticket Type View Model?
            TicketType = new TicketTypeViewModel(ticket.TicketType);

            EmailAddress = ticket.EmailAddress;
            BadgeName = ticket.TicketName;
            PreferredFullName = ticket.PreferredName;

            DateOfBirth = ticket.DateOfBirth;

            KnownAllergies = ticket.KnownAllergens;
            CabinPreferences = ticket.CabinGrouping;

            OtherNotes = ticket.AdditionalNotes;
        }
    }
}
