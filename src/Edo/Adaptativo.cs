namespace Conde.Edo;

/// <summary>
/// O PASSO ADAPTATIVO: medir o proprio erro a cada passo e mudar o tamanho do
/// passo por causa dele.
///
/// Passo fixo e uma aposta cega. O passo tem que ser pequeno o bastante para a
/// pior parte do intervalo, e em todo o resto ele e pequeno a toa. Num problema
/// com um pico estreito e muita planicie, isso quer dizer gastar na planicie o
/// mesmo que no pico.
///
/// A estimativa sai de graca, por extrapolacao de Richardson: dar UM passo de
/// tamanho h e depois DOIS de tamanho h/2, do mesmo ponto. As duas respostas
/// diferem pelo erro, e para um metodo de ordem p o erro do passo fino e a
/// diferenca dividida por 2^p - 1. Isso custa tres passos no lugar de um, e
/// paga.
/// </summary>
public static class Adaptativo
{
    /// <summary>O que a integracao adaptativa devolve, com o custo junto.</summary>
    public sealed record Resultado(double Final, double PiorErro, long Avaliacoes, int Aceitos,
                                   int Rejeitados, double MenorPasso, double MaiorPasso, bool Terminou);

    /// <summary>
    /// Integra com passo adaptativo ate o fim do intervalo.
    ///
    /// A tolerancia e POR UNIDADE DE TEMPO: um passo de tamanho h pode errar no
    /// maximo tolerancia vezes h. Sem isso, dividir o intervalo em mais passos
    /// deixaria o erro total crescer junto, e a tolerancia nao significaria
    /// nada no fim.
    /// </summary>
    public static Resultado Integrar(Metodo metodo, Problema problema, double ate, double tolerancia,
                                     double passoInicial = 0.1, int limite = 4_000_000)
    {
        ArgumentNullException.ThrowIfNull(metodo);
        ArgumentNullException.ThrowIfNull(problema);

        var t = 0.0;
        var y = problema.Inicial;
        var h = passoInicial;
        long avaliacoes = 0;
        int aceitos = 0, rejeitados = 0;
        double menor = double.PositiveInfinity, maior = 0, pior = 0;

        var divisor = Math.Pow(2, metodo.OrdemTeorica) - 1;

        // O limite nao e paranoia: com um metodo de ordem UM e tolerancia
        // apertada, o passo que a tolerancia pede fica na casa de 1e-8 e a
        // conta deixa de terminar em tempo humano. Melhor devolver "nao
        // terminou" do que rodar para sempre.
        while (t < ate - 1e-14 && aceitos + rejeitados < limite)
        {
            h = Math.Min(h, ate - t);

            var grosso = metodo.Passo(problema.F, problema.DerivadaEmY, t, y, h);
            var meio = metodo.Passo(problema.F, problema.DerivadaEmY, t, y, h / 2);
            var fino = metodo.Passo(problema.F, problema.DerivadaEmY, t + h / 2, meio, h / 2);
            avaliacoes += 3L * metodo.AvaliacoesPorPasso;

            var erro = Math.Abs(fino - grosso) / divisor;
            var permitido = tolerancia * h;

            if (erro <= permitido || h <= 1e-12)
            {
                // O passo fino ja esta calculado e e melhor que o grosso: jogar
                // ele fora para repetir com o grosso seria pagar e nao levar.
                t += h;
                y = fino;
                aceitos++;
                menor = Math.Min(menor, h);
                maior = Math.Max(maior, h);
                pior = Math.Max(pior, Math.Abs(y - problema.Exata(t)));
            }
            else
            {
                rejeitados++;
            }

            h *= Fator(erro, permitido, metodo.OrdemTeorica);
        }

        return new Resultado(y, pior, avaliacoes, aceitos, rejeitados,
                             double.IsInfinity(menor) ? 0 : menor, maior, t >= ate - 1e-14);
    }

    /// <summary>
    /// Por quanto multiplicar o passo depois de medir o erro.
    ///
    /// O erro de um passo cresce como h^(p+1), entao o fator que acerta a
    /// tolerancia na mosca e a raiz (p+1) da razao. Os tres cuidados em volta
    /// nao sao enfeite: o 0,9 evita ficar na beirada e ser rejeitado no passo
    /// seguinte, e os limites de 0,2 e 5 evitam que um erro minusculo mande o
    /// passo para o infinito de uma vez.
    /// </summary>
    public static double Fator(double erro, double permitido, int ordem)
    {
        if (erro <= 0) return 5;
        var cru = 0.9 * Math.Pow(permitido / erro, 1.0 / (ordem + 1));
        return Math.Clamp(cru, 0.2, 5);
    }

    /// <summary>
    /// Quantas avaliacoes o passo FIXO gasta para chegar no mesmo erro final,
    /// que e a unica comparacao justa.
    /// </summary>
    public static (long Avaliacoes, int Passos) CustoDoPassoFixo(Metodo metodo, Problema problema,
                                                                double ate, double erroAlvo)
    {
        ArgumentNullException.ThrowIfNull(metodo);

        // A busca comeca em 128 passos, e nao em um, porque "o pior erro do
        // caminho" com um ponto so nao mede caminho nenhum. E ha um caso
        // concreto: no problema do pico, o ponto medio com UM passo avalia a
        // funcao exatamente no centro, onde ela vale zero, nao sai do lugar e
        // termina no valor certo, com erro ZERO. Ele acerta por nao andar.
        var passos = 128;
        while (passos < 20_000_000 && Ordem.ErroMaximo(metodo, problema, ate, passos) > erroAlvo) passos *= 2;

        return ((long)passos * metodo.AvaliacoesPorPasso, passos);
    }
}
