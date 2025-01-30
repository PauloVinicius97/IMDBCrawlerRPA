using System.Globalization;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using Serilog;
using Services.Models;

namespace Services
{
    public class FilmeIMDBService
    {
        private const string BaseUrl = "https://www.imdb.com/chart/top/?ref_=nv_mv_250";
        private readonly ILogger logger;

        public FilmeIMDBService(ILogger logger)
        {
            this.logger = logger;
        }

        public List<Filme> MelhoresFilmesIMDBPorQuantidade(int quantidadeFilmes)
        {
            logger.Information($"Coletando os {quantidadeFilmes} melhores filmes do IMDB");

            // Configurando opções do Firefox para janela não aparecer e abrir a página em português do Brasil
            logger.Information("Adicionando argumentos --headless, --lang=pt-BR e no-sandbox no FirefoxDriver");
            var options = new FirefoxOptions();
            options.AddArgument("--headless");
            options.AddArgument("--lang=pt-BR");
            options.AddArgument("no-sandbox");

            // Abrindo o driver
            logger.Information("Abrindo o FirefoxDriver");
            var driver = new FirefoxDriver(options);

            var filmesList = new List<Filme>();

            try
            {
                logger.Information($"Acessando {BaseUrl}");
                driver.Navigate().GoToUrl(BaseUrl);

                // Aguardando a lista carregar
                logger.Information("Aguardando a lista de filmes carregar");
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                var ulElement = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("ul.ipc-metadata-list")));

                // Pegando a lista de filmes baseada na quantidadeFilmes
                logger.Information("Coletando filmes");
                var filmes = driver.FindElements(By.CssSelector("ul.ipc-metadata-list li")).Take(quantidadeFilmes);

                // Percorre cada filme, coleta os dados, trata-os e cria uma nova entidade Filme
                logger.Information("Percorrendo filmes");

                foreach (var filme in filmes)
                {
                    var titulo = filme.FindElement(By.CssSelector("h3.ipc-title__text")).Text.Remove(0, 3).Trim();
                    var ano = Int32.TryParse(filme.FindElement(By.CssSelector("span.cli-title-metadata-item")).Text, out int anoInt);

                    logger.Information($"Coletando informações do filme {titulo} de {anoInt}");

                    var avaliacaoMediaString = filme.FindElement(By.CssSelector("span.ipc-rating-star--rating")).Text.Replace(",", ".");
                    var avaliacaoMediaBool = float.TryParse(avaliacaoMediaString, NumberStyles.Float, CultureInfo.InvariantCulture, out float avaliacaoMediaFloat);

                    var numeroDeAvaliacoesString = filme.FindElement(By.CssSelector("span.ipc-rating-star--voteCount")).Text.Replace("(", "").Replace(")", "");
                    var numeroDeAvaliacoes = ConverterNumeroDeAvaliacoesParaInt(numeroDeAvaliacoesString);

                    // Abre a popupzinha onde aparece o nome do diretor para coletar o nome do mesmo
                    var botaoPopup = filme.FindElement(By.CssSelector("button.ipc-icon-button.li-info-icon.ipc-icon-button--base.ipc-icon-button--onAccent2"));
                    // Uso JavaScript pra clicar no botão, já que ele pode estar fora da área de visão do navegador
                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", botaoPopup);

                    // Espera a popup ficar visível
                    var popup = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("div.ipc-promptable-base__panel")));

                    var nomeDiretorElement = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("div.ipc-promptable-base__panel a.ipc-link.ipc-link--baseAlt")));
                    var nomeDiretor = nomeDiretorElement.Text;

                    // Localizando o botão de fechar da popup e fechando-a
                    var botaoFechar = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("button.ipc-icon-button.ipc-icon-button--baseAlt.ipc-icon-button--onBase")));
                    botaoFechar.Click();

                    var filmeEntity = new Filme(titulo, anoInt, nomeDiretor, avaliacaoMediaFloat, numeroDeAvaliacoes);

                    filmesList.Add(filmeEntity);

                    logger.Information($"Filme {titulo} convertido em entidade Filme");
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Erro encontrado: {ex.Message}, {ex.InnerException}");
            }
            finally
            {
                Log.Information("Fechando driver");
                driver.Quit();
            }

            return filmesList;
        }

        /*
         * Converte string de numero de avaliações para um número real
         * ex.: "1 mi" é convertido para 1000000, enquanto "1 mil" é convertido para 1000
         */
        private int ConverterNumeroDeAvaliacoesParaInt(string numeroDeAvaliacoes)
        {
            var stringDividida = numeroDeAvaliacoes.Trim().Split(' ');
            var ehNumeroValido = float.TryParse(stringDividida[0], out float resultado);

            if (ehNumeroValido)
            {
                if (stringDividida.Length > 1)
                {
                    float numeroFinal;

                    switch (stringDividida[1])
                    {
                        case "mi":
                            numeroFinal = resultado * 1000000;
                            break;

                        case "mil":
                            numeroFinal = resultado * 1000;
                            break;

                        default:
                            numeroFinal = resultado;
                            break;
                    }

                    return Convert.ToInt32(numeroFinal);
                }
                else
                {
                    var numeroQuebrado = resultado != Math.Floor(resultado);

                    if (numeroQuebrado)
                    {
                        return Convert.ToInt32(resultado) * 10;
                    }
                    else
                    {
                        return Convert.ToInt32(resultado);
                    }
                }
            }
            else
            {
                return 0;
            }
        }

        // Salva o arquivo em CSV
        public void SalvarFilmesEmCsv(List<Filme> filmes, string caminho)
        {
            try
            {
                using (var writer = new StreamWriter(caminho))
                {
                    writer.WriteLine("Nome,Ano,Diretor,Avaliação Média,Número de Avaliações");

                    foreach (var filme in filmes)
                    {
                        var linha = $"{filme.Nome},{filme.AnoDeLancamento},{filme.Diretor},{filme.AvaliacaoMedia},{filme.NumeroDeAvaliacoes}";
                        writer.WriteLine(linha);
                    }
                }

                logger.Information($"Arquivo .csv salvo com sucesso em {caminho}");
            }
            catch (Exception ex)
            {
                logger.Error($"Erro ao salvar o arquivo .csv: {ex.Message}");
            }
        }
    }
}
