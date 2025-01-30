using Serilog;
using Services;

var logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();

Log.Logger = logger;

Console.Write("Digite o seu e-mail de login no IMDb: ");
var email = Console.ReadLine() ?? "teste@testemail.com";

Console.WriteLine("Digite sua senha: ");
var senha = Console.ReadLine() ?? "123456";

var loginService = new LoginIMDBService(logger);
loginService.Login(Services.Enums.TipoLogin.IMDB, email, senha);

Console.WriteLine("Processo concluído. Pressione qualquer tecla para sair.");
Console.ReadKey();