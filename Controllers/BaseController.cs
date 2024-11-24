using Microsoft.AspNetCore.Mvc;
using KartverketGruppe5.Models.Helpers;
public abstract class BaseController : Controller
{
    protected void SetupViewData(string sortOrder, string statusFilter, string fylkeFilter, string kommuneFilter)
    {
        ViewData["DateSortParam"] = sortOrder == "date_asc" ? "date_desc" : "date_asc";
        ViewData["CurrentSort"] = sortOrder;
        ViewData["CurrentStatus"] = statusFilter;
        ViewData["CurrentFylke"] = fylkeFilter;
        ViewData["CurrentKommune"] = Request.Query["kommuneFilter"].ToString();
        ViewData["Statuses"] = InnmeldingHelper.GetAllStatuses();
    }
} 