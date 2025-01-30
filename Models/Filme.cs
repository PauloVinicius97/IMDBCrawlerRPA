namespace Models
{
    public class Filme
    {
        public string Nome { get; private set; }
        public int AnoDeLancamento { get; private set; }
        public string Diretor { get; private set; }
        public float AvaliacaoMedia { get; private set; }
        public int NumeroDeAvaliacoes { get; private set; }

        public Filme(string nome, int anoDeLancamento, string diretor, float avaliacaoMedia, int numeroDeAvaliacoes)
        {
            Nome = nome;
            AnoDeLancamento = anoDeLancamento;
            Diretor = diretor;
            AvaliacaoMedia = avaliacaoMedia;
            NumeroDeAvaliacoes = numeroDeAvaliacoes;
        }
    }
}
