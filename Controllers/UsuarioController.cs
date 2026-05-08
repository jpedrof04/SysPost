using SysPost.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace SysPost.Controllers;

[Authorize]
//area do usuario commum - acessivel por qualquer user autenticado
public class UsuarioController : Controller
{
    private readonly UserManager<Usuario> _userManager;

    public UsuarioController(UserManager<Usuario> userManager)
    {
        _userManager = userManager;
    }

    //DASHBOARD DO USUARIO --------------------------------------------

    public async Task<IActionResult> Index()
    {
        var usuario = await _userManager.GetUserAsync(User);
        if (usuario == null) return NotFound();
        
        return View(usuario);
    } 
}
