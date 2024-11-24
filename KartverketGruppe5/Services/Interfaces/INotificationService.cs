namespace KartverketGruppe5.Services.Interfaces
{
    public interface INotificationService
    {
        void AddSuccessMessage(string message);
        void AddErrorMessage(string message);
        void AddWarningMessage(string message);
        void AddInfoMessage(string message);
    } 
} 