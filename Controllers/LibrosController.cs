using Microsoft.AspNetCore.Mvc;

namespace EntrePaginas.Controllers
{
    public class LibrosController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}