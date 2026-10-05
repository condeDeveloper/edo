namespace Conde.Edo;

/// <summary>
/// Um PROBLEMA DE VALOR INICIAL: y' = f(t, y), com y(0) dado.
///
/// Todo problema deste repositorio vem com a solucao EXATA junto, e isso nao e
/// conveniencia: e o juiz. Metodo de equacao diferencial erra de um jeito
/// traicoeiro, porque a saida e uma curva plausivel mesmo quando esta errada, e
/// uma curva plausivel que diverge devagar nao parece errada em nada.
///
/// Com a solucao exata, a pergunta "qual a ORDEM deste metodo?" tem resposta
/// medida: dividir o passo pela metade e ver por quanto o erro cai.
/// </summary>
public sealed class Problema
{
    public Problema(string nome, Func<double, double, double> f, double inicial, Func<double, double> exata,
                    Func<double, double, double>? derivadaEmY = null)
    {
        Nome = nome;
        F = f;
        Inicial = inicial;
        Exata = exata;
        DerivadaEmY = derivadaEmY;
    }

    public string Nome { get; }

    /// <summary>A funcao f(t, y) do lado direito.</summary>
    public Func<double, double, double> F { get; }

    public double Inicial { get; }

    /// <summary>A solucao certa, para a conta de erro.</summary>
    public Func<double, double> Exata { get; }

    /// <summary>
    /// A derivada de f em relacao a y, para o metodo implicito resolver por
    /// Newton.
    ///
    /// Ela e opcional porque nem todo problema precisa: quando falta, o
    /// implicito cai no ponto fixo, que converge devagar e as vezes nao
    /// converge. A medida mostra a diferenca.
    /// </summary>
    public Func<double, double, double>? DerivadaEmY { get; }

    public override string ToString() => Nome;

    /// <summary>
    /// y' = y, com y(0) = 1. A solucao e a exponencial.
    ///
    /// E o problema mais simples que existe e nao e trivial: a solucao cresce
    /// sem limite, entao o erro relativo e o que importa, e qualquer metodo que
    /// erre a taxa de crescimento diverge junto.
    /// </summary>
    public static Problema Exponencial() =>
        new("y' = y", (t, y) => y, 1, Math.Exp, (t, y) => 1);

    /// <summary>
    /// y' = -2ty, com y(0) = 1. A solucao e a gaussiana e^(-t^2).
    ///
    /// Ela sobe, vira e desce, entao um metodo que acerta o comeco pode errar a
    /// virada. E o problema que separa ordem um de ordem dois sem precisar de
    /// nada rigido.
    /// </summary>
    public static Problema Gaussiana() =>
        new("y' = -2ty", (t, y) => -2 * t * y, 1, t => Math.Exp(-t * t), (t, y) => -2 * t);

    /// <summary>
    /// y' = -LAMBDA*y, com lambda grande: o problema RIGIDO.
    ///
    /// A solucao decai para zero depressa e depois nao faz mais nada. Um metodo
    /// explicito precisa de passo minusculo para nao EXPLODIR, mesmo depois que
    /// a solucao ja virou uma linha reta em zero, e esse e o assunto inteiro da
    /// rigidez: o passo e ditado pela estabilidade e nao pela precisao.
    /// </summary>
    public static Problema Rigido(double lambda) =>
        new($"y' = -{lambda}y", (t, y) => -lambda * y, 1, t => Math.Exp(-lambda * t), (t, y) => -lambda);

    /// <summary>
    /// y' = t - y, com y(0) = 1. A solucao e t - 1 + 2e^(-t).
    ///
    /// Ela tem uma parte que decai e uma que cresce linear, e serve para conferir
    /// que o metodo acompanha as duas ao mesmo tempo.
    /// </summary>
    public static Problema Mista() =>
        new("y' = t - y", (t, y) => t - y, 1, t => t - 1 + 2 * Math.Exp(-t), (t, y) => -1);

    /// <summary>
    /// Um PICO estreito no meio de muita planicie: y' = -200*(t - 0,5)*y^2.
    ///
    /// A solucao e 1/(1 + 100*(t - 0,5)^2), que sobe de quase nada ate um e
    /// volta, tudo dentro de um pedaco pequeno do intervalo. Ela existe para o
    /// passo adaptativo: com passo fixo, o tamanho e ditado pelo pico e gasto
    /// na planicie, onde nao acontece nada.
    /// </summary>
    public static Problema Pico(double aperto = 100) =>
        new($"pico {aperto:N0}", (t, y) => -2 * aperto * (t - 0.5) * y * y,
            1 / (1 + aperto * 0.25),
            t => 1 / (1 + aperto * (t - 0.5) * (t - 0.5)),
            (t, y) => -4 * aperto * (t - 0.5) * y);

    /// <summary>
    /// Uma AGULHA: y' = 1/(1 + 1.000.000*(t - 0,5)^2), com y(0) = 0.
    ///
    /// O lado direito nao depende de y nenhum, entao integrar isto e calcular
    /// uma area, e a solucao exata e o arco tangente. Ela existe por dois
    /// motivos.
    ///
    /// O primeiro e que a agulha tem largura perto de 0,001 dentro de um
    /// intervalo de tamanho um: a escala do problema muda por mil de um lado
    /// para o outro, que e exatamente onde passo adaptativo deveria ganhar.
    ///
    /// O segundo e que ela e BEM CONDICIONADA. A primeira agulha que escrevi
    /// era o pico apertado mil vezes, e nela o erro cresce com y ao quadrado:
    /// uma perturbacao minuscula perto do topo volta multiplicada por 1e10, e a
    /// medida deixava de falar sobre o metodo e passava a falar sobre o
    /// problema. Aqui o erro so soma.
    /// </summary>
    public static Problema Agulha() =>
        new("agulha", (t, _) => 1 / (1 + 1e6 * (t - 0.5) * (t - 0.5)), 0,
            t => (Math.Atan(1000 * (t - 0.5)) + Math.Atan(500)) / 1000,
            (t, y) => 0);

    public static IEnumerable<Problema> Todos()
    {
        yield return Exponencial();
        yield return Gaussiana();
        yield return Mista();
    }
}
