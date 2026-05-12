using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SysPost.Models
{
    public class Post
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Descricao { get; set; } = string.Empty;

        [StringLength(5000)]
        public string? Detalhamento { get; set; }

        [StringLength(2000)]
        public string? InformacaoEspecial { get; set; }

        [Required]
        public TopicoPost Topico { get; set; }

        public byte[]? Imagem { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.Now;

        public ICollection<Comment> Comentarios { get; set; } = new List<Comment>();

        // FK usuário
        [Required]
        public string UsuarioId { get; set; } = string.Empty;

        [ForeignKey(nameof(UsuarioId))]
        public Usuario Usuario { get; set; } = null!;
    }
}