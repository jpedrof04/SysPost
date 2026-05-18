using SysPost.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SysPost.Data;
using Microsoft.EntityFrameworkCore;

namespace SysPost.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
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
                if (!roles.Contains("SuperAdmin"))
                    listaComRoles.Add((u, roles));
            }
            ViewBag.UsuariosComRoles = listaComRoles;
            ViewBag.EhSuperAdmin = User.IsInRole("SuperAdmin");

            var posts = await _context.Posts
                .Include(p => p.Usuario)
                .Include(p => p.Comentarios)
                    .ThenInclude(c => c.Usuario)
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

            var roles = await _userManager.GetRolesAsync(usuario);
            if (roles.Contains("SuperAdmin"))
            {
                TempData["Erro"] = "Não é possível alterar um Super Admin.";
                return RedirectToAction("Index");
            }

            if (!roles.Contains("Admin"))
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

            var roles = await _userManager.GetRolesAsync(usuario);
            if (roles.Contains("SuperAdmin"))
            {
                TempData["Erro"] = "Não é possível alterar um Super Admin.";
                return RedirectToAction("Index");
            }

            var usuarioAtual = await _userManager.GetUserAsync(User);
            if (usuarioAtual?.Id == userId)
            {
                TempData["Erro"] = "Você não pode alterar sua própria role.";
                return RedirectToAction("Index");
            }

            if (roles.Contains("Admin"))
            {
                await _userManager.RemoveFromRoleAsync(usuario, "Admin");
                await _userManager.AddToRoleAsync(usuario, "User");
                TempData["Sucesso"] = $"{usuario.NomeCompleto} agora é Usuário comum.";
            }

            return RedirectToAction("Index");
        }

        // ─── EXCLUIR USUÁRIO (apenas SuperAdmin) ────────────────────

        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExcluirUsuario(string userId)
        {
            var usuario = await _userManager.FindByIdAsync(userId);
            if (usuario == null)
            {
                TempData["Erro"] = "Usuário não encontrado.";
                return RedirectToAction("Index");
            }

            var roles = await _userManager.GetRolesAsync(usuario);
            if (roles.Contains("SuperAdmin"))
            {
                TempData["Erro"] = "Não é possível excluir um Super Admin.";
                return RedirectToAction("Index");
            }

            var usuarioAtual = await _userManager.GetUserAsync(User);
            if (usuarioAtual?.Id == userId)
            {
                TempData["Erro"] = "Você não pode excluir a si mesmo.";
                return RedirectToAction("Index");
            }

            var posts = await _context.Posts
                .Include(p => p.Comentarios)
                .Where(p => p.UsuarioId == userId)
                .ToListAsync();

            foreach (var post in posts)
            {
                _context.Comments.RemoveRange(post.Comentarios);
            }
            _context.Posts.RemoveRange(posts);

            var comments = await _context.Comments
                .Where(c => c.UsuarioId == userId)
                .ToListAsync();
            _context.Comments.RemoveRange(comments);

            await _context.SaveChangesAsync();

            var resultado = await _userManager.DeleteAsync(usuario);
            if (resultado.Succeeded)
                TempData["Sucesso"] = $"Usuário {usuario.NomeCompleto} foi excluído.";
            else
                TempData["Erro"] = "Erro ao excluir usuário.";

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
            var post = await _context.Posts
                .Include(p => p.Comentarios)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
            {
                TempData["Erro"] = "Post não encontrado.";
                return RedirectToAction("Index");
            }

            _context.Comments.RemoveRange(post.Comentarios);
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Post removido com sucesso.";
            return RedirectToAction("Index");
        }
    }
}
