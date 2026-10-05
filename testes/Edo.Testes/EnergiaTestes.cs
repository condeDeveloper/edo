using Conde.Edo;
using Xunit;

namespace Conde.Edo.Testes;

/// <summary>
/// A ENERGIA do oscilador, que e a medida que separa os tres metodos.
///
/// Ela vale um na solucao exata e fica em um para sempre. Nenhum dos tres
/// metodos sabe disso: nenhum deles tem a palavra energia escrita em lugar
/// nenhum. Dois deles destroem a constante em direcoes opostas, e o terceiro a
/// mantem, de graca.
/// </summary>
public class EnergiaTestes
{
    private static Orbita Pela(string nome) => Orbita.Todas().First(o => o.Nome == nome);

    /// <summary>
    /// O Euler explicito GANHA energia: a orbita abre numa espiral que vai para
    /// o infinito. Ele simula um pendulo que acelera sozinho.
    /// </summary>
    [Fact]
    public void OExplicitoGanhaEnergia()
    {
        var (final, _, menor) = Orbita.Rodar(Pela("Euler explicito"), 200, 0.05);

        Assert.True(final > 20_000, $"a energia terminou em {final}");
        Assert.Equal(1, menor, 10);
    }

    /// <summary>
    /// O implicito PERDE energia: a orbita fecha numa espiral que morre no
    /// centro. Ele simula um pendulo que para, sem atrito nenhum no problema.
    /// </summary>
    [Fact]
    public void OImplicitoPerdeEnergia()
    {
        var (final, maior, _) = Orbita.Rodar(Pela("Euler implicito"), 200, 0.05);

        Assert.True(final < 1e-4, $"a energia terminou em {final}");
        Assert.Equal(1, maior, 10);
    }

    /// <summary>
    /// E o SIMPLETICO, que e o explicito com uma linha trocada de lugar, mantem
    /// a energia oscilando em volta de um. Para sempre.
    /// </summary>
    [Fact]
    public void OSimpleticoMantemAEnergiaLimitada()
    {
        var (final, maior, menor) = Orbita.Rodar(Pela("Euler simpletico"), 200, 0.05);

        Assert.InRange(final, 0.97, 1.03);
        Assert.InRange(maior, 1.0, 1.05);
        Assert.InRange(menor, 0.95, 1.0);
    }

    /// <summary>
    /// E o tempo nao estraga: em dez mil, com duzentos mil passos, a energia do
    /// simpletico continua no mesmo lugar.
    /// </summary>
    [Fact]
    public void OTempoNaoEstragaOSimpletico()
    {
        foreach (var ate in new[] { 10, 100, 1000, 10000 })
            Assert.InRange(Orbita.Rodar(Pela("Euler simpletico"), ate, 0.05).Final, 0.97, 1.03);
    }

    [Fact]
    public void EOTempoEstragaAsOutrasDuasCadaVezMais()
    {
        double antesExplicito = 0, antesImplicito = 2;

        foreach (var ate in new[] { 10, 100, 1000 })
        {
            var subindo = Orbita.Rodar(Pela("Euler explicito"), ate, 0.05).Final;
            var descendo = Orbita.Rodar(Pela("Euler implicito"), ate, 0.05).Final;

            Assert.True(subindo > antesExplicito, $"em t = {ate} o explicito parou de subir");
            Assert.True(descendo < antesImplicito, $"em t = {ate} o implicito parou de descer");

            antesExplicito = subindo;
            antesImplicito = descendo;
        }
    }

    /// <summary>
    /// A oscilacao da energia do simpletico encolhe com o passo, e o desvio e
    /// proporcional a h: metade do passo, metade do desvio. Ele nao conserva a
    /// energia exata, conserva uma energia vizinha, e e isso que basta.
    /// </summary>
    [Fact]
    public void ODesvioDoSimpleticoEncolheComOPasso()
    {
        var grosso = Orbita.Rodar(Pela("Euler simpletico"), 200, 0.05);
        var fino = Orbita.Rodar(Pela("Euler simpletico"), 200, 0.025);

        var desvioGrosso = grosso.Maior - grosso.Menor;
        var desvioFino = fino.Maior - fino.Menor;

        Assert.InRange(desvioGrosso / desvioFino, 1.8, 2.2);
    }

    /// <summary>
    /// E com passo menor as outras duas continuam indo embora, so que mais
    /// devagar: nao e questao de refinar, e de direcao.
    /// </summary>
    [Fact]
    public void RefinarOPassoNaoMudaADirecaoDasOutrasDuas()
    {
        foreach (var passo in new[] { 0.05, 0.01, 0.001 })
        {
            Assert.True(Orbita.Rodar(Pela("Euler explicito"), 200, passo).Final > 1);
            Assert.True(Orbita.Rodar(Pela("Euler implicito"), 200, passo).Final < 1);
        }
    }

    [Fact]
    public void RodarSemPassoNenhumDevolveAEnergiaInicial()
    {
        var (final, maior, menor) = Orbita.Rodar(Pela("Euler simpletico"), 0, 0.05);

        Assert.Equal(1, final, 12);
        Assert.Equal(1, maior, 12);
        Assert.Equal(1, menor, 12);
    }
}
