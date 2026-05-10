using SysPost.Models;
using SysPost.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace SysPost.Controllers;

[Authorize]
public class UsuarioController : Controller
{
    private readonly UserManager<Usuario> _userManager;

    public UsuarioController(UserManager<Usuario> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var usuario = await _userManager.GetUserAsync(User);
        if (usuario == null) return NotFound();

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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarPerfil(EditarPerfilViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Erro"] = "Verifique os campos do formulário.";
            return RedirectToAction(nameof(Index));
        }

        var usuario = await _userManager.GetUserAsync(User);
        if (usuario == null) return NotFound();

        usuario.NomeCompleto = model.NomeCompleto;
        usuario.Bio = model.Bio;

        if (model.FotoArquivo != null && model.FotoArquivo.Length > 0)
        {
            var extensoesPermitidas = new[] { ".jpg", ".png", ".gif", ".jpeg", ".webp" };
            var ext = Path.GetExtension(model.FotoArquivo.FileName).ToLowerInvariant();

            if (!extensoesPermitidas.Contains(ext))
            {
                TempData["Erro"] = "Apenas imagens são permitidas (.jpg, .png, .gif, .jpeg, .webp).";
                return RedirectToAction(nameof(Index));
            }

            if (model.FotoArquivo.Length > 2 * 1024 * 1024)
            {
                TempData["Erro"] = "A imagem deve ter no máximo 2MB.";
                return RedirectToAction(nameof(Index));
            }

            using var ms = new MemoryStream();
            await model.FotoArquivo.CopyToAsync(ms);
            usuario.FotoPerfil = ms.ToArray();
        }

        await _userManager.UpdateAsync(usuario);
        TempData["Sucesso"] = "Perfil atualizado com sucesso!";
        return RedirectToAction(nameof(Index));
    }
}
