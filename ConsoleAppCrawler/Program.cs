using Serilog;
using Services;

var logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();

Log.Logger = logger;

var filmeService = new FilmeIMDBService(logger);
var filmes = filmeService.MelhoresFilmesIMDBPorQuantidade(20);

// Salva no desktop
var caminhoCsv = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "melhores_filmes.csv");
filmeService.SalvarFilmesEmCsv(filmes, caminhoCsv);

Console.WriteLine("Processo concluído. Verifique o arquivo melhores_filmes.csv no seu desktop. Pressione qualquer tecla para sair.");
Console.ReadKey();