using Conde.Edo;
using Xunit;

namespace Conde.Edo.Testes;

public class AdaptativoTestes
{
    private static Metodo Pelo(string nome) => Metodos.Todos().First(m => m.Nome == nome);

    [Fact]
    public void OAdaptativoChegaNoFimDoIntervalo()
    {
        foreach (var metodo in Metodos.Todos())
        {
            if (metodo.Implicito || metodo.OrdemTeorica == 1) continue;

            var r = Adaptativo.Integrar(metodo, Problema.Agulha(), 1, 1e-9, 0.01);

            Assert.True(r.Terminou);
            Assert.True(r.Aceitos > 0);
        }
    }

    /// <summary>
    /// Com a tolerancia apertada, o resultado respeita a tolerancia de verdade,
    /// medido contra a solucao exata e nao contra a propria estimativa.
    /// </summary>
    [Fact]
    public void ComToleranciaApertadaOErroRealFicaDentro()
    {
        foreach (var metodo in Metodos.Todos())
        {
            if (metodo.Implicito || metodo.OrdemTeorica == 1) continue;

            var r = Adaptativo.Integrar(metodo, Problema.Agulha(), 1, 1e-9, 0.01);
            Assert.True(r.PiorErro < 1e-8, $"{metodo.Nome} errou {r.PiorErro}");
        }
    }

    /// <summary>
    /// E o passo varia de verdade ao longo do intervalo: o maior e dezenas ou
    /// centenas de vezes o menor, e e essa razao que o passo fixo joga fora.
    /// </summary>
    [Fact]
    public void OPassoVariaAoLongoDoIntervalo()
    {
        var r = Adaptativo.Integrar(Pelo("Runge-Kutta 4"), Problema.Agulha(), 1, 1e-9, 0.01);
        Assert.True(r.MaiorPasso / r.MenorPasso > 50, $"a razao deu {r.MaiorPasso / r.MenorPasso}");
    }

    /// <summary>
    /// O ACHADO: com tolerancia de 1e-6 o Runge-Kutta PULA a agulha inteira.
    ///
    /// O passo cresce cinco vezes por vez enquanto nada acontece, atravessa a
    /// agulha de uma vez e termina reportando sucesso, com erro tres mil vezes
    /// maior que a tolerancia pedida. Controle de passo e local por construcao,
    /// e nenhum controle local sabe o que existe entre dois pontos que ele nunca
    /// avaliou.
    /// </summary>
    [Fact]
    public void OControleDePassoPulaAAgulhaEReportaSucesso()
    {
        var r = Adaptativo.Integrar(Pelo("Runge-Kutta 4"), Problema.Agulha(), 1, 1e-6, 0.01);

        Assert.True(r.Terminou);
        Assert.True(r.Aceitos < 20, $"ele deu {r.Aceitos} passos, entao nao pulou");
        Assert.True(r.PiorErro > 1e-4, $"o erro foi {r.PiorErro}, entao ele viu a agulha");
    }

    /// <summary>
    /// No PICO largo o adaptativo perde para o passo fixo, e isso me corrigiu.
    /// Estimar o erro por duplicacao custa tres passos onde um bastaria, e a
    /// razao de passo do pico nao chega a pagar os tres.
    /// </summary>
    [Fact]
    public void NoPicoLargoOAdaptativoPerdeParaOPassoFixo()
    {
        foreach (var metodo in Metodos.Todos())
        {
            if (metodo.Implicito || metodo.OrdemTeorica == 1) continue;

            var r = Adaptativo.Integrar(metodo, Problema.Pico(), 1, 1e-6);
            var (fixo, _) = Adaptativo.CustoDoPassoFixo(metodo, Problema.Pico(), 1, r.PiorErro);

            Assert.True(fixo < r.Avaliacoes, $"{metodo.Nome}: fixo {fixo}, adaptativo {r.Avaliacoes}");
        }
    }

    /// <summary>
    /// E na agulha ele ganha, com a mesma conta e o mesmo codigo. A diferenca e
    /// so o problema.
    /// </summary>
    [Fact]
    public void NaAgulhaOMesmoCodigoGanha()
    {
        var agulha = Problema.Agulha();

        foreach (var metodo in Metodos.Todos())
        {
            if (metodo.Implicito || metodo.OrdemTeorica == 1) continue;

            var r = Adaptativo.Integrar(metodo, agulha, 1, 1e-9, 0.01);
            var (fixo, _) = Adaptativo.CustoDoPassoFixo(metodo, agulha, 1, r.PiorErro);

            Assert.True(fixo > r.Avaliacoes * 3, $"{metodo.Nome}: fixo {fixo}, adaptativo {r.Avaliacoes}");
        }
    }

