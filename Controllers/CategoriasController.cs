using EntrePaginas.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EntrePaginas.Controllers;

[Authorize(Roles = RolesAplicacion.Administrador)]
public class CategoriasController : Controller
{
    [HttpGet]
    public IActionResult Index() => View();
}
