**EXPLICANDO O PROJETO DO MEU JEITO**

*PIPELINE DO PROJETO*
 _1_ - .CSPROJ E APPSETTINGS => PROGRAM.CS
 cria o 'WebApplicationBuilder'
 configura AppDbContext com SqlServer
 Define politicas de senha  e cookies de autenticação
 registra controllers com views
 MiddleWare Pipeline => UseHttpsRedirection => UseStaticFiles => UseRouting => UseAuthentication => UseAuthorization
 Define rota padrao + {/controller=Home}/{Action=index}/{id}
 executa SeedData => CRIA roles e usuarios iniciais
 aplica migrations e inica a aplicação
 _________________________________________________

 **Model/Usuario**
 Herda IdentityUser com campos personalizados
 NomeComppleto
 FotoPerfil
 DataCadastro (byte[])
 Bio

**Data/AppDbContext**
contexto do Efcore, herda IdentityDbContext<Usuario> incluindo todas as tabelas do identiti
vou usar DbSet tambem pra simplificar

**Data/AppDbSeed**
criar roles admin e user e usuarios iniciais
**admin**: admin@fds.com | Admin@123
**user**:  user@fds.com  | User@123

**ViewModels**
modulos de formulario
 - 'LoginViewModel': Dados de login
 - 'RegisterViewModel': Dados de registro
 - 'PerfilViewModel': Exibição de perfil
 - 'EditarPerfilViewModel': Edição de perfil

**Controllers/ ordem de fluxo**
 - 1 HomeController = pagina inicial, se logado, exibe uma coisa, se nao, exibe outra
 - 2 AccountController = Login, Registro, LogOut, Perfil, Editar Perfil
 - 3 UserController = Area do usuario autenticado ('[Authorize]')
 - 4 AdminController = area administrativa, gerencia usuarios e roles ('[Authorize(Roles = "admin")]')

 **VISUALMENTE, FLUXO DO PROGRAMA:**
 ### HTTPREQUEST => PROGRAM.CS (MIDDLEWARE PIPELINE) => ROUTING ( CONTROLLER/ACTION) => VIEWMODEL ( VALIDAÇÃO COM DATAANNOTATIONS)
 ### => IDENTITY ( USERMANAGER/SINGINMANAGER) => APPDBCONTEXT ( SQLSERVER ) => VIEW RESULT