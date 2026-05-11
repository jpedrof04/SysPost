using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SysPost.Data;
using SysPost.Models;
using SysPost.ViewModels;

namespace SysPost.Controllers;

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

    // =========================
    // MEUS POSTS
    // =========================
    public async Task<IActionResult> MeusPosts()
    {
        var usuario = await _userManager.GetUserAsync(User);

        var posts = await _context.Posts
            .Where(p => p.UsuarioId == usuario!.Id)
            .OrderByDescending(p => p.DataCriacao)
            .ToListAsync();

        return View(posts);
    }

    // =========================
    // CRIAR POST
    // =========================
    [HttpGet]
    public IActionResult Create()
    {
        return View("Criar");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CriarPostViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var usuario = await _userManager.GetUserAsync(User);

        byte[]? imagemBytes = null;

        if (model.Imagem != null)
        {
            using var ms = new MemoryStream();

            await model.Imagem.CopyToAsync(ms);

            imagemBytes = ms.ToArray();
        }

        var post = new Post
        {
            Titulo = model.Titulo,
            Descricao = model.Descricao,
            Topico = model.Topico,
            Imagem = imagemBytes,
            UsuarioId = usuario!.Id,
            DataCriacao = DateTime.Now
        };

        _context.Posts.Add(post);

        await _context.SaveChangesAsync();

        TempData["Sucesso"] = "Post criado com sucesso!";

        return RedirectToAction(nameof(MeusPosts));
    }

    // =========================
    // EDITAR
    // =========================
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var usuario = await _userManager.GetUserAsync(User);

        var post = await _context.Posts
            .FirstOrDefaultAsync(p =>
                p.Id == id &&
                p.UsuarioId == usuario!.Id);

        if (post == null)
            return NotFound();

        var model = new EditarPostViewModel
        {
            Id = post.Id,
            Titulo = post.Titulo,
            Descricao = post.Descricao,
            Topico = post.Topico,
            ImagemAtual = post.Imagem
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditarPostViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var usuario = await _userManager.GetUserAsync(User);

        var post = await _context.Posts
            .FirstOrDefaultAsync(p =>
                p.Id == model.Id &&
                p.UsuarioId == usuario!.Id);

        if (post == null)
            return NotFound();

        post.Titulo = model.Titulo;
        post.Descricao = model.Descricao;
        post.Topico = model.Topico;

        if (model.Imagem != null)
        {
            using var ms = new MemoryStream();
            await model.Imagem.CopyToAsync(ms);
            post.Imagem = ms.ToArray();
        }

        await _context.SaveChangesAsync();

        TempData["Sucesso"] = "Post atualizado com sucesso!";

        return RedirectToAction(nameof(MeusPosts));
    }

    // =========================
    // EXCLUIR
    // =========================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var usuario = await _userManager.GetUserAsync(User);

        var post = await _context.Posts
            .FirstOrDefaultAsync(p =>
                p.Id == id &&
                p.UsuarioId == usuario!.Id);

        if (post == null)
            return NotFound();

        _context.Posts.Remove(post);

        await _context.SaveChangesAsync();

        TempData["Sucesso"] = "Post removido.";

        return RedirectToAction(nameof(MeusPosts));
    }

    // =========================
    // ADMIN - TODOS POSTS
    // =========================
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Todos()
    {
        var posts = await _context.Posts
            .Include(p => p.Usuario)
            .OrderByDescending(p => p.DataCriacao)
            .ToListAsync();

        return View(posts);
    }

    // =========================
    // ADMIN APAGA QUALQUER POST
    // =========================
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAdmin(int id)
    {
        var post = await _context.Posts.FindAsync(id);

        if (post == null)
            return NotFound();

        _context.Posts.Remove(post);

        await _context.SaveChangesAsync();

        TempData["Sucesso"] = "Post apagado pelo admin.";

        return RedirectToAction(nameof(Todos));
    }
}