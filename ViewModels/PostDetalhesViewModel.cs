using System.ComponentModel.DataAnnotations;
using SysPost.Models;

namespace SysPost.ViewModels
{
    public class PostDetalhesViewModel
    {
        public Post Post { get; set; } = null!;

        public List<Comment> Comentarios { get; set; } = new();

        [Required(ErrorMessage = "O comentário não pode estar vazio.")]
        [StringLength(500)]
        [Display(Name = "comentário")]
        public string? NovoComentario { get; set; }
    }
}
