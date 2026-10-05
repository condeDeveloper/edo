namespace Conde.Edo;

/// <summary>
/// Os METODOS DE PASSO: cada um recebe o ponto de agora e devolve o de daqui a
/// um passo.
///
/// Todos eles sao a mesma ideia com mais ou menos cuidado: estimar a inclinacao
/// media no intervalo. O que muda e quantas vezes a funcao e avaliada e onde, e
/// esse numero decide a ORDEM, ou seja, por quanto o erro cai quando o passo cai
/// pela metade.
/// </summary>
public sealed class Metodo
{
    public Metodo(string nome, int ordemTeorica, int avaliacoesPorPasso, bool implicito,
                  Func<Func<double, double, double>, Func<double, double, double>?, double, double, double, double> passo)
    {
        Nome = nome;
        OrdemTeorica = ordemTeorica;
        AvaliacoesPorPasso = avaliacoesPorPasso;
        Implicito = implicito;
        Passo = passo;
    }

    public string Nome { get; }

    /// <summary>A ordem que a teoria promete, para a medida comparar.</summary>
    public int OrdemTeorica { get; }

    public int AvaliacoesPorPasso { get; }

    /// <summary>Se ele precisa resolver uma equacao a cada passo.</summary>
    public bool Implicito { get; }

    /// <summary>f, df/dy, t, y, h -> y do proximo passo.</summary>
    public Func<Func<double, double, double>, Func<double, double, double>?, double, double, double, double> Passo { get; }

    public override string ToString() => Nome;
}
