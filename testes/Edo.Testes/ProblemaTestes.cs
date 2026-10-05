using Conde.Edo;
using Xunit;

namespace Conde.Edo.Testes;

/// <summary>
/// O juiz tambem e julgado.
///
/// Todo problema deste repositorio vem com a solucao exata junto, e e contra ela
/// que todo erro e medido. Se uma dessas formulas estivesse errada, o
/// repositorio inteiro estaria medindo com muita precisao a distancia ate a
/// coisa errada, e nada acusaria.
/// </summary>
public class ProblemaTestes
{
    private static IEnumerable<Problema> Todos()
    {
        foreach (var problema in Problema.Todos()) yield return problema;
        yield return Problema.Pico();
        yield return Problema.Agulha();
        yield return Problema.Rigido(10);
    }

    [Fact]
    public void ASolucaoExataComecaNoValorInicial()
    {
        foreach (var problema in Todos())
            Assert.Equal(problema.Inicial, problema.Exata(0), 12);
    }

    /// <summary>
    /// E ela e mesmo solucao: derivada por diferenca central, a formula fechada
    /// bate com f(t, y) em todo ponto conferido.
    /// </summary>
    [Fact]
    public void ASolucaoExataResolveMesmoAEquacao()
    {
        foreach (var problema in Todos())
            foreach (var t in new[] { 0.1, 0.3, 0.5, 0.7, 1.0 })
            {
                var passo = 1e-6;
                var derivada = (problema.Exata(t + passo) - problema.Exata(t - passo)) / (2 * passo);
                Assert.Equal(problema.F(t, problema.Exata(t)), derivada, 5);
            }
    }

    /// <summary>
    /// A derivada em y tambem, que e o que o metodo implicito usa no Newton. Um
    /// erro aqui nao quebraria nada: so faria Newton convergir devagar, o que
    /// passa despercebido.
    /// </summary>
    [Fact]
    public void ADerivadaEmYTambemConfere()
    {
        foreach (var problema in Todos())
        {
            Assert.NotNull(problema.DerivadaEmY);

            foreach (var t in new[] { 0.2, 0.6 })
            {
                var y = problema.Exata(t);
                var passo = 1e-6;
                var derivada = (problema.F(t, y + passo) - problema.F(t, y - passo)) / (2 * passo);
                Assert.Equal(problema.DerivadaEmY!(t, y), derivada, 5);
            }
        }
    }

    /// <summary>
    /// O problema rigido decai para zero e depois nao faz mais nada: em t = 0,01
    /// ja sobrou menos de um decimilesimo, e o intervalo inteiro vai ate um.
    /// </summary>
    [Fact]
    public void ORigidoJaEZeroNoPrimeiroCentesimo()
    {
        var rigido = Problema.Rigido(1000);

        Assert.Equal(1, rigido.Exata(0), 12);
        Assert.True(rigido.Exata(0.01) < 1e-4);
        Assert.True(rigido.Exata(1) < 1e-100);
    }

    /// <summary>
    /// E o pico e mesmo um pico: quase nada nas pontas, um no meio.
    /// </summary>
    [Fact]
    public void OPicoEEstreitoMesmo()
    {
        var pico = Problema.Pico();

        Assert.Equal(1, pico.Exata(0.5), 12);
        Assert.True(pico.Exata(0) < 0.04);
        Assert.True(pico.Exata(1) < 0.04);
        Assert.True(pico.Exata(0.3) < 0.21);
    }

    [Fact]
    public void ASolucaoExataDoOsciladorMantemAEnergiaEmUm()
    {
        foreach (var t in new[] { 0.0, 1.0, 10.0, 1000.0 })
        {
            var (x, v) = Oscilador.Exata(t);
            Assert.Equal(1, Oscilador.Energia(x, v), 10);
        }
    }

    [Fact]
    public void ADerivadaDoOsciladorEOQueDiz()
    {
        Assert.Equal((0.5, -2.0), Oscilador.Derivada(2, 0.5));
    }

    [Fact]
    public void CadaProblemaTemNome()
    {
        foreach (var problema in Todos())
            Assert.False(string.IsNullOrWhiteSpace(problema.ToString()));
    }
}
