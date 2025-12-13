using Microsoft.AspNetCore.Mvc;

namespace TaskManagement.Api.Controllers
{
    public class ProjectController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
