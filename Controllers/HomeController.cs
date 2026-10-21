using Microsoft.AspNetCore.Mvc;

namespace CafeManager.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Tables", "Order");
        }
        return RedirectToAction("Login", "Account");
    }
}
