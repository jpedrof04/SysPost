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


        public async Task<IActionResult> Index(string? usuarioNome, TopicoPost? topico, string? ordenar)
        {
            var query = _context.Posts
                .Include(p => p.Usuario)
                .Include(p => p.Comentarios)
                    .ThenInclude(c => c.Usuario)
                .Include(p => p.Likes)
                .AsQueryable();

            if (!string.IsNullOrEmpty(usuarioNome))
                query = query.Where(p => p.Usuario.NomeCompleto.Contains(usuarioNome));

            if (topico.HasValue)
                query = query.Where(p => p.Topico == topico.Value);

            IOrderedQueryable<Post> ordered;
            if (ordenar == "likes")
                ordered = query.OrderByDescending(p => p.Likes.Count);
            else
                ordered = query.OrderByDescending(p => p.DataCriacao);

            var posts = await ordered.ToListAsync();

            ViewBag.Posts = posts;
            ViewBag.FiltroUsuarioNome = usuarioNome;
            ViewBag.FiltroTopico = topico;
            ViewBag.FiltroOrdenar = ordenar;

            // VISITANTE
            if (User.Identity?.IsAuthenticated == true)
            {
                var usuarioLogado = await _userManager.GetUserAsync(User);
                if (usuarioLogado != null)
                    ViewBag.CurrentUserId = usuarioLogado.Id;
            }

            ViewBag.DbQueries = _monitor.QueryCount;
            ViewBag.DbTimeMs = Math.Round(_monitor.TotalTimeMs, 2);
            ViewBag.DbLastMs = Math.Round(_monitor.LastQueryMs, 2);

            return View();
        }
    }
}