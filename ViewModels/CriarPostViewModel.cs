using System.ComponentModel.DataAnnotations;
using SysPost.Models;

namespace SysPost.ViewModels
{
    public class CriarPostViewModel
    {
        [Required(ErrorMessage = "o titulo é obrigatorio!")]
        [StringLength(150)]
        [Display(Name = "titulo post")]
        public string Titulo { get; set; } = string.Empty;

        
        [StringLength(500)]
        [Required(ErrorMessage = "a descrição é obrigatoria!")]
        [Display(Name = "descrição post")]
        public string Descricao { get; set; } = string.Empty;

        [StringLength(5000)]
        [Display(Name = "detalhamento post")]
        public string? Detalhamento { get; set; }

        [StringLength(2000)]
        [Display(Name = "informação especial")]
        public string? InformacaoEspecial { get; set; }

        [Required]
        public TopicoPost Topico { get; set; }

    
        public IFormFile? Imagem { get; set; }
    }
}