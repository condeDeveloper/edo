namespace Conde.Edo;

public static class Metodos
{
    /// <summary>
    /// EULER EXPLICITO, de 1768: anda na direcao da inclinacao de agora.
    ///
    /// E o metodo mais simples que existe e o unico que da para fazer de cabeca.
    /// Ordem um: dividir o passo pela metade divide o erro pela metade, o que e
    /// uma troca ruim, porque dobra o trabalho para ganhar um bit.
    /// </summary>
    public static double EulerExplicito(Func<double, double, double> f, Func<double, double, double>? _,
                                        double t, double y, double h) => y + h * f(t, y);

    /// <summary>
    /// EULER IMPLICITO: anda na direcao da inclinacao DE CHEGADA, que ainda nao
    /// se conhece.
    ///
    /// Isso parece impossivel e e so uma equacao: y1 = y0 + h*f(t+h, y1), com y1
    /// dos dois lados. Resolver custa Newton a cada passo, e a ordem continua
    /// sendo UM. Pagar mais para ganhar a mesma ordem parece burrice, e nao e: o
    /// que se compra e ESTABILIDADE, e a medida mostra o que isso vale.
    /// </summary>
    public static double EulerImplicito(Func<double, double, double> f, Func<double, double, double>? df,
                                        double t, double y, double h)
    {
        var chute = y + h * f(t, y);

        for (var volta = 0; volta < 60; volta++)
        {
            var resto = chute - y - h * f(t + h, chute);

            // Com a derivada, Newton; sem ela, ponto fixo. A diferenca aparece
            // justamente no problema rigido: ali o ponto fixo so converge com
            // passo pequeno, que e exatamente o que o metodo implicito existe
            // para evitar.
            if (df is not null)
            {
                var inclinacao = 1 - h * df(t + h, chute);
                if (Math.Abs(inclinacao) < 1e-14) break;
                var proximo = chute - resto / inclinacao;
                if (Math.Abs(proximo - chute) < 1e-14) return proximo;
                chute = proximo;
            }
            else
            {
                var proximo = y + h * f(t + h, chute);
                if (Math.Abs(proximo - chute) < 1e-14) return proximo;
                chute = proximo;
            }
        }

        return chute;
    }

    /// <summary>
    /// PONTO MEDIO: usa a inclinacao do MEIO do intervalo, estimada por um meio
    /// passo de Euler.
    ///
    /// Duas avaliacoes e ordem dois. O ganho vem de simetria: o erro de ordem h
    /// que o Euler comete de um lado do meio se cancela com o do outro.
    /// </summary>
    public static double PontoMedio(Func<double, double, double> f, Func<double, double, double>? _,
                                    double t, double y, double h)
    {
        var meio = y + h / 2 * f(t, y);
        return y + h * f(t + h / 2, meio);
    }

    /// <summary>
    /// HEUN, ou Euler melhorado: a MEDIA entre a inclinacao de agora e a de
    /// chegada estimada.
    ///
    /// Duas avaliacoes e ordem dois, igual ao ponto medio, e sem resolver
    /// equacao nenhuma. Ele e o Euler implicito com o lado direito chutado em
    /// vez de resolvido, e isso custa a estabilidade: ele volta a ser explicito.
    /// </summary>
    public static double Heun(Func<double, double, double> f, Func<double, double, double>? _,
                              double t, double y, double h)
    {
        var inclinacao = f(t, y);
        var chegada = y + h * inclinacao;
        return y + h / 2 * (inclinacao + f(t + h, chegada));
    }

    /// <summary>
    /// RUNGE E KUTTA de quarta ordem, de 1901: quatro avaliacoes, ordem quatro.
    ///
    /// Ele e o metodo padrao do mundo inteiro, e os pesos 1, 2, 2, 1 divididos
    /// por seis nao sao escolha estetica: eles sao a unica combinacao que faz os
    /// termos de erro ate ordem quatro se cancelarem.
    ///
    /// Quatro avaliacoes por passo parece caro perto do Euler, e a medida mostra
    /// que e o oposto: para o mesmo erro, ele gasta ordens de grandeza menos.
    /// </summary>
    public static double RungeKutta(Func<double, double, double> f, Func<double, double, double>? _,
                                    double t, double y, double h)
    {
        var k1 = f(t, y);
        var k2 = f(t + h / 2, y + h / 2 * k1);
        var k3 = f(t + h / 2, y + h / 2 * k2);
        var k4 = f(t + h, y + h * k3);
        return y + h / 6 * (k1 + 2 * k2 + 2 * k3 + k4);
    }

    public static IEnumerable<Metodo> Todos()
    {
        yield return new Metodo("Euler explicito", 1, 1, false, EulerExplicito);
        yield return new Metodo("Euler implicito", 1, 1, true, EulerImplicito);
        yield return new Metodo("ponto medio", 2, 2, false, PontoMedio);
        yield return new Metodo("Heun", 2, 2, false, Heun);
        yield return new Metodo("Runge-Kutta 4", 4, 4, false, RungeKutta);
    }
}
