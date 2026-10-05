namespace Conde.Edo;

/// <summary>
/// O OSCILADOR e a conservacao de energia, que e o achado mais bonito do
/// repositorio.
///
/// A energia do oscilador harmonico e constante na solucao exata: ela vale um e
/// fica em um para sempre. Os metodos tratam essa constante de maneiras
/// completamente diferentes, e a diferenca NAO aparece no erro ponto a ponto
/// medido em tempo curto.
///
/// O Euler explicito GANHA energia e a orbita abre numa espiral que vai para o
/// infinito. O implicito PERDE energia e a orbita fecha numa espiral que morre
/// no centro. Os dois sao de ordem um e erram parecido no inicio; no longo
/// prazo, um simula um pendulo que acelera sozinho e o outro um que para.
///
/// E o EULER SIMPLETICO, que e o explicito com UMA LINHA trocada de lugar,
/// mantem a energia oscilando em volta do valor certo para sempre. Ele continua
/// sendo de ordem um, ou seja, menos preciso ponto a ponto, e e o unico dos tres
/// que simula um pendulo de verdade.
/// </summary>
public sealed class Orbita
{
    public Orbita(string nome, Func<double, double, double, (double X, double V)> passo)
    {
        Nome = nome;
        Passo = passo;
    }

    public string Nome { get; }

    /// <summary>x, v, h -> o proximo par.</summary>
    public Func<double, double, double, (double X, double V)> Passo { get; }

    public override string ToString() => Nome;

    /// <summary>Euler explicito nas duas variaveis ao mesmo tempo.</summary>
    public static (double X, double V) Explicito(double x, double v, double h) => (x + h * v, v - h * x);

    /// <summary>
    /// Euler implicito, resolvido de forma fechada.
    ///
    /// O sistema x1 = x0 + h*v1 e v1 = v0 - h*x1 tem solucao direta, sem Newton:
    /// os dois saem divididos por 1 + h^2. E e esse divisor que come a energia a
    /// cada passo.
    /// </summary>
    public static (double X, double V) Implicito(double x, double v, double h)
    {
        var fundo = 1 + h * h;
        return ((x + h * v) / fundo, (v - h * x) / fundo);
    }

    /// <summary>
    /// EULER SIMPLETICO: atualiza a velocidade PRIMEIRO e usa a velocidade NOVA
    /// para andar a posicao.
    ///
    /// A diferenca para o explicito e uma linha de lugar. E ela muda a natureza
    /// do metodo: ele passa a preservar area no espaco de fase, que e o que
    /// mantem a energia limitada para sempre.
    /// </summary>
    public static (double X, double V) Simpletico(double x, double v, double h)
    {
        var novaV = v - h * x;
        return (x + h * novaV, novaV);
    }

    public static IEnumerable<Orbita> Todas()
    {
        yield return new Orbita("Euler explicito", Explicito);
        yield return new Orbita("Euler implicito", Implicito);
        yield return new Orbita("Euler simpletico", Simpletico);
    }

    /// <summary>Roda a orbita e devolve a energia no fim e a maior energia vista.</summary>
    public static (double Final, double Maior, double Menor) Rodar(Orbita orbita, double ate, double passo)
    {
        ArgumentNullException.ThrowIfNull(orbita);

        double x = 1, v = 0;
        var maior = Oscilador.Energia(x, v);
        var menor = maior;

        var quantos = (int)(ate / passo);
        for (var i = 0; i < quantos; i++)
        {
            (x, v) = orbita.Passo(x, v, passo);
            var energia = Oscilador.Energia(x, v);
            if (double.IsNaN(energia) || double.IsInfinity(energia)) return (energia, energia, menor);
            maior = Math.Max(maior, energia);
            menor = Math.Min(menor, energia);
        }

        return (Oscilador.Energia(x, v), maior, menor);
    }

    /// <summary>O erro de posicao depois de um tempo curto, para a comparacao justa.</summary>
    public static double ErroDePosicao(Orbita orbita, double ate, double passo)
    {
        ArgumentNullException.ThrowIfNull(orbita);

        double x = 1, v = 0;
        var quantos = (int)(ate / passo);
        for (var i = 0; i < quantos; i++) (x, v) = orbita.Passo(x, v, passo);

        return Math.Abs(x - Oscilador.Exata(quantos * passo).X);
    }
}
