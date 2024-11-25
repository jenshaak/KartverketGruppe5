using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using KartverketGruppe5.Services.Interfaces;
using KartverketGruppe5.Models;
using System.Collections.Generic;
using System.Text.Json;

namespace KartverketGruppe5.Services
{
    /// <summary>
    /// Service for notifikasjoner
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITempDataDictionaryFactory _tempDataDictionaryFactory;

        private static readonly Dictionary<string, string> NotificationStyles = new()
        {
            { "Success", "bg-green-100 text-green-800 border border-green-800" },
            { "Error", "bg-red-100 text-red-800 border border-red-800" },
            { "Warning", "bg-yellow-100 text-yellow-800 border border-yellow-800" },
            { "Info", "bg-blue-100 text-blue-800 border border-blue-800" }
        };

        public NotificationService(
            IHttpContextAccessor httpContextAccessor,
            ITempDataDictionaryFactory tempDataDictionaryFactory)
        {
            _httpContextAccessor = httpContextAccessor;
            _tempDataDictionaryFactory = tempDataDictionaryFactory;
        }

        private ITempDataDictionary TempData
        {
            get
            {
                var context = _httpContextAccessor.HttpContext 
                    ?? throw new InvalidOperationException("HttpContext is not available");
                return _tempDataDictionaryFactory.GetTempData(context);
            }
        }

        public void AddSuccessMessage(string message)
        {
            var notification = new NotificationMessage(message, NotificationStyles["Success"]);
            TempData["Notification"] = JsonSerializer.Serialize(notification);
        }

        public void AddErrorMessage(string message)
        {
            var notification = new NotificationMessage(message, NotificationStyles["Error"]);
            TempData["Notification"] = JsonSerializer.Serialize(notification);
        }

        public void AddWarningMessage(string message)
        {
            var notification = new NotificationMessage(message, NotificationStyles["Warning"]);
            TempData["Notification"] = JsonSerializer.Serialize(notification);
        }

        public void AddInfoMessage(string message)
        {
            var notification = new NotificationMessage(message, NotificationStyles["Info"]);
            TempData["Notification"] = JsonSerializer.Serialize(notification);
        }
    }
} 