# IMDBCrawlerRPA

- Crawler feito utilizando Selenium em .NET para coletar os 20 melhores filmes do IMDb de acordo com votação do usuário.
- RPA feito utilizando Selenium em .NET para logar no IMDb.

___

# Como executar

- Faça o download do projeto e abra-o no Visual Studio;
- Configure como projeto de inicialização o **UConsoleAppCrawler** ou **ConsoleAppRPA**, de acordo com o que quer testar.
- Alternativamente, você pode compilá-los e executá-los direto da pasta que escolher.

___

# Funcionamento

## Crawler
- Salva um arquivo .csv na área de trabalho com os 20 melhores filmes do IMDb. O log é exibido no console.

## RPA
- Pede usuário e senha para logar no IMDb. Caso caia no captcha, o console irá pedir para o usuário resolvê-lo;
- No final, tira um print da tela e salva na área de trabalho. Vai tirar um print da tela exibindo as avaliações do usuário, ou a tela de login com erro de usuário ou senha.
  
___


# Outros projetos

Caso tenha interesse em um web crawler feito com o Html Agility Pack com implementação em DDD com API REST, [clique aqui](https://github.com/PauloVinicius97/WebCrawler).
