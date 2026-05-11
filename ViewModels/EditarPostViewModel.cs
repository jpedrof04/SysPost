using System.ComponentModel.DataAnnotations;
using SysPost.Models;

namespace SysPost.ViewModels
{
    public class EditarPostViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Descricao { get; set; } = string.Empty;

        [Required]
        public TopicoPost Topico { get; set; }

        public IFormFile? Imagem { get; set; }

        public byte[]? ImagemAtual { get; set; }
    }
}
