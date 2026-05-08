

using System.ComponentModel.DataAnnotations;

namespace SysPost.ViewModels
{

    //EDITAR PERFIL!!
    public class EditarPerfilViewModel
    {
        [Required(ErrorMessage = " o nome completo é obrigatorio!.")]
        [StringLength(200)]
        [Display(Name = "Nome completo")]

        public string NomeCompleto { get; set; } = string.Empty;

        public string? Bio { get; set; }

        public IFormFile? FotoArquivo { get; set; }

        //pra mostrar a foto de perfil atual na edição
        public byte[]? FotoAtual { get; set; }
    }
}
