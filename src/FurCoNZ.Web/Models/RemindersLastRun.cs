using System;
using System.ComponentModel.DataAnnotations;

namespace FurCoNZ.Web.Models
{
    public class RemindersLastRun
    {
        [Key]
        public string ReminderService { get; set; }
        public DateTimeOffset LastRun { get; set; }
    }
}
