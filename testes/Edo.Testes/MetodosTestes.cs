using Conde.Edo;
using Xunit;

namespace Conde.Edo.Testes;

/// <summary>
/// Um passo de cada metodo, conferido na mao.
///
/// Erro de ordem aparece na medida de ordem; erro de SINAL ou de peso, nao: um
/// metodo com o peso errado continua convergindo, so que para o lugar errado, e
/// a curva continua plausivel. Por isso cada formula e conferida contra uma
/// conta feita a mao, com numero redondo.
/// </summary>
public class MetodosTestes
{
    private static Metodo Pelo(string nome) => Metodos.Todos().First(m => m.Nome == nome);

    [Fact]
    public void UmPassoDeEulerEExatamenteAFormula()
    {
        var problema = Problema.Exponencial();

        // y' = y, y0 = 1, h = 0,5: a inclinacao e 1, entao y vira 1,5.
        Assert.Equal(1.5, Metodos.EulerExplicito(problema.F, null, 0, 1, 0.5), 12);
    }

    [Fact]
    public void UmPassoDeHeunEAMediaDasDuasInclinacoes()
    {
        var problema = Problema.Exponencial();

        // a inclinacao em casa e 1, a de chegada estimada e 1,5, a media e 1,25,
        // entao y vira 1 + 0,5 * 1,25.
        Assert.Equal(1.625, Metodos.Heun(problema.F, null, 0, 1, 0.5), 12);
    }

    [Fact]
    public void UmPassoDoPontoMedioUsaAInclinacaoDoMeio()
    {
        var problema = Problema.Exponencial();

        // meio passo de Euler leva a 1,25, e a inclinacao la e 1,25.
        Assert.Equal(1 + 0.5 * 1.25, Metodos.PontoMedio(problema.F, null, 0, 1, 0.5), 12);
    }

    /// <summary>
    /// Os pesos 1, 2, 2, 1 divididos por seis do Runge-Kutta nao sao escolha
    /// estetica: sao a unica combinacao que cancela os termos de erro ate ordem
    /// quatro. Aqui eles sao conferidos pela definicao.
    /// </summary>
    [Fact]
    public void ORungeKuttaUsaOsPesosUmDoisDoisUm()
    {
        var problema = Problema.Gaussiana();
        double t = 0.3, y = 0.9, h = 0.2;

        var k1 = problema.F(t, y);
        var k2 = problema.F(t + h / 2, y + h / 2 * k1);
        var k3 = problema.F(t + h / 2, y + h / 2 * k2);
        var k4 = problema.F(t + h, y + h * k3);

        Assert.Equal(y + h / 6 * (k1 + 2 * k2 + 2 * k3 + k4),
                     Metodos.RungeKutta(problema.F, null, t, y, h), 12);
    }

    /// <summary>
    /// O implicito resolve a equacao de verdade: y1 = y0 + h*y1 tem solucao
    /// fechada y0/(1 - h), e Newton tem que cair exatamente nela.
    /// </summary>
    [Fact]
    public void OImplicitoResolveAEquacaoENaoChuta()
    {
        var problema = Problema.Exponencial();
        var achado = Metodos.EulerImplicito(problema.F, problema.DerivadaEmY, 0, 1, 0.25);

        Assert.Equal(1 / (1 - 0.25), achado, 10);
    }

    /// <summary>
    /// E resolve mesmo sem a derivada, caindo no ponto fixo. A diferenca entre
    /// os dois aparece no problema rigido, nao aqui.
    /// </summary>
    [Fact]
    public void OImplicitoTambemResolveSemADerivada()
    {
        var problema = Problema.Exponencial();
        Assert.Equal(1 / (1 - 0.25), Metodos.EulerImplicito(problema.F, null, 0, 1, 0.25), 10);
    }

    /// <summary>
    /// No problema rigido com passo grande, o PONTO FIXO do implicito nao
    /// converge: ele so converge quando h vezes lambda e menor que um, que e
    /// quase o limite que o metodo implicito existe para evitar. Com a derivada,
    /// Newton acerta na primeira, porque a equacao e linear.
    /// </summary>
    [Fact]
    public void NoRigidoComPassoGrandeOPontoFixoNaoAjuda()
    {
        var rigido = Problema.Rigido(1000);

        var comNewton = Metodos.EulerImplicito(rigido.F, rigido.DerivadaEmY, 0, 1, 0.1);
        Assert.Equal(1.0 / (1 + 0.1 * 1000), comNewton, 12);

        var semDerivada = Metodos.EulerImplicito(rigido.F, null, 0, 1, 0.1);
        Assert.True(Math.Abs(semDerivada) > 1e10 || double.IsInfinity(semDerivada),
                    $"o ponto fixo convergiu quando nao devia, deu {semDerivada}");
    }

    [Fact]
    public void IntegrarComUmPassoSoEUmPassoSo()
    {
        var problema = Problema.Exponencial();
        Assert.Equal(2, Ordem.Integrar(Pelo("Euler explicito"), problema, 1, 1), 12);
    }

    [Fact]
    public void CadaMetodoDeclaraQuantoCusta()
    {
        foreach (var metodo in Metodos.Todos())
        {
            Assert.True(metodo.AvaliacoesPorPasso >= 1);
            Assert.True(metodo.OrdemTeorica >= 1);
            Assert.False(string.IsNullOrWhiteSpace(metodo.ToString()));
        }

        Assert.Equal(4, Pelo("Runge-Kutta 4").AvaliacoesPorPasso);
        Assert.Single(Metodos.Todos(), m => m.Implicito);
    }
}
