

namespace KartverketGruppe5.Models
{
    public class NotificationMessage
    {
        public string Message { get; set; }
        public string CssClasses { get; set; }

        public NotificationMessage(string message, string cssClasses)
        {   
            Message = message;
            CssClasses = cssClasses;
        }
    }
}
