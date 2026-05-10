using SysPost.Models;
using SysPost.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SysPost.Data;
using Microsoft.EntityFrameworkCore;

namespace SysPost.Controllers
{
    /// <summary>
    /// Página inicial da aplicação.
    /// Se o usuário estiver logado, exibe informações personalizadas por perfil.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly AppDbContext _context;

        public HomeController(UserManager<Usuario> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Se não está logado, exibe a home pública
            if (User.Identity?.IsAuthenticated != true)
                return View();

            // Se está logado, busca dados do usuário e passa para a view
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null) return View();

            var roles = await _userManager.GetRolesAsync(usuario);

            var vm = new PerfilViewModel
            {
                NomeCompleto = usuario.NomeCompleto,
                Email = usuario.Email ?? "",
                Bio = usuario.Bio,
                FotoPerfil = usuario.FotoPerfil,
                DataCadastro = usuario.DataCadastro,
                Perfil = roles.FirstOrDefault() ?? "User"
            };

            var posts = await _context.Posts
            .Include(p => p.Usuario)
            .OrderByDescending(p => p.DataCriacao)
            .ToListAsync();

            ViewBag.Posts = posts;

            return View(vm);
        }
    }
}