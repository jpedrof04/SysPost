using SysPost.Models;
using SysPost.ViewModels;
using SysPost.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SysPost.Data;
using Microsoft.EntityFrameworkCore;

namespace SysPost.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly AppDbContext _context;
        private readonly QueryMonitor _monitor;

        public HomeController(UserManager<Usuario> userManager, AppDbContext context, QueryMonitor monitor)
        {
            _userManager = userManager;
            _context = context;
            _monitor = monitor;
        }


        public async Task<IActionResult> Index(string? usuarioNome, TopicoPost? topico)
        {
            var query = _context.Posts
                .Include(p => p.Usuario)
                .Include(p => p.Comentarios)
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
            {
                ViewBag.DbQueries = _monitor.QueryCount;
                ViewBag.DbTimeMs = Math.Round(_monitor.TotalTimeMs, 2);
                ViewBag.DbLastMs = Math.Round(_monitor.LastQueryMs, 2);
                return View();
            }

            // USUÁRIO LOGADO
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                ViewBag.DbQueries = _monitor.QueryCount;
                ViewBag.DbTimeMs = Math.Round(_monitor.TotalTimeMs, 2);
                ViewBag.DbLastMs = Math.Round(_monitor.LastQueryMs, 2);
                return View();
            }

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

            ViewBag.DbQueries = _monitor.QueryCount;
            ViewBag.DbTimeMs = Math.Round(_monitor.TotalTimeMs, 2);
            ViewBag.DbLastMs = Math.Round(_monitor.LastQueryMs, 2);

            return View(vm);
        }
    }
}