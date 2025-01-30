using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using Serilog;
using OpenQA.Selenium.Firefox;
using Services.Enums;
using SeleniumExtras.WaitHelpers;

namespace Services
{
    public class LoginIMDBService
    {
        private const string BaseUrl = "https://www.imdb.com/registration/signin/";
        private readonly ILogger logger;
        private IWebDriver driver;
        private WebDriverWait wait;

        public LoginIMDBService(ILogger logger)
        {
            this.logger = logger;

            var options = new FirefoxOptions();
            //options.AddArgument("--headless");
            options.AddArgument("--lang=pt-BR");
            options.AddArgument("no-sandbox");

            driver = new FirefoxDriver(options);
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        public bool Login(TipoLogin tipoLogin, string email, string senha)
        {
            var logou = false;

            try
            {
                Log.Information($"Navegando para a URL {BaseUrl}");
                driver.Navigate().GoToUrl(BaseUrl);

                switch (tipoLogin)
                {
                    case TipoLogin.Google:
                        logou = LoginComGoogle(email, senha);
                        break;
                    case TipoLogin.IMDB:
                        logou = LoginComIMDB(email, senha);
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Erro ao tentar logar: {ex.Message}. {ex.InnerException}");
            }
            finally
            {
                Log.Information("Encerrando FirefoxDriver");
                driver.Quit();
            }

            return logou;
        }


        /*
         * Não consegui fazer funcionar pois o Google detectou que estava tentando logar com Selenium.
         * Vi algumas alternativas para fazer rodar mas envolvem usar o Chrome local e com um perfil já logado naquela conta.
         */
        private bool LoginComGoogle(string email, string senha)
        {
            var googleSignInButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//span[text()='Sign in with Google']")));
            googleSignInButton.Click();

            var emailField = wait.Until(ExpectedConditions.ElementIsVisible(By.Id("identifierId")));
            emailField.SendKeys(email);

            var nextButton = driver.FindElement(By.Id("identifierNext"));
            nextButton.Click();

            var passwordField = wait.Until(ExpectedConditions.ElementIsVisible(By.Name("password")));
            passwordField.SendKeys(senha);

            var signInButton = driver.FindElement(By.Id("passwordNext"));
            signInButton.Click();

            var continuar = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//span[text()='Continuar']")));
            continuar.Click();

            if (driver.PageSource.Contains("Finish registration"))
            {
                var continuarImdb = wait.Until(ExpectedConditions.ElementToBeClickable(By.Id("continue")));
                continuarImdb.Click();
            }

            var conta = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("/ html / body / div[2] / nav / div[2] / div[6]")));
            conta.Click();

            var suasAvaliacoes = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//span[text()='Suas avaliações']")));
            suasAvaliacoes.Click();

            var suasClassificacoes = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//span[text()='Suas classificações']")));

            return true;
        }

        private bool LoginComIMDB(string email, string senha)
        {
            Log.Information("Tipo de login IMDb selecionado");

            Log.Information("Clicando na opção de logar com o IMDb");
            var imdbLogin = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//span[text()='Sign in with IMDb']")));
            imdbLogin.Click();

            Log.Information($"Preenchendo campo de e-mail com {email}");
            var campoEmail = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//*[@id='ap_email']")));
            campoEmail.SendKeys(email);

            Log.Information($"Preenchendo campo de senha com ******");
            var campoSenha = driver.FindElement(By.XPath("//*[@id='ap_password']"));
            campoSenha.SendKeys(senha);

            Log.Information("Clicando no botão de logar");
            var botaoLogar = driver.FindElement(By.XPath("//*[@id='signInSubmit']"));
            botaoLogar.Click();

            // Espera até que uma dessas mensagens apareça (o ElementoExiste é para a div do captcha)
            wait.Until(d => d.PageSource.Contains("Your password is incorrect") 
                || d.PageSource.Contains("We cannot find an account with that email address") 
                || d.PageSource.Contains("Menu")
                || ElementoExiste(By.CssSelector("div.a-section:nth-child(3)")));

            // Verifica por alguma mensagem de erro
            VerificarMensagensErro();

            // Se estiver na página de captcha
            if (ElementoExiste(By.CssSelector("div.a-section:nth-child(3)")))
            {
                Log.Information("Aguardando usuário resolver o captcha");
                var wait30s = new WebDriverWait(driver, TimeSpan.FromSeconds(30));

                // Aguarda até que o captcha esteja visível
                wait30s.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("div.a-section:nth-child(3)")));

                // Aguarda até que o captcha seja resolvido e o elemento esteja invisível
                wait30s.Until(ExpectedConditions.InvisibilityOfElementLocated(By.CssSelector("div.a-section:nth-child(3)")));

                // Aguarda até que a página esteja completamente carregada
                Log.Information("Aguardando a tela estar totalmente carregada");
                wait30s.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));

                string tituloDaPagina = driver.Title;

                if (tituloDaPagina.Contains("IMDb Sign-In"))
                {
                    // Verifica mensagens de erro de novo agora que voltou pra página de login
                    VerificarMensagensErro();
                }
            }

            Log.Information("Clicando em Conta");
            var conta = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("/ html / body / div[2] / nav / div[2] / div[6]")));
            conta.Click();

            Log.Information("Clicando em Suas Avaliações");
            var suasAvaliacoes = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//span[text()='Suas avaliações']")));
            suasAvaliacoes.Click();

            Log.Information("Verificando se acessou a página de avaliações");
            var suasClassificacoes = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//span[text()='Suas classificações']")));

            Log.Information("Tirando print da tela");
            TirarPrint(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "printloginimdb.png"));

            return true;
        }

        // Tira print da tela
        private void TirarPrint(string caminho)
        {
            Screenshot ss = ((ITakesScreenshot)driver).GetScreenshot();
            ss.SaveAsFile(caminho);
        }

        // Separei em um método próprio pois repito essas ações após o usuário digitar o captcha
        private void VerificarMensagensErro()
        {
            if (driver.PageSource.Contains("Your password is incorrect"))
            {
                Log.Information("Senha incorreta detectada");

                Log.Information("Tirando print da tela");
                TirarPrint(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "senhaincorretaimdb.png"));

                throw new Exception("Senha incorreta");
            }
            else if (driver.PageSource.Contains("We cannot find an account with that email address"))
            {
                Log.Information("E-mail não cadastrado detectado");

                Log.Information("Tirando print da tela");
                TirarPrint(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "emailnaocadastradoimdb.png"));

                throw new Exception("E-mail não cadastrado");
            }
        }

        bool ElementoExiste(By by)
        {
            try
            {
                return driver.FindElement(by) != null;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }
    }
}
