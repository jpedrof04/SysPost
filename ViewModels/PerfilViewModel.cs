

namespace SysPost.ViewModels
{
    public class PerfilViewModel
    {
        public string NomeCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public string? Bio { get; set; }

        //foto vem do banco com bytes

        public byte[]? FotoPerfil { get; set; }

        public DateTime DataCadastro { get; set; }

        public string Perfil { get; set; } = string.Empty;
    }
}