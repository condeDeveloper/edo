namespace Conde.Edo;

/// <summary>
/// Um sistema de DUAS equacoes, para o oscilador harmonico.
///
/// Ele existe separado do escalar porque o achado mais interessante do
/// repositorio so aparece com duas variaveis: a conservacao de energia. Com uma
/// variavel so nao ha o que conservar.
/// </summary>
public sealed class Oscilador
{
    /// <summary>
    /// O oscilador harmonico: x'' = -x, escrito como x' = v e v' = -x.
    ///
    /// A solucao e o seno e o cosseno, e a ENERGIA, que vale x^2 + v^2, e
    /// constante para sempre. E essa constante que os metodos tratam de maneiras
    /// completamente diferentes, e a diferenca nao aparece no erro ponto a ponto.
    /// </summary>
    public static (double X, double V) Derivada(double x, double v) => (v, -x);

    /// <summary>A energia, que a solucao exata mantem em um para sempre.</summary>
    public static double Energia(double x, double v) => x * x + v * v;

    public static (double X, double V) Exata(double t) => (Math.Cos(t), -Math.Sin(t));
}