    /// <summary>
    /// A tolerancia e por unidade de tempo, entao apertar a tolerancia aperta o
    /// erro: mil vezes mais exigencia, pelo menos cem vezes menos erro.
    /// </summary>
    [Fact]
    public void ApertarAToleranciaApertaOErro()
    {
        var agulha = Problema.Agulha();
        var metodo = Pelo("Heun");

        var frouxo = Adaptativo.Integrar(metodo, agulha, 1, 1e-6, 1e-4).PiorErro;
        var apertado = Adaptativo.Integrar(metodo, agulha, 1, 1e-9, 1e-4).PiorErro;

        Assert.True(apertado * 100 < frouxo, $"frouxo {frouxo}, apertado {apertado}");
    }

    [Fact]
    public void ApertarAToleranciaCustaMaisAvaliacoes()
    {
        var agulha = Problema.Agulha();
        var metodo = Pelo("Runge-Kutta 4");

        Assert.True(Adaptativo.Integrar(metodo, agulha, 1, 1e-9, 1e-4).Avaliacoes >
                    Adaptativo.Integrar(metodo, agulha, 1, 1e-6, 1e-4).Avaliacoes);
    }

    /// <summary>
    /// O fator de ajuste do passo e limitado dos dois lados: nem encolhe mais
    /// que cinco vezes de uma vez, nem cresce mais que cinco. Sem isso, um erro
    /// minusculo mandaria o passo para o infinito num passo so.
    /// </summary>
    [Fact]
    public void OFatorDeAjusteEhLimitadoDosDoisLados()
    {
        Assert.Equal(5, Adaptativo.Fator(0, 1e-6, 4));
        Assert.Equal(5, Adaptativo.Fator(1e-30, 1e-6, 4));
        Assert.Equal(0.2, Adaptativo.Fator(1e30, 1e-6, 4));

        foreach (var erro in new[] { 1e-12, 1e-9, 1e-6, 1e-3, 1.0 })
            Assert.InRange(Adaptativo.Fator(erro, 1e-6, 2), 0.2, 5);
    }

    /// <summary>
    /// E o fator fica abaixo de um quando o erro passa do permitido: e isso que
    /// faz o passo encolher no lugar certo.
    /// </summary>
    [Fact]
    public void OFatorEncolheOPassoQuandoOErroPassa()
    {
        Assert.True(Adaptativo.Fator(1e-5, 1e-6, 2) < 1);
        Assert.True(Adaptativo.Fator(1e-7, 1e-6, 2) > 1);
    }

    /// <summary>
    /// O limite de passos existe porque um metodo de ordem um com tolerancia
    /// apertada pede passo na casa de 1e-10 e a conta deixa de terminar. Melhor
    /// devolver "nao terminou" do que rodar para sempre.
    /// </summary>
    [Fact]
    public void OLimiteDePassosDevolveNaoTerminou()
    {
        var r = Adaptativo.Integrar(Pelo("Euler explicito"), Problema.Agulha(), 1, 1e-9, 0.01, limite: 5_000);

        Assert.False(r.Terminou);
        Assert.Equal(5_000, r.Aceitos + r.Rejeitados);
    }

    /// <summary>
    /// A busca do custo fixo comeca em 128 passos, e nao em um, e o motivo esta
    /// aqui: no problema do pico, o ponto medio com UM passo avalia a funcao
    /// exatamente no centro, onde ela vale zero, nao sai do lugar e termina no
    /// valor certo. Ele acerta por nao andar.
    /// </summary>
    [Fact]
    public void OPontoMedioComUmPassoAcertaOPicoPorNaoAndar()
    {
        var pico = Problema.Pico();
        var pontoMedio = Pelo("ponto medio");

        Assert.Equal(pico.Inicial, Ordem.Integrar(pontoMedio, pico, 1, 1), 12);
        Assert.Equal(pico.Exata(1), Ordem.Integrar(pontoMedio, pico, 1, 1), 12);

        // e com passos de verdade o erro aparece
        Assert.True(Ordem.ErroMaximo(pontoMedio, pico, 1, 128) > 1e-3);
    }
}
