using Conde.Edo;
using Xunit;

namespace Conde.Edo.Testes;

/// <summary>
/// O FATOR DE AMPLIFICACAO, que e o numero que decide a estabilidade inteira.
///
/// No problema linear, todo metodo de passo faz a mesma coisa: multiplica o
/// valor por um numero. Esse numero so depende de h vezes lambda, e o metodo e
/// estavel exatamente quando o modulo dele e menor que um. A regiao de
/// estabilidade nao e misterio nenhum: e esse modulo.
/// </summary>
public class AmplificacaoTestes
{
    private static Metodo Pelo(string nome) => Metodos.Todos().First(m => m.Nome == nome);

    /// <summary>
    /// Em h = 2/lambda exatamente, o Euler explicito amplifica por MENOS UM: a
    /// solucao fica pulando entre mais e menos um para sempre, sem crescer e sem
    /// decair. Esse e o ponto exato da fronteira.
    /// </summary>
    [Fact]
    public void NaFronteiraOEulerExplicitoAmplificaPorMenosUm()
    {
        foreach (var lambda in new[] { 10.0, 100.0, 1000.0 })
            Assert.Equal(-1, Estabilidade.Amplificacao(Pelo("Euler explicito"), lambda, 2 / lambda), 10);
    }

    [Fact]
    public void UmFioDeCabeloAcimaDaFronteiraEleDobraACadaPasso()
    {
        var euler = Pelo("Euler explicito");
        var acima = Estabilidade.Amplificacao(euler, 1000, 2.2 / 1000);

        Assert.True(Math.Abs(acima) > 1, $"deu {acima}");
        Assert.False(Estabilidade.Estavel(euler, 1000, 2.2 / 1000));
    }

    [Fact]
    public void OEulerExplicitoAmplificaPorUmMenosHVezesLambda()
    {
        foreach (var passo in new[] { 1e-4, 1e-3, 2e-3 })
            Assert.Equal(1 - passo * 1000, Estabilidade.Amplificacao(Pelo("Euler explicito"), 1000, passo), 10);
    }

    /// <summary>
    /// O fator do implicito e 1/(1 + h*lambda), e ele tem modulo menor que um
    /// para QUALQUER passo positivo, por maior que seja.
    /// </summary>
    [Fact]
    public void OFatorDoImplicitoEMenorQueUmParaQualquerPasso()
    {
        foreach (var passo in new[] { 0.001, 0.1, 1.0, 100.0, 1e6 })
        {
            var fator = Estabilidade.Amplificacao(Pelo("Euler implicito"), 1000, passo);

            Assert.Equal(1 / (1 + passo * 1000), fator, 10);
            Assert.True(Math.Abs(fator) < 1);
        }
    }

    /// <summary>
    /// O ponto medio e o Heun amplificam por 1 + z + z^2/2, entao em z = -2 o
    /// fator da UM: eles estao exatamente na fronteira no mesmo lugar que o
    /// Euler explicito, com o dobro do custo por passo.
    /// </summary>
    [Fact]
    public void OPontoMedioEOHeunEstaoNaMesmaFronteiraDoEuler()
    {
        foreach (var nome in new[] { "ponto medio", "Heun" })
            Assert.Equal(1, Estabilidade.Amplificacao(Pelo(nome), 1000, 2.0 / 1000), 10);
    }

    /// <summary>
    /// E o Runge-Kutta 4 amplifica pelo polinomio de grau quatro da serie da
    /// exponencial truncada, que em z = -2 vale um terco.
    /// </summary>
    [Fact]
    public void ORungeKuttaAmplificaPelaExponencialTruncada()
    {
        foreach (var z in new[] { -0.5, -1.0, -2.0, -2.5 })
        {
            var esperado = 1 + z + z * z / 2 + z * z * z / 6 + z * z * z * z / 24;
            Assert.Equal(esperado, Estabilidade.Amplificacao(Pelo("Runge-Kutta 4"), 1000, -z / 1000), 10);
        }
    }

    /// <summary>
    /// Em z = -2 o implicito e o Runge-Kutta 4 dao o MESMO um terco, e a
    /// coincidencia acaba ali: com o passo cem vezes maior o implicito continua
    /// abaixo de um e o Runge-Kutta ja foi embora.
    /// </summary>
    [Fact]
    public void ACoincidenciaDeUmTercoAcabaForaDaFronteira()
    {
        var implicito = Pelo("Euler implicito");
        var rk = Pelo("Runge-Kutta 4");

        Assert.Equal(Estabilidade.Amplificacao(implicito, 1000, 2.0 / 1000),
                     Estabilidade.Amplificacao(rk, 1000, 2.0 / 1000), 6);

        Assert.True(Math.Abs(Estabilidade.Amplificacao(implicito, 1000, 0.2)) < 1);
        Assert.True(Math.Abs(Estabilidade.Amplificacao(rk, 1000, 0.2)) > 1e6);
    }

    /// <summary>
    /// O modulo do fator e exatamente o criterio: menor que um e o metodo
    /// decai, maior que um e ele cresce. Conferido contra a medida de
    /// estabilidade, que nao olha o fator e sim a conta inteira.
    /// </summary>
    [Fact]
    public void OModuloMenorQueUmEExatamenteOCriterio()
    {
        foreach (var metodo in Metodos.Todos())
            foreach (var passo in new[] { 5e-4, 1e-3, 1.5e-3, 2.5e-3, 3e-3, 1e-2 })
            {
                var cresce = Math.Abs(Estabilidade.Amplificacao(metodo, 1000, passo)) > 1;
                Assert.Equal(!cresce, Estabilidade.Estavel(metodo, 1000, passo));
            }
    }
}
