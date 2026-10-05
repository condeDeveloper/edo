namespace Conde.Edo;

/// <summary>
/// A ORDEM MEDIDA, que e o que transforma "este metodo e de ordem quatro" de
/// citacao em numero.
///
/// Toda a aritmetica de erro deste repositorio esta aqui, e ela e simples de
/// proposito: integrar, comparar com a solucao exata, repetir com metade do
/// passo.
/// </summary>
public static class Ordem
{
    /// <summary>Roda o metodo do zero ate o fim e devolve o valor final.</summary>
    public static double Integrar(Metodo metodo, Problema problema, double ate, int passos)
    {
        ArgumentNullException.ThrowIfNull(metodo);
        ArgumentNullException.ThrowIfNull(problema);

        var h = ate / passos;
        var y = problema.Inicial;

        for (var i = 0; i < passos; i++)
            y = metodo.Passo(problema.F, problema.DerivadaEmY, h * i, y, h);

        return y;
    }

    /// <summary>O erro do metodo no fim do intervalo.</summary>
    public static double Erro(Metodo metodo, Problema problema, double ate, int passos)
    {
        ArgumentNullException.ThrowIfNull(problema);
        return Math.Abs(Integrar(metodo, problema, ate, passos) - problema.Exata(ate));
    }

    /// <summary>
    /// O PIOR erro ao longo do caminho, e nao so no fim.
    ///
    /// Medir so o valor final parece natural e tem uma armadilha: se a solucao
    /// volta para perto de onde comecou, um metodo que praticamente nao anda
    /// acerta o fim sem ter visto nada no meio. No problema do pico isso
    /// acontece de verdade, e o numero esta no README.
    /// </summary>
    public static double ErroMaximo(Metodo metodo, Problema problema, double ate, int passos)
    {
        ArgumentNullException.ThrowIfNull(metodo);
        ArgumentNullException.ThrowIfNull(problema);

        var h = ate / passos;
        var y = problema.Inicial;
        var pior = 0.0;

        for (var i = 0; i < passos; i++)
        {
            y = metodo.Passo(problema.F, problema.DerivadaEmY, h * i, y, h);
            if (double.IsNaN(y) || double.IsInfinity(y)) return double.PositiveInfinity;
            pior = Math.Max(pior, Math.Abs(y - problema.Exata(h * (i + 1))));
        }

        return pior;
    }

    /// <summary>
    /// A ORDEM MEDIDA: dividir o passo pela metade e ver por quanto o erro cai.
    ///
    /// Um metodo de ordem p tem o erro dividido por 2^p quando o passo cai pela
    /// metade, entao o logaritmo da razao na base dois e a ordem. Esse numero
    /// nao vem de teoria nenhuma: ele sai da conta.
    ///
    /// A medida usa passos pequenos o bastante para o termo dominante do erro
    /// mandar, e grandes o bastante para o arredondamento nao mandar. Fora dessa
    /// faixa o numero deixa de significar o que diz.
    /// </summary>
    public static double OrdemMedida(Metodo metodo, Problema problema, double ate, int passos)
    {
        var grosso = Erro(metodo, problema, ate, passos);
        var fino = Erro(metodo, problema, ate, passos * 2);
        if (fino <= 0 || grosso <= 0) return double.NaN;
        return Math.Log2(grosso / fino);
    }
}
