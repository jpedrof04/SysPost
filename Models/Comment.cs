using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SysPost.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        [StringLength(500)]
        public string Conteudo { get; set; } = string.Empty;

        public DateTime DataCriacao { get; set; } = DateTime.Now;

        [Required]
        public int PostId { get; set; }

        [ForeignKey(nameof(PostId))]
        public Post Post { get; set; } = null!;

        [Required]
        public string UsuarioId { get; set; } = string.Empty;

        [ForeignKey(nameof(UsuarioId))]
        public Usuario Usuario { get; set; } = null!;
    }
}
