using Conde.Edo;
using Xunit;

namespace Conde.Edo.Testes;

public class OrdemTestes
{
    private static Metodo Pelo(string nome) => Metodos.Todos().First(m => m.Nome == nome);

    /// <summary>
    /// Cada metodo entrega a ordem que promete, medida contra a solucao exata.
    /// Nenhum numero aqui vem de teoria: todos saem da conta de dividir o passo
    /// pela metade e olhar por quanto o erro caiu.
    /// </summary>
    [Fact]
    public void CadaMetodoEntregaAOrdemQuePromete()
    {
        foreach (var metodo in Metodos.Todos())
            foreach (var problema in Problema.Todos())
            {
                var medida = Ordem.OrdemMedida(metodo, problema, 1, 200);
                Assert.InRange(medida, metodo.OrdemTeorica - 0.1, metodo.OrdemTeorica + 0.1);
            }
    }

    /// <summary>
    /// Os DOIS Euler sao de ordem um. O implicito custa Newton a cada passo para
    /// chegar na mesma ordem, e o que ele compra nao e precisao.
    /// </summary>
    [Fact]
    public void OsDoisEulerTemAMesmaOrdem()
    {
        var problema = Problema.Gaussiana();

        Assert.InRange(Ordem.OrdemMedida(Pelo("Euler explicito"), problema, 1, 200), 0.9, 1.1);
        Assert.InRange(Ordem.OrdemMedida(Pelo("Euler implicito"), problema, 1, 200), 0.9, 1.1);
    }

    /// <summary>
    /// Dividir o passo pela metade divide o erro por 2 elevado a ordem. Esta e a
    /// definicao inteira, conferida direto em vez de pelo logaritmo.
    /// </summary>
    [Fact]
    public void DividirOPassoPelaMetadeDivideOErroPorDoisElevadoAOrdem()
    {
        var problema = Problema.Exponencial();

        foreach (var metodo in Metodos.Todos())
        {
            var grosso = Ordem.Erro(metodo, problema, 1, 100);
            var fino = Ordem.Erro(metodo, problema, 1, 200);
            var esperado = Math.Pow(2, metodo.OrdemTeorica);

            Assert.InRange(grosso / fino, esperado * 0.93, esperado * 1.07);
        }
    }

    [Fact]
    public void MaisPassosErraMenos()
    {
        var problema = Problema.Mista();

        foreach (var metodo in Metodos.Todos())
            Assert.True(Ordem.Erro(metodo, problema, 1, 400) < Ordem.Erro(metodo, problema, 1, 50),
                        $"{metodo.Nome} nao melhorou com mais passos");
    }

    /// <summary>
    /// O Runge-Kutta com cem passos erra menos que o Euler com um milhao. Quatro
    /// avaliacoes por passo nao e caro: e barato.
    /// </summary>
    [Fact]
    public void ORungeKuttaComCemPassosGanhaDoEulerComUmMilhao()
    {
        var problema = Problema.Gaussiana();

        Assert.True(Ordem.Erro(Pelo("Runge-Kutta 4"), problema, 1, 100) <
                    Ordem.Erro(Pelo("Euler explicito"), problema, 1, 1_000_000));
    }

    /// <summary>
    /// E ganha gastando MENOS avaliacoes, que e a comparacao justa: ordem quatro
    /// com quatro avaliacoes por passo compra mais que ordem um com uma.
    /// </summary>
    [Fact]
    public void EGanhaGastandoMenosAvaliacoes()
    {
        var problema = Problema.Gaussiana();
        var euler = Pelo("Euler explicito");
        var rk = Pelo("Runge-Kutta 4");

        var alvo = 1e-8;

        var passosEuler = 1;
        while (Ordem.Erro(euler, problema, 1, passosEuler) > alvo) passosEuler *= 2;

        var passosRk = 1;
        while (Ordem.Erro(rk, problema, 1, passosRk) > alvo) passosRk *= 2;

        var custoEuler = (long)passosEuler * euler.AvaliacoesPorPasso;
        var custoRk = (long)passosRk * rk.AvaliacoesPorPasso;

        Assert.True(custoRk * 1000 < custoEuler, $"Euler {custoEuler}, Runge-Kutta {custoRk}");
    }

    /// <summary>
    /// O ponto medio e o Heun tem a mesma ordem e o mesmo custo por passo. A
    /// diferenca entre eles e so onde a segunda avaliacao acontece.
    /// </summary>
    [Fact]
    public void OPontoMedioEOHeunEmpatam()
    {
        var problema = Problema.Gaussiana();

        var pontoMedio = Ordem.Erro(Pelo("ponto medio"), problema, 1, 100);
        var heun = Ordem.Erro(Pelo("Heun"), problema, 1, 100);

        Assert.InRange(pontoMedio / heun, 0.1, 10);
    }

    [Fact]
    public void OErroEhSempreNaoNegativo()
    {
        foreach (var metodo in Metodos.Todos())
            foreach (var problema in Problema.Todos())
                Assert.True(Ordem.Erro(metodo, problema, 1, 10) >= 0);
    }

    /// <summary>
    /// Integrar ate zero nao anda, entao devolve o valor inicial sem passo
    /// nenhum. O caso de borda existe porque a adaptativa chama com o que sobrou
    /// do intervalo, e o que sobra pode ser zero.
    /// </summary>
    [Fact]
    public void IntegrarAteZeroDevolveOInicial()
    {
        var problema = Problema.Gaussiana();
        Assert.Equal(problema.Inicial, Ordem.Integrar(Pelo("Heun"), problema, 0, 10), 12);
    }
}
