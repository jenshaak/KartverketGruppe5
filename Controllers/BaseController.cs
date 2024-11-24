using Microsoft.AspNetCore.Mvc;
using KartverketGruppe5.Models.Helpers;
using KartverketGruppe5.Models.ViewModels;
using KartverketGruppe5.Models;
using KartverketGruppe5.Models.RequestModels;
using KartverketGruppe5.Models.Interfaces;
using KartverketGruppe5.Services.Interfaces;

public abstract class BaseController : Controller
{
    protected readonly IInnmeldingService _innmeldingService;
    protected readonly ILogger<BaseController> _logger;

    public BaseController(IInnmeldingService innmeldingService, ILogger<BaseController> logger)
    {
        _innmeldingService = innmeldingService;
        _logger = logger;
    }

    protected void SetupViewData(string sortOrder, string statusFilter, string fylkeFilter, string kommuneFilter)
    {
        ViewData["DateSortParam"] = sortOrder == "date_asc" ? "date_desc" : "date_asc";
        ViewData["CurrentSort"] = sortOrder;
        ViewData["CurrentStatus"] = statusFilter;
        ViewData["CurrentFylke"] = fylkeFilter;
        ViewData["CurrentKommune"] = Request.Query["kommuneFilter"].ToString();
        ViewData["Statuses"] = InnmeldingHelper.GetAllStatuses();
    }

    
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
} 