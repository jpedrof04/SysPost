using SysPost.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SysPost.Data;
using Microsoft.EntityFrameworkCore;

namespace SysPost.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;

        public AdminController(
            UserManager<Usuario> userManager,
            RoleManager<IdentityRole> roleManager,
            AppDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalUsuarios = _userManager.Users.Count();
            ViewBag.TotalAdmins = (await _userManager.GetUsersInRoleAsync("Admin")).Count;
            ViewBag.TotalUsers = (await _userManager.GetUsersInRoleAsync("User")).Count;

            var usuarios = _userManager.Users.ToList();
            var listaComRoles = new List<(Usuario Usuario, IList<string> Roles)>();
            foreach (var u in usuarios)
            {
                var roles = await _userManager.GetRolesAsync(u);
                listaComRoles.Add((u, roles));
            }
            ViewBag.UsuariosComRoles = listaComRoles;

            var posts = await _context.Posts
                .Include(p => p.Usuario)
                .OrderByDescending(p => p.DataCriacao)
                .ToListAsync();
            ViewBag.TodosPosts = posts;

            return View();
        }

        // ─── PROMOVER ──────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PromoverAdmin(string userId)
        {
            var usuario = await _userManager.FindByIdAsync(userId);
            if (usuario == null)
            {
                TempData["Erro"] = "Usuário não encontrado.";
                return RedirectToAction("Index");
            }

            if (!await _userManager.IsInRoleAsync(usuario, "Admin"))
            {
                await _userManager.RemoveFromRoleAsync(usuario, "User");
                await _userManager.AddToRoleAsync(usuario, "Admin");
                TempData["Sucesso"] = $"{usuario.NomeCompleto} agora é Administrador.";
            }
            else
            {
                TempData["Aviso"] = "Este usuário já é Administrador.";
            }

            return RedirectToAction("Index");
        }

        // ─── REBAIXAR ──────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RebaixarUser(string userId)
        {
            var usuario = await _userManager.FindByIdAsync(userId);
            if (usuario == null)
            {
                TempData["Erro"] = "Usuário não encontrado.";
                return RedirectToAction("Index");
            }

            var usuarioAtual = await _userManager.GetUserAsync(User);
            if (usuarioAtual?.Id == userId)
            {
                TempData["Erro"] = "Você não pode alterar sua própria role.";
                return RedirectToAction("Index");
            }

            if (await _userManager.IsInRoleAsync(usuario, "Admin"))
            {
                await _userManager.RemoveFromRoleAsync(usuario, "Admin");
                await _userManager.AddToRoleAsync(usuario, "User");
                TempData["Sucesso"] = $"{usuario.NomeCompleto} agora é Usuário comum.";
            }

            return RedirectToAction("Index");
        }

        // ─── DETALHES DO POST ──────────────────────────────────────

        public async Task<IActionResult> DetalhesPost(int id)
        {
            var post = await _context.Posts
                .Include(p => p.Usuario)
                .Include(p => p.Comentarios)
                    .ThenInclude(c => c.Usuario)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
                return NotFound();

            return View(post);
        }

        // ─── EXCLUIR COMENTÁRIO ────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExcluirComentario(int id, int postId)
        {
            var comment = await _context.Comments.FindAsync(id);

            if (comment == null)
            {
                TempData["Erro"] = "Comentário não encontrado.";
                return RedirectToAction("DetalhesPost", new { id = postId });
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Comentário removido com sucesso.";
            return RedirectToAction("DetalhesPost", new { id = postId });
        }

        // ─── EXCLUIR POST ──────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExcluirPost(int id)
        {
            var post = await _context.Posts.FindAsync(id);

            if (post == null)
            {
                TempData["Erro"] = "Post não encontrado.";
                return RedirectToAction("Index");
            }

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Post removido com sucesso.";
            return RedirectToAction("Index");
        }
    }
}
