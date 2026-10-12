using System.Diagnostics;
using EntrePaginas.Data;
using EntrePaginas.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EntrePaginas.Controllers;

public class HomeController : Controller
{
    private readonly EntrePaginasContext _context;
    public HomeController(EntrePaginasContext context) => _context = context;

    public async Task<IActionResult> Index()
    {
        var destacados = await _context.Libros.AsNoTracking()
            .Include(x => x.Categoria).Include(x => x.Escritor)
            .OrderByDescending(x => x.Id).Take(5).ToListAsync();
        return View(destacados);
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
