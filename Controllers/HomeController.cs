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


        public async Task<IActionResult> Index(string? usuarioNome, TopicoPost? topico)
        {
            var query = _context.Posts
                .Include(p => p.Usuario)
                .AsQueryable();

            if (!string.IsNullOrEmpty(usuarioNome))
                query = query.Where(p => p.Usuario.NomeCompleto.Contains(usuarioNome));

            if (topico.HasValue)
                query = query.Where(p => p.Topico == topico.Value);

            var posts = await query
                .OrderByDescending(p => p.DataCriacao)
                .ToListAsync();

            ViewBag.Posts = posts;
            ViewBag.FiltroUsuarioNome = usuarioNome;
            ViewBag.FiltroTopico = topico;

            // VISITANTE
            if (User.Identity?.IsAuthenticated != true)
                return View();

            // USUÁRIO LOGADO
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
                return View();

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

            return View(vm);
        }
    }
}