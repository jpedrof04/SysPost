using learnfds.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace fds.Controllers
{
    /// <summary>
    /// Área administrativa — acessível somente por usuários com a role "Admin".
    /// O atributo [Authorize(Roles = "Admin")] na controller bloqueia todas as actions.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(
            UserManager<Usuario> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // ─── DASHBOARD ────────────────────────────────────────────────────────

        public async Task<IActionResult> Index()
        {
            // Conta totais para o painel
            ViewBag.TotalUsuarios = _userManager.Users.Count();
            ViewBag.TotalAdmins = (await _userManager.GetUsersInRoleAsync("Admin")).Count;
            ViewBag.TotalUsers = (await _userManager.GetUsersInRoleAsync("User")).Count;

            return View();
        }

        // ─── LISTA DE USUÁRIOS ────────────────────────────────────────────────

        public async Task<IActionResult> Usuarios()
        {
            var usuarios = _userManager.Users.ToList();

            // Para cada usuário, descobrir suas roles
            var listaComRoles = new List<(Usuario Usuario, IList<string> Roles)>();

            foreach (var u in usuarios)
            {
                var roles = await _userManager.GetRolesAsync(u);
                listaComRoles.Add((u, roles));
            }

            return View(listaComRoles);
        }

        // ─── PROMOVER PARA ADMIN ──────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PromoverAdmin(string userId)
        {
            var usuario = await _userManager.FindByIdAsync(userId);
            if (usuario == null)
            {
                TempData["Erro"] = "Usuário não encontrado.";
                return RedirectToAction("Usuarios");
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

            return RedirectToAction("Usuarios");
        }

        // ─── REBAIXAR PARA USER ───────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RebaixarUser(string userId)
        {
            var usuario = await _userManager.FindByIdAsync(userId);
            if (usuario == null)
            {
                TempData["Erro"] = "Usuário não encontrado.";
                return RedirectToAction("Usuarios");
            }

            // Impede rebaixar a si mesmo
            var usuarioAtual = await _userManager.GetUserAsync(User);
            if (usuarioAtual?.Id == userId)
            {
                TempData["Erro"] = "Você não pode alterar sua própria role.";
                return RedirectToAction("Usuarios");
            }

            if (await _userManager.IsInRoleAsync(usuario, "Admin"))
            {
                await _userManager.RemoveFromRoleAsync(usuario, "Admin");
                await _userManager.AddToRoleAsync(usuario, "User");
                TempData["Sucesso"] = $"{usuario.NomeCompleto} agora é Usuário comum.";
            }

            return RedirectToAction("Usuarios");
        }
    }
}