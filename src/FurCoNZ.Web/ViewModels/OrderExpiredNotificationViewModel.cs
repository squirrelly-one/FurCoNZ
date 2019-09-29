using System;
namespace FurCoNZ.Web.ViewModels
{
    public class OrderExpiredNotificationViewModel
    {
        public int Id { get; internal set; }
        public DateTimeOffset CreatedAt { get; internal set; }
        public string Name { get; internal set; }
        public string Email { get; internal set; }
    }
}
