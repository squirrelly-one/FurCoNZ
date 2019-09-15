using System.Collections.Generic;

namespace FurCoNZ.Web.Options
{
    public class ReminderServiceOptions
    {
        public int UnpaidOrderExpiryDays { get; set; }
        public IEnumerable<int> UnpaidOrderReminderDays { get; set; }
    }
}
