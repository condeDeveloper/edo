using Conde.Edo;
using Xunit;

namespace Conde.Edo.Testes;

public class OrbitaTestes
{
    private static Orbita Pela(string nome) => Orbita.Todas().First(o => o.Nome == nome);

    /// <summary>
    /// O ACHADO: o simpletico e o PIOR dos tres no erro de posicao em tempo
    /// curto, que e a medida com que metodo costuma ser escolhido.
    ///
    /// Os tres sao de ordem um, e medir erro em tempo curto nao distingue de
    /// jeito nenhum um metodo que conserva de um que explode. Escolher por
    /// tabela de erro e exatamente como se chega no metodo errado para
    /// simulacao longa.
    /// </summary>
    [Fact]
    public void OSimpleticoEOPiorDosTresNoErroDeCurtoPrazo()
    {
        var erroSimpletico = Orbita.ErroDePosicao(Pela("Euler simpletico"), 1, 0.05);

        Assert.True(erroSimpletico > Orbita.ErroDePosicao(Pela("Euler explicito"), 1, 0.05));
        Assert.True(erroSimpletico > Orbita.ErroDePosicao(Pela("Euler implicito"), 1, 0.05));
    }

    /// <summary>
    /// E os tres sao de ordem um, inclusive o simpletico: ele nao e mais
    /// preciso, ele e de outra natureza.
    /// </summary>
    [Fact]
    public void OsTresSaoDeOrdemUmNaPosicao()
    {
        foreach (var orbita in Orbita.Todas())
        {
            var grosso = Orbita.ErroDePosicao(orbita, 1, 0.01);
            var fino = Orbita.ErroDePosicao(orbita, 1, 0.005);

            Assert.InRange(Math.Log2(grosso / fino), 0.9, 1.1);
        }
    }

    /// <summary>
    /// A diferenca entre o explicito e o simpletico e UMA LINHA: usar a
    /// velocidade nova em vez da velha para andar a posicao.
    /// </summary>
    [Fact]
    public void ADiferencaEntreExplicitoESimpleticoEUmaLinha()
    {
        var (xe, ve) = Orbita.Explicito(1, 0, 0.1);
        var (xs, vs) = Orbita.Simpletico(1, 0, 0.1);

        // a velocidade sai igual nos dois
        Assert.Equal(ve, vs, 12);

        // e a posicao difere exatamente por h vezes a mudanca de velocidade
        Assert.Equal(xe + 0.1 * (vs - 0), xs, 12);
    }

    [Fact]
    public void OImplicitoFechadoResolveMesmoOSistema()
    {
        var (x, v) = Orbita.Implicito(1, 0, 0.1);

        // x1 = x0 + h*v1 e v1 = v0 - h*x1, conferidos de volta
        Assert.Equal(1 + 0.1 * v, x, 12);
        Assert.Equal(0 - 0.1 * x, v, 12);
    }

    [Fact]
    public void OExplicitoEOQueDiz()
    {
        var (x, v) = Orbita.Explicito(1, 2, 0.1);

        Assert.Equal(1 + 0.1 * 2, x, 12);
        Assert.Equal(2 - 0.1 * 1, v, 12);
    }

    /// <summary>
    /// Com passo minusculo os tres se aproximam do circulo exato, e e por isso
    /// que a diferenca entre eles nao aparece em tempo curto nenhum.
    /// </summary>
    [Fact]
    public void ComPassoMinusculoOsTresSeAproximamDoCirculo()
    {
        foreach (var orbita in Orbita.Todas())
            Assert.True(Orbita.ErroDePosicao(orbita, 1, 1e-5) < 1e-4,
                        $"{orbita.Nome} nao se aproximou");
    }

    [Fact]
    public void SaoTresOrbitasComNome()
    {
        var nomes = Orbita.Todas().Select(o => o.Nome).ToList();

        Assert.Equal(3, nomes.Count);
        Assert.Equal(nomes.Count, nomes.Distinct().Count());
        Assert.All(Orbita.Todas(), o => Assert.Equal(o.Nome, o.ToString()));
    }

    [Fact]
    public void AEnergiaEhASomaDosQuadrados()
    {
        Assert.Equal(25, Oscilador.Energia(3, 4), 12);
        Assert.Equal(0, Oscilador.Energia(0, 0), 12);
    }
}
