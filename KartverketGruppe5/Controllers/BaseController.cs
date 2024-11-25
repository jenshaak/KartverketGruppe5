using Microsoft.AspNetCore.Mvc;
using KartverketGruppe5.Models.Helpers;
using KartverketGruppe5.Models.ViewModels;
using KartverketGruppe5.Models;
using KartverketGruppe5.Models.RequestModels;
using KartverketGruppe5.Models.Interfaces;
using KartverketGruppe5.Services.Interfaces;


/// <summary>
/// Base controller for alle controllers
/// </summary>
public abstract class BaseController : Controller
{
    protected readonly IInnmeldingService _innmeldingService;
    protected readonly ILogger<BaseController> _logger;
    protected readonly INotificationService _notificationService;

    public BaseController(IInnmeldingService innmeldingService, ILogger<BaseController> logger, INotificationService notificationService)
    {
        _innmeldingService = innmeldingService;
        _logger = logger;
        _notificationService = notificationService;
    }

    /// <summary>
    /// Setter opp visningsdata for innmeldinger
    /// </summary>
    protected void SetupViewData(string sortOrder, string statusFilter, string fylkeFilter, string kommuneFilter)
    {
        ViewData["DateSortParam"] = sortOrder == "date_asc" ? "date_desc" : "date_asc";
        ViewData["CurrentSort"] = sortOrder;
        ViewData["CurrentStatus"] = statusFilter;
        ViewData["CurrentFylke"] = fylkeFilter;
        ViewData["CurrentKommune"] = Request.Query["kommuneFilter"].ToString();
        ViewData["Statuses"] = InnmeldingHelper.GetAllStatuses();
    }

    /// <summary>
    /// Oppretter en tom paged result
    /// </summary>
    protected PagedResult<InnmeldingViewModel> CreateEmptyPagedResult()
    {
        return new PagedResult<InnmeldingViewModel>
        {
            Items = new List<InnmeldingViewModel>(),
            CurrentPage = 1,
            PageSize = 10,
            TotalItems = 0
        };
    }

    /// <summary>
    /// Henter innmeldinger basert på filtre og sortering
    /// </summary>
    protected async Task<IPagedResult<InnmeldingViewModel>> GetInnmeldinger(
            int? id, string sortOrder, string statusFilter, 
            string fylkeFilter, string kommuneFilter, int page, bool isBruker)
    {
        var request = new InnmeldingRequest
        {
            SortOrder = sortOrder,
            StatusFilter = statusFilter,
            FylkeFilter = fylkeFilter,
            KommuneFilter = kommuneFilter,
            Page = page
        };

        if (isBruker && id != null)
        {
            request.InnmelderBrukerId = id.Value;
        }
        else if (id != null)
        {
            request.SaksbehandlerId = id.Value;
        }

        return await _innmeldingService.GetInnmeldinger(request);
    }

    /// <summary>

    protected string SanitizeInput(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;

        input = System.Web.HttpUtility.HtmlEncode(input);
        
        input = input.Trim();
        
        input = System.Text.RegularExpressions.Regex.Replace(input, @"\s+", " ");
        
        return input;
    }

    /// <summary>
    /// Validerer input basert på maksimal lengde
    /// </summary>
    protected bool ValidateInput(string input, int? maxLength = 1000)
    {
        var sanitizedInput = SanitizeInput(input);

        if (string.IsNullOrEmpty(sanitizedInput))
        {
            ModelState.AddModelError("", "Tekst-feltet er påkrevd");
            _notificationService.AddErrorMessage("Tekst-feltet er påkrevd");
            return false;
        }
        
        if (input.Length > maxLength)
        {
            ModelState.AddModelError("", $"Tekst-feltet er for langt (max {maxLength} tegn)");
            _notificationService.AddErrorMessage($"Tekst-feltet er for langt (max {maxLength} tegn)");
            return false;
        }

        return true;
    }
} 