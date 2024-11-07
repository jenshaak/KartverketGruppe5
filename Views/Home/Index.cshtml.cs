using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KartverketGruppe5.Views.Home
{
    public class IndexModel : PageModel
    {
        public string UserName { get; set; }

        public void OnGet()
        {
            UserName = HttpContext.Session.GetString("UserName");
        }
    }
} 