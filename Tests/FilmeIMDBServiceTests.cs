using Moq;
using Serilog;
using Services;
using Services.Models;

namespace Tests
{
    public class FilmeIMDBServiceTests
    {
        private readonly FilmeIMDBService _service;
        private readonly Mock<ILogger> _loggerMock;

        public FilmeIMDBServiceTests()
        {
            _loggerMock = new Mock<ILogger>();
            _service = new FilmeIMDBService(_loggerMock.Object);
        }

        [Fact]
        public void MelhoresFilmesIMDBPorQuantidade_DeveRetornarListaDeFilmes()
        {
            // Arrange
            int quantidadeFilmes = 20;

            // Act
            var result = _service.MelhoresFilmesIMDBPorQuantidade(quantidadeFilmes);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<Filme>>(result);
            Assert.Equal(quantidadeFilmes, result.Count);
        }
    }
}
