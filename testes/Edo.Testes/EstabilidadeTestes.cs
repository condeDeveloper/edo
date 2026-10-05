using Conde.Edo;
using Xunit;

namespace Conde.Edo.Testes;

public class EstabilidadeTestes
{
    private static Metodo Pelo(string nome) => Metodos.Todos().First(m => m.Nome == nome);

    /// <summary>
    /// O limite do Euler explicito e 2/lambda, e ele nao vem de tabela: vem de
    /// que o metodo multiplica por (1 - h*lambda) a cada passo.
    /// </summary>
    [Fact]
    public void OLimiteDoEulerExplicitoEDoisSobreLambda()
    {
        var euler = Pelo("Euler explicito");

        foreach (var lambda in new[] { 10.0, 100.0, 1000.0 })
        {
            var medido = Estabilidade.MaiorPassoEstavel(euler, lambda);
            Assert.InRange(medido, 2 / lambda * 0.999, 2 / lambda * 1.001);
        }
    }

    /// <summary>
    /// O criterio importa, e o primeiro que escrevi estava errado: eu media
    /// "explodir" por limiar, o valor nao pode passar de dez. Com lambda igual a
    /// dez o intervalo tem quatro passos, e um fator de 1,78 por passo termina em
    /// 9,99, abaixo do limiar. O limite medido dava 0,278 em vez de 0,2.
    ///
    /// Este teste prende o conserto: medir CRESCIMENTO por passo nao depende do
    /// tamanho do intervalo, entao o limite tem que ser o mesmo medindo ate 1,
    /// ate 0,05 ou ate 10.
    /// </summary>
    [Fact]
    public void OLimiteNaoDependeDoTamanhoDoIntervalo()
    {
        var euler = Pelo("Euler explicito");

        foreach (var ate in new[] { 0.05, 1.0, 10.0 })
            Assert.InRange(Estabilidade.MaiorPassoEstavel(euler, 10, ate), 0.1998, 0.2002);
    }

    /// <summary>
    /// O Euler IMPLICITO nao tem limite nenhum, e esse e o motivo inteiro de ele
    /// existir apesar de ser so de ordem um.
    /// </summary>
    [Fact]
    public void OEulerImplicitoNaoTemLimiteDePasso()
    {
        var implicito = Pelo("Euler implicito");

        Assert.True(double.IsInfinity(Estabilidade.MaiorPassoEstavel(implicito, 1000)));
        Assert.True(Estabilidade.Estavel(implicito, 1_000_000, 1));
    }

    /// <summary>
    /// Todo metodo EXPLICITO tem limite, por mais preciso que seja. A ordem nao
    /// compra estabilidade.
    /// </summary>
    [Fact]
    public void TodoMetodoExplicitoTemLimite()
    {
        foreach (var metodo in Metodos.Todos())
        {
            if (metodo.Implicito) continue;
            Assert.True(double.IsFinite(Estabilidade.MaiorPassoEstavel(metodo, 1000)),
                        $"{metodo.Nome} apareceu sem limite");
        }
    }

    /// <summary>
    /// E os tres de ordem um e dois tem o MESMO limite: duas avaliacoes por
    /// passo compram ordem, e estabilidade nenhuma.
    /// </summary>
    [Fact]
    public void OrdemDoisNaoCompraFolgaNenhuma()
    {
        var euler = Estabilidade.MaiorPassoEstavel(Pelo("Euler explicito"), 1000);

        foreach (var nome in new[] { "ponto medio", "Heun" })
            Assert.Equal(euler, Estabilidade.MaiorPassoEstavel(Pelo(nome), 1000), 9);
    }

    /// <summary>
    /// O Runge-Kutta 4 tem a maior regiao de estabilidade dos explicitos, e ela
    /// e 2,785/lambda, nao 4 nem 8: ordem quatro nao vira limite quatro vezes
    /// maior.
    /// </summary>
    [Fact]
    public void ORungeKuttaGanhaPoucoMaisQueUmTercoDeFolga()
    {
        var medido = Estabilidade.MaiorPassoEstavel(Pelo("Runge-Kutta 4"), 1000);
        Assert.InRange(medido * 1000, 2.6, 2.9);
    }

    /// <summary>
    /// E e isso que custa a rigidez: centenas de passos para atravessar um
    /// intervalo em que a solucao ja e zero desde o primeiro centesimo.
    /// </summary>
    [Fact]
    public void ARigidezCustaCentenasDePassosParaUmaSolucaoQueJaEZero()
    {
        Assert.True(Estabilidade.PassosNecessarios(Pelo("Euler explicito"), 1000, 1) >= 500);
        Assert.Equal(1, Estabilidade.PassosNecessarios(Pelo("Euler implicito"), 1000, 1));
        Assert.True(Problema.Rigido(1000).Exata(0.01) < 1e-4);
    }

    /// <summary>
    /// Quanto mais rigido o problema, pior fica: dobrar lambda corta o passo
    /// pela metade e dobra o numero de passos, sem ganho nenhum de precisao.
    /// </summary>
    [Fact]
    public void DobrarARigidezDobraOTrabalhoDoExplicito()
    {
        var euler = Pelo("Euler explicito");

        var mil = Estabilidade.PassosNecessarios(euler, 1000, 1);
        var doisMil = Estabilidade.PassosNecessarios(euler, 2000, 1);

        Assert.InRange(doisMil / mil, 1.9, 2.1);
    }

    /// <summary>
    /// E o implicito atravessa o mesmo problema com um passo so, por mais
    /// rigido que ele seja.
    /// </summary>
    [Fact]
    public void OImplicitoAtravessaQualquerRigidezComUmPasso()
    {
        foreach (var lambda in new[] { 1e3, 1e6, 1e9 })
            Assert.Equal(1, Estabilidade.PassosNecessarios(Pelo("Euler implicito"), lambda, 1));
    }

    [Fact]
    public void PassoNaoPositivoNaoExplodeNaMedida()
    {
        Assert.True(Estabilidade.Estavel(Pelo("Euler explicito"), 1000, 0));
    }
}
