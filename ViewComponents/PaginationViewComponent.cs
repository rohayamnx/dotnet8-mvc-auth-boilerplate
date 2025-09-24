using Microsoft.AspNetCore.Mvc;
using dotnet8_mvc_auth_boilerplate.Models;

namespace dotnet8_mvc_auth_boilerplate.ViewComponents
{
    public class PaginationViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(PaginationInfo paginationInfo)
        {
            return View(paginationInfo);
        }
    }
}