using System.ComponentModel.DataAnnotations;

namespace learnfds.ViewModels
{
    //LOGIN 
    //dados enviados pelo formulario de login
    public class LoginViewModel
    {
        [Required(ErrorMessage = "O email é obrigatorio.")]
        [EmailAddress(ErrorMessage = "Formato de email invalido!")]
        [Display(Name = "E-mail")]
        public string Email { get; set; } = string.Empty;


        [Required(ErrorMessage = "A senha é obrigatoria.")]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string Senha { get; set; } = string.Empty;


        [Display(Name = "lembrar do login")]
        public bool LembrarMe { get; set; }
    }

    //REGISTRO DE NOVO USUARIO!!!!!!

    public class RegisterViewModel
    {

        [Required(ErrorMessage = "O nome é obrigatorio")]
        [StringLength(200, ErrorMessage = "o nome deve ter no maximo 200 caracteres. ")]
        [Display(Name = "nome completo")]
        public string NomeCompleto { get; set; } = string.Empty;

        [Required(ErrorMessage = "o e-mail é obrigatorio.")]
        [EmailAddress(ErrorMessage = "Formato e-mail invalido")]
        [Display(Name = "E-mail")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatoria.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = " a senha deve ter entre 6 a 100 caracteres")]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string Senha { get; set; } = string.Empty;

        [Required(ErrorMessage = "A confirmação de senha é obrigatoria.")]
        [DataType(DataType.Password)]
        [Compare("Senha", ErrorMessage = "as senhas sao diferentes")]
        [Display(Name = "Confirmar Senha")]
        public string ConfirmarSenha { get; set; } = string.Empty;

    }
}
