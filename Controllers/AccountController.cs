using SysPost.Models;
using SysPost.ViewModels;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace SysPost.Controllers 
{
    //gerencia autenticação: login, registro, logout e perfil
    public class AccountController : Controller
    {

        private readonly UserManager<Usuario> _userManager;
        private readonly SignInManager<Usuario> _signInManager;
        private readonly IWebHostEnvironment _webHostEnviroment;

        public AccountController(
            UserManager<Usuario> userManager,
            SignInManager<Usuario> signInManager,
            IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _webHostEnviroment = webHostEnvironment;
        }

        private async Task<Usuario?> GetCurrentUserAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            return user;
        }

        private static (bool isValid, string? error) ValidatePhotoFile(IFormFile file)
        {
            var extensoesPermitidas = new[] { ".jpg", ".png", ".gif", ".jpeg", ".webp" };
            var extesao = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!extensoesPermitidas.Contains(extesao))
                return (false, "apenas imagens sao permitidas (.jpg, .png, .gif, .jpeg, .webp)");

            if (file.Length > 2 * 1024 * 1024)
                return (false, "a imagem deve ter no maximo 2MB");

            return (true, null);
        }

        //----------------LOGIN----------------------------

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("index", "Home");

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            var resultado = await _signInManager.PasswordSignInAsync(
                model.Email, model.Senha, isPersistent: model.LembrarMe, lockoutOnFailure: false);

            if (resultado.Succeeded)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Email ou senha invalidos");
            return View(model);
        }


        // ----- REGISTRO -------


        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            return View();
        }

 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var usuario = new Usuario
            {
                UserName = model.Email,
                Email = model.Email,
                NomeCompleto = model.NomeCompleto,
                DataCadastro = DateTime.UtcNow,
                EmailConfirmed = true
            };

            var resultado = await _userManager.CreateAsync(usuario, model.Senha);

            if (resultado.Succeeded)
            {
                await _userManager.AddToRoleAsync(usuario, "User"); //aq a role de Usuario simples é passada
                await _signInManager.SignInAsync(usuario, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var erro in resultado.Errors)
                ModelState.AddModelError(string.Empty, erro.Description);

            return View(model);
        }


        //logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        //perfil
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Perfil()
        {
            var usuario = await GetCurrentUserAsync();
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


        //editar perfil

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EditarPerfil()
        {
            var usuario = await GetCurrentUserAsync();
            if (usuario == null) return NotFound();

            var vm = new EditarPerfilViewModel
            {
                NomeCompleto = usuario.NomeCompleto,
                Bio = usuario.Bio,
                FotoAtual = usuario.FotoPerfil
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> EditarPerfil(EditarPerfilViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var usuario = await GetCurrentUserAsync();
            if (usuario == null) return NotFound();

            usuario.NomeCompleto = model.NomeCompleto;
            usuario.Bio = model.Bio;

            if (model.FotoArquivo != null && model.FotoArquivo.Length > 0)
            {
                var (isValid, error) = ValidatePhotoFile(model.FotoArquivo);
                if (!isValid)
                {
                    ModelState.AddModelError("FotoArquivo", error!);
                    model.FotoAtual = usuario.FotoPerfil;
                    return View(model);
                }

                using var ms = new MemoryStream();
                await model.FotoArquivo.CopyToAsync(ms);
                usuario.FotoPerfil = ms.ToArray();
            }

            await _userManager.UpdateAsync(usuario);

            TempData["Sucesso"] = "Perfil atualizado com sucesso!";
            return RedirectToAction("Perfil");

        }
    }

}