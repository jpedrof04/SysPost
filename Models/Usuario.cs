
using Microsoft.AspNetCore.Identity;

namespace SysPost.Models
{
    public class Usuario : IdentityUser
    {
        
        public string NomeCompleto { get; set; } = string.Empty;
        public byte[]? FotoPerfil { get; set; }
        public DateTime DataCadastro { get; set; } = DateTime.Now;
        public string? Bio { get; set; }

    }
}