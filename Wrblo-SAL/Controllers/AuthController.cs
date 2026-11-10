using Microsoft.AspNetCore.Mvc;

namespace Wrblo_SAL.Controllers;

public class AuthController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
