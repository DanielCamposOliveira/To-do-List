using Microsoft.AspNetCore.Mvc;

namespace Front_End.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
