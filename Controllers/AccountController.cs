using learnfds.Models;
using learnfds.ViewModels;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace learnfds.Controllers
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
            var extensoesPermitidas = new[] { ".jpg", ".png", ".gif", ".webp"};
            var extesao = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!extensoesPermitidas.Contains(extesao))
                return (false, "apenas imagens sao permitidas (.jpg, .png, .gif, .webp)");
            
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

        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl= null)
        {
            ViewData["returnUrl"] = returnUrl;

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

            if(resultado.Succeeded)
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



        //perfil


        //editar perfil






    }
}

