using SysPost.Data;
using SysPost.Models;
using SysPost.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SysPost.Controllers
{
    [Authorize]
    public class PostController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Usuario> _userManager;

        public PostController(
            AppDbContext context,
            UserManager<Usuario> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Criar()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar(CriarPostViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
                return RedirectToAction("Login", "Account");

            byte[]? imagemBytes = null;

            if (model.ImagemArquivo != null)
            {
                var extensoesPermitidas = new[]
                {
                    ".jpg", ".jpeg", ".png", ".webp"
                };

                var extensao = Path.GetExtension(model.ImagemArquivo.FileName).ToLower();

                if (!extensoesPermitidas.Contains(extensao))
                {
                    ModelState.AddModelError("", "Imagem inválida.");
                    return View(model);
                }

                if (model.ImagemArquivo.Length > 2 * 1024 * 1024)
                {
                    ModelState.AddModelError("", "Máximo 2MB.");
                    return View(model);
                }

                using var ms = new MemoryStream();

                await model.ImagemArquivo.CopyToAsync(ms);

                imagemBytes = ms.ToArray();
            }

            var post = new Post
            {
                Titulo = model.Titulo,
                Descricao = model.Descricao,
                Topico = model.Topico,
                Imagem = imagemBytes,
                UsuarioId = usuario.Id
            };

            _context.Posts.Add(post);

            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Post criado!";

            return RedirectToAction("Index", "Home");
        }
    }
}