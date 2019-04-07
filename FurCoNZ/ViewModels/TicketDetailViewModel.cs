using System;
using FurCoNZ.Models;

namespace FurCoNZ.ViewModels
{
    public class TicketDetailViewModel
    {
        public int Id { get; set; }
        public TicketType TicketType { get; set; } // TODO: Probably need to map this to a ViewModel
        public string EmailAddress { get; set; }
        public bool SendTicketToOtherAccount { get; set; }
        public string BadgeName { get; set; }

        // https://www.w3.org/International/questions/qa-personal-names
        public string PreferredFullName { get; set; }
        public string PreferredKnownAs { get; set; }

        // Legal name is required for legal reasons, however this can be distressing in some circumstances.
        // We need to ensure this is never publically used, and is only available to:
        // * FurcoNZ Staff, where necessary (such as registration)
        // * NZ officials, when legally required
        // * Where required for safety reasons (such as medical emergencies)
        public string LegalFullName { get; set; }

        public DateTime DateOfBirth { get; set; }
        public string KnownAllergies { get; set; }
        public string OtherNotes { get; set; }
    }
}