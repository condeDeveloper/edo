namespace Conde.Edo;

/// <summary>
/// A ESTABILIDADE: quando o metodo explode mesmo com a solucao indo para zero.
///
/// Este e o fenomeno que separa os metodos em duas familias, e ele nao tem nada
/// a ver com precisao. No problema y' = -LAMBDA*y com lambda grande, a solucao
/// cai para zero em um piscar e depois nao faz mais nada; um metodo explicito
/// com passo grande demais nao apenas erra: ele produz numeros que dobram de
/// tamanho a cada passo ate virar infinito.
///
/// O limite e exato e da para deduzir numa linha. O Euler explicito faz
/// y1 = y0 * (1 - h*lambda), entao ele multiplica por esse fator a cada passo, e
/// o modulo do fator precisa ser menor que um: h menor que 2/lambda. Com lambda
/// igual a mil, isso e um passo menor que 0,002, mesmo que a solucao ja seja uma
/// linha reta em zero desde t = 0,01.
///
/// O Euler implicito faz y1 = y0 / (1 + h*lambda), e esse fator tem modulo menor
/// que um para QUALQUER h. Ele nao tem limite, e e por isso que existe.
/// </summary>
public static class Estabilidade
{
    /// <summary>
    /// O maior passo em que o metodo nao explode, procurado por bissecao.
    ///
    /// "Explodir" aqui tem definicao: a solucao do problema cai para zero, entao
    /// qualquer valor final maior que o inicial e sinal de que o metodo esta
    /// crescendo quando deveria estar decaindo.
    /// </summary>
    public static double MaiorPassoEstavel(Metodo metodo, double lambda, double ate = 1, double limite = 10)
    {
        ArgumentNullException.ThrowIfNull(metodo);

        double seguro = 0, perigoso = limite;

        if (Estavel(metodo, lambda, limite, ate)) return double.PositiveInfinity;

        for (var volta = 0; volta < 80; volta++)
        {
            var meio = (seguro + perigoso) / 2;
            if (Estavel(metodo, lambda, meio, ate)) seguro = meio;
            else perigoso = meio;
        }

        return seguro;
    }

    /// <summary>
    /// Se o metodo com aquele passo nao CRESCE em nenhum passo.
    ///
    /// A definicao importa, e a primeira que escrevi estava errada. Eu media
    /// "explodir" por limiar: o valor nao pode passar de dez. Com lambda igual a
    /// mil isso da quase o numero certo, porque o intervalo tem quinhentos
    /// passos e qualquer crescimento chega a dez bem antes do fim. Com lambda
    /// igual a dez o intervalo tem QUATRO passos, e um fator de 1,78 por passo
    /// termina em 9,99: passava no teste, e o limite medido dava 0,278 em vez de
    /// 0,2.
    ///
    /// O limiar media o tamanho do intervalo junto com a estabilidade. Medir
    /// CRESCIMENTO por passo nao mede: a solucao do problema rigido decai, entao
    /// qualquer passo que aumente o modulo ja e o metodo indo para o lado
    /// errado, aconteca no primeiro passo ou no quingentesimo.
    /// </summary>
    public static bool Estavel(Metodo metodo, double lambda, double passo, double ate = 1)
    {
        ArgumentNullException.ThrowIfNull(metodo);
        if (passo <= 0) return true;

        var problema = Problema.Rigido(lambda);
        var y = problema.Inicial;
        var quantos = Math.Max(1, (int)Math.Ceiling(ate / passo));

        for (var i = 0; i < quantos; i++)
        {
            var proximo = metodo.Passo(problema.F, problema.DerivadaEmY, passo * i, y, passo);
            if (double.IsNaN(proximo) || double.IsInfinity(proximo)) return false;
            if (Math.Abs(proximo) > Math.Abs(y)) return false;
            y = proximo;
        }

        return true;
    }

    /// <summary>
    /// O FATOR DE AMPLIFICACAO do metodo num passo, que e o que decide tudo.
    ///
    /// Ele e calculado rodando um passo a partir de um, no problema linear. Para
    /// o Euler explicito da 1 - h*lambda; para o implicito, 1/(1 + h*lambda).
    /// A medida confere os dois contra a formula.
    /// </summary>
    public static double Amplificacao(Metodo metodo, double lambda, double passo)
    {
        ArgumentNullException.ThrowIfNull(metodo);
        var problema = Problema.Rigido(lambda);
        return metodo.Passo(problema.F, problema.DerivadaEmY, 0, 1, passo);
    }

    /// <summary>
    /// Quantos passos o metodo precisa para atravessar o intervalo com
    /// estabilidade, que e o custo de verdade da rigidez.
    /// </summary>
    public static double PassosNecessarios(Metodo metodo, double lambda, double ate)
    {
        var maior = MaiorPassoEstavel(metodo, lambda, ate);
        return double.IsInfinity(maior) ? 1 : Math.Ceiling(ate / maior);
    }
}
