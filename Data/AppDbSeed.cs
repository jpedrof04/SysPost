using System.ComponentModel;
using learnfds.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Identity.Client;

namespace learnfds.Data
{
    
    public class AppDbSeed
    {
        //metodo original - us o DI ( para o banco principal )
        public static async Task InicializarAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<Usuario>>();
            await SeedRolesEUsuarios(roleManager, userManager);
        }

        //metodo para bancos adicionais
        public static async Task InicializarComContextoAsync(AppDbContext db)
        {
            var roleStore = new Microsoft.AspNetCore.Identity.EntityFrameworkCore.RoleStore<IdentityRole>(db);
            var userStore = new Microsoft.AspNetCore.Identity.EntityFrameworkCore.UserStore<Usuario>(db);

            var roleManager = new RoleManager<IdentityRole>(
                roleStore,
                new IRoleValidator<IdentityRole>[] { new RoleValidator<IdentityRole>()},
                new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(),
                null!

            );

            var userManager = new UserManager<Usuario>(
                userStore,
                null!,
                new PasswordHasher<Usuario>(),
                new IUserValidator<Usuario>[] { new UserValidator<Usuario>() },
                new IPasswordValidator<Usuario>[] { new PasswordValidator<Usuario>() },
                new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(),
                null!,
                null!);

            await SeedRolesEUsuarios(roleManager, userManager);
            await db.SaveChangesAsync();

        }

        private static async Task SeedRolesEUsuarios(
            RoleManager<IdentityRole> roleManager,
            UserManager<Usuario> userManager)
        {
            //roles
            foreach (var role in new[] { "Admin", "User" })
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            //Admin padrão
            await CriarUsuarioSeNaoExistir(userManager,
                email: "admin@sis.com",
                senha: "Admin@123",
                nome: "Admin do sistema guys",
                bio: "Conta de admin",
                role: "Admin"
            );

            //usuario padrao
            await CriarUsuarioSeNaoExistir(userManager,
                email: "usuario@sis.com",
                senha: "Usuario@123",
                nome: "usuario do sistema guys",
                bio: "Conta de user normal",
                role: "User"
            );
        }

        private static async Task CriarUsuarioSeNaoExistir(
            UserManager<Usuario> userManager,
            string email, string senha, string nome, string bio, string role)
        {
            if (await userManager.FindByEmailAsync(email) != null) return;

            var user = new Usuario
            {
              UserName = email,
              Email = email,
              NomeCompleto = nome,
              EmailConfirmed = true,
              DataCadastro = DateTime.Now,
              Bio = bio  
            };

            var resultado = await userManager.CreateAsync(user, senha);
            if (resultado.Succeeded)
                await userManager.AddToRoleAsync(user, role);
        }

    }
}