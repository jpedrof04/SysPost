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
    // DETALHES DO POST
    // =========================
    [AllowAnonymous]
    public async Task<IActionResult> Detalhes(int id)
    {
        var post = await _context.Posts
            .Include(p => p.Usuario)
            .Include(p => p.Comentarios)
                .ThenInclude(c => c.Usuario)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null)
            return NotFound();

        var comentarios = post.Comentarios
            .OrderByDescending(c => c.DataCriacao)
            .Take(6)
            .ToList();

        var vm = new PostDetalhesViewModel
        {
            Post = post,
            Comentarios = comentarios
        };

        return View(vm);
    }

    // =========================
    // COMENTAR
    // =========================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Comentar(int postId, PostDetalhesViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.NovoComentario))
        {
            TempData["Erro"] = "O comentário não pode estar vazio.";
            return RedirectToAction(nameof(Detalhes), new { id = postId });
        }

        if (model.NovoComentario.Length > 500)
        {
            TempData["Erro"] = "O comentário deve ter no máximo 500 caracteres.";
            return RedirectToAction(nameof(Detalhes), new { id = postId });
        }

        var post = await _context.Posts
            .Include(p => p.Comentarios)
            .FirstOrDefaultAsync(p => p.Id == postId);

        if (post == null)
            return NotFound();

        var qtdComentarios = post.Comentarios.Count;

        if (qtdComentarios >= 6)
        {
            TempData["Erro"] = "Este post já atingiu o limite máximo de 6 comentários.";
            return RedirectToAction(nameof(Detalhes), new { id = postId });
        }

        var usuario = await _userManager.GetUserAsync(User);
        if (usuario == null) return Challenge();

        var comment = new Comment
        {
            Conteudo = model.NovoComentario.Trim(),
            PostId = postId,
            UsuarioId = usuario.Id,
            DataCriacao = DateTime.Now
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        TempData["Sucesso"] = "Comentário adicionado!";
        return RedirectToAction(nameof(Detalhes), new { id = postId });
    }

    // =========================
    // MEUS POSTS
    // =========================
    public async Task<IActionResult> MeusPosts()
    {
        var usuario = await _userManager.GetUserAsync(User);

        var posts = await _context.Posts
            .Include(p => p.Usuario)
            .Include(p => p.Comentarios)
                .ThenInclude(c => c.Usuario)
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
            return View("Criar", model);

        var usuario = await _userManager.GetUserAsync(User);

        if (usuario == null)
            return Challenge();

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
            Detalhamento = model.Detalhamento,
            InformacaoEspecial = model.InformacaoEspecial,
            Topico = model.Topico,
            Imagem = imagemBytes,
            UsuarioId = usuario.Id,
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
        if (usuario == null) return Challenge();

        var post = await _context.Posts
            .FirstOrDefaultAsync(p =>
                p.Id == id &&
                p.UsuarioId == usuario.Id);

        if (post == null)
            return NotFound();

        var model = new EditarPostViewModel
        {
            Id = post.Id,
            Titulo = post.Titulo,
            Descricao = post.Descricao,
            Detalhamento = post.Detalhamento,
            InformacaoEspecial = post.InformacaoEspecial,
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
        if (usuario == null) return Challenge();

        var post = await _context.Posts
            .FirstOrDefaultAsync(p =>
                p.Id == model.Id &&
                p.UsuarioId == usuario.Id);

        if (post == null)
            return NotFound();

        post.Titulo = model.Titulo;
        post.Descricao = model.Descricao;
        post.Detalhamento = model.Detalhamento;
        post.InformacaoEspecial = model.InformacaoEspecial;
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
        if (usuario == null) return Challenge();

        var post = await _context.Posts
            .Include(p => p.Comentarios)
            .FirstOrDefaultAsync(p =>
                p.Id == id &&
                p.UsuarioId == usuario.Id);

        if (post == null)
            return NotFound();

        _context.Comments.RemoveRange(post.Comentarios);
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
            .Include(p => p.Comentarios)
                .ThenInclude(c => c.Usuario)
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
        var post = await _context.Posts
            .Include(p => p.Comentarios)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null)
            return NotFound();

        _context.Comments.RemoveRange(post.Comentarios);
        _context.Posts.Remove(post);

        await _context.SaveChangesAsync();

        TempData["Sucesso"] = "Post apagado pelo admin.";

        return RedirectToAction(nameof(Todos));
    }

    // =========================
    // LIKE / UNLIKE
    // =========================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Like(int postId)
    {
        var usuario = await _userManager.GetUserAsync(User);
        if (usuario == null)
            return Challenge();

        var existing = await _context.PostLikes
            .FirstOrDefaultAsync(l => l.PostId == postId && l.UsuarioId == usuario.Id);

        if (existing != null)
        {
            _context.PostLikes.Remove(existing);
        }
        else
        {
            _context.PostLikes.Add(new PostLike
            {
                PostId = postId,
                UsuarioId = usuario.Id
            });
        }

        await _context.SaveChangesAsync();

        var referer = Request.Headers["Referer"].ToString();
        if (!string.IsNullOrEmpty(referer))
            return Redirect(referer);

        return RedirectToAction("Index", "Home");
    }
}