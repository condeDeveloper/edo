using Conde.Edo;

namespace Conde.Edo.Medidor;

/// <summary>
/// As medidas. Ordem medida contra a solucao exata e contagem de avaliacoes:
/// nenhuma depende de relogio nem de maquina.
/// </summary>
public static class Programa
{
    public static int Main(string[] argumentos)
    {
        var qual = argumentos.Length > 0 ? argumentos[0] : "tudo";
        switch (qual)
        {
            case "ordem": MedirOrdem(); break;
            case "custo": Custo(); break;
            case "rigidez": Rigidez(); break;
            case "energia": Energia(); break;
            case "longo": Longo(); break;
            case "adaptativo": Adaptativa(); break;
            case "tudo": MedirOrdem(); Custo(); Rigidez(); Energia(); Longo(); Adaptativa(); break;
            default:
                Console.Error.WriteLine("medidas: ordem, custo, rigidez, energia, longo, adaptativo, tudo");
                return 1;
        }
        return 0;
    }

    /// <summary>A ordem de cada metodo, medida em vez de citada.</summary>
    private static void MedirOrdem()
    {
        Console.WriteLine("== a ordem de cada metodo, medida contra a solucao exata ==");
        Console.Write($"{"metodo",-20}{"teorica",9}");
        foreach (var problema in Problema.Todos()) Console.Write($"{problema.Nome,14}");
        Console.WriteLine();

        foreach (var metodo in Metodos.Todos())
        {
            Console.Write($"{metodo.Nome,-20}{metodo.OrdemTeorica,9}");
            foreach (var problema in Problema.Todos())
                Console.Write($"{Ordem.OrdemMedida(metodo, problema, 1, 200),14:N2}");
            Console.WriteLine();
        }

        Console.WriteLine();
        Console.WriteLine("a ordem nao sai de teoria nenhuma aqui: ela sai da conta. Dividir o passo");
        Console.WriteLine("pela metade divide o erro por 2 elevado a ordem, entao o logaritmo da razao");
        Console.WriteLine("na base dois E a ordem, e os numeros batem com a teoria ate a segunda casa");
        Console.WriteLine();
        Console.WriteLine("os dois Euler sao de ordem UM, e o implicito custa Newton a cada passo para");
        Console.WriteLine("chegar na mesma ordem. Pagar mais pelo mesmo parece burrice, e o que ele");
        Console.WriteLine("compra aparece na medida da rigidez");
        Console.WriteLine();
        Console.WriteLine("a medida so vale numa faixa de passo: grande demais e o termo dominante do");
        Console.WriteLine("erro deixa de mandar, pequeno demais e o arredondamento manda. Fora dela o");
        Console.WriteLine("numero deixa de significar o que diz");
        Console.WriteLine();
    }

    /// <summary>O custo para o mesmo erro, que e a comparacao justa.</summary>
    private static void Custo()
    {
        Console.WriteLine("== quantas avaliacoes para chegar no mesmo erro ==");
        Console.WriteLine("  o problema e y' = -2ty, de 0 a 1");
        Console.WriteLine();
        Console.WriteLine($"{"metodo",-20}{"erro alvo",12}{"passos",12}{"avaliacoes",14}");

        var problema = Problema.Gaussiana();

        foreach (var alvo in new[] { 1e-4, 1e-8 })
        {
            foreach (var metodo in Metodos.Todos())
            {
                if (metodo.Implicito) continue;

                var passos = 1;
                while (passos < 50_000_000 && Ordem.Erro(metodo, problema, 1, passos) > alvo) passos *= 2;

                Console.WriteLine($"{metodo.Nome,-20}{alvo,12:E0}{passos,12:N0}" +
                                  $"{(long)passos * metodo.AvaliacoesPorPasso,14:N0}");
            }
            Console.WriteLine();
        }

        Console.WriteLine("quatro avaliacoes por passo parece caro perto de uma, e e o oposto: para");
        Console.WriteLine("errar menos que um centesimo de milionesimo, o Runge-Kutta gasta menos");
        Console.WriteLine("avaliacoes que o Euler gasta para errar um decimilesimo");
        Console.WriteLine();
        Console.WriteLine("e a distancia CRESCE com a exigencia. Cada digito a mais custa dez vezes");
        Console.WriteLine("mais passos no Euler e a raiz quarta de dez no Runge-Kutta");
        Console.WriteLine();
    }

    /// <summary>A rigidez: quando o passo e ditado pela estabilidade.</summary>
    private static void Rigidez()
    {
        Console.WriteLine("== rigidez: quando o passo e ditado pela estabilidade, nao pela precisao ==");
        Console.WriteLine("  o problema e y' = -1000y, e a solucao vira uma linha reta em zero antes de t = 0,01");
        Console.WriteLine();
        Console.WriteLine($"{"metodo",-20}{"maior passo estavel",22}{"passos ate t = 1",18}");

        foreach (var metodo in Metodos.Todos())
        {
            var maior = Estabilidade.MaiorPassoEstavel(metodo, 1000);
            var texto = double.IsInfinity(maior) ? "sem limite" : maior.ToString("E3");
            Console.WriteLine($"{metodo.Nome,-20}{texto,22}{Estabilidade.PassosNecessarios(metodo, 1000, 1),18:N0}");
        }

        Console.WriteLine();
        Console.WriteLine("o Euler explicito precisa de quinhentos passos para atravessar um intervalo");
        Console.WriteLine("em que a solucao ja e zero depois do primeiro centesimo. Nao e precisao: e");
        Console.WriteLine("que com passo maior ele EXPLODE");
        Console.WriteLine();
        Console.WriteLine("o limite da para deduzir numa linha. O Euler explicito faz y novo igual a y");
        Console.WriteLine("vezes (1 - h*lambda), entao ele multiplica por esse fator a cada passo, e o");
        Console.WriteLine("modulo precisa ser menor que um: h menor que 2 sobre lambda");
        Console.WriteLine();
        Console.WriteLine($"  o fator de amplificacao em h = 2/lambda, que e a fronteira:");

        foreach (var metodo in Metodos.Todos())
            Console.WriteLine($"  {metodo.Nome,-20}{Estabilidade.Amplificacao(metodo, 1000, 2.0 / 1000),14:N6}");

        Console.WriteLine();
        Console.WriteLine("  o Euler explicito da exatamente MENOS UM ali: a solucao fica pulando entre");
        Console.WriteLine("  mais e menos um para sempre, sem crescer e sem decair. Um fio de cabelo");
        Console.WriteLine("  acima disso e ela dobra a cada passo");
        Console.WriteLine();
        Console.WriteLine("  o Euler implicito faz y novo igual a y dividido por (1 + h*lambda), e esse");
        Console.WriteLine("  fator tem modulo menor que um para QUALQUER passo. Ele nao tem limite, e e");
        Console.WriteLine("  exatamente por isso que ele existe apesar de ser so de ordem um");
        Console.WriteLine();
    }

    /// <summary>A energia do oscilador, que e o achado do repositorio.</summary>
    private static void Energia()
    {
        Console.WriteLine("== o oscilador harmonico: a energia que deveria ficar parada ==");
        Console.WriteLine("  a solucao exata tem energia um, e ela fica em um para sempre");
        Console.WriteLine();
        Console.WriteLine($"{"metodo",-20}{"energia em t=200",18}{"maior vista",14}{"menor vista",14}");

        foreach (var orbita in Orbita.Todas())
        {
            var (final, maior, menor) = Orbita.Rodar(orbita, 200, 0.05);
            Console.WriteLine($"{orbita.Nome,-20}{final,18:E3}{maior,14:E3}{menor,14:E3}");
        }

        Console.WriteLine();
        Console.WriteLine("o Euler explicito GANHA energia: a orbita abre numa espiral que vai para o");
        Console.WriteLine("infinito, e em t = 200 a energia ja e vinte e duas mil vezes a certa. Ele");
        Console.WriteLine("simula um pendulo que acelera sozinho");
        Console.WriteLine();
        Console.WriteLine("o implicito PERDE energia: a orbita fecha numa espiral que morre no centro.");
        Console.WriteLine("Ele simula um pendulo que para sem atrito nenhum no problema");
        Console.WriteLine();
        Console.WriteLine("e o SIMPLETICO, que e o explicito com uma linha trocada de lugar, mantem a");
        Console.WriteLine("energia oscilando entre 0,976 e 1,026 para sempre. Ele e o unico dos tres");
        Console.WriteLine("que simula um pendulo de verdade");
        Console.WriteLine();

        Console.WriteLine($"  e o erro de POSICAO em tempo curto, que e a medida usual:");
        Console.WriteLine($"  {"metodo",-20}{"erro em t = 1",16}");

        foreach (var orbita in Orbita.Todas())
            Console.WriteLine($"  {orbita.Nome,-20}{Orbita.ErroDePosicao(orbita, 1, 0.05),16:E2}");

        Console.WriteLine();
        Console.WriteLine("  o simpletico e o PIOR dos tres nesta coluna. Os tres sao de ordem um, e");
        Console.WriteLine("  medir erro em tempo curto nao distingue de jeito nenhum um metodo que");
        Console.WriteLine("  conserva de um que explode");
        Console.WriteLine();
        Console.WriteLine("  escolher metodo por tabela de erro e exatamente como se chega no metodo");
        Console.WriteLine("  errado para simulacao longa");
        Console.WriteLine();
    }

    /// <summary>O que acontece quando o tempo cresce.</summary>
    private static void Longo()
    {
        Console.WriteLine("== a energia conforme o tempo passa ==");
        Console.Write($"{"ate t =",10}");
        foreach (var orbita in Orbita.Todas()) Console.Write($"{orbita.Nome,20}");
        Console.WriteLine();

        foreach (var ate in new[] { 10, 100, 1000, 10000 })
        {
            Console.Write($"{ate,10}");
            foreach (var orbita in Orbita.Todas())
                Console.Write($"{Orbita.Rodar(orbita, ate, 0.05).Final,20:E3}");
            Console.WriteLine();
        }

        Console.WriteLine();
        Console.WriteLine("as duas primeiras colunas vao embora em direcoes opostas, e cada vez mais");
        Console.WriteLine("rapido. A terceira fica parada em um, qualquer que seja o tempo");
        Console.WriteLine();
        Console.WriteLine("a diferenca entre o explicito e o simpletico e atualizar a velocidade ANTES");
        Console.WriteLine("de andar a posicao, e usar a velocidade NOVA para andar. Uma linha de lugar,");
        Console.WriteLine("e ela muda a natureza do metodo: ele passa a preservar area no espaco de");
        Console.WriteLine("fase, que e o que mantem a energia limitada");
        Console.WriteLine();
        Console.WriteLine("e e por isso que simulacao de orbita de planeta, de molecula e de particula");
        Console.WriteLine("usa metodo simpletico e nao Runge-Kutta, mesmo com o Runge-Kutta sendo tres");
        Console.WriteLine("ordens mais preciso em cada passo");
        Console.WriteLine();
    }

    /// <summary>O passo adaptativo contra o passo fixo, no mesmo erro.</summary>
    private static void Adaptativa()
    {
        Console.WriteLine("== passo adaptativo contra passo fixo, no mesmo erro final ==");
        Console.WriteLine("  o problema e um pico estreito no meio de muita planicie");
        Console.WriteLine();
        Console.WriteLine($"{"metodo",-20}{"tolerancia",12}{"pior erro",14}{"aceitos",10}{"rejeitados",12}{"avaliacoes",13}{"fixo gastaria",15}");

        var problema = Problema.Pico();

        foreach (var metodo in Metodos.Todos())
        {
            if (metodo.Implicito) continue;

            // A tolerancia depende da ORDEM, e nao por capricho: com ordem um
            // o erro por passo cai como h ao quadrado, entao pedir 1e-9 por
            // unidade de tempo pede um passo na casa de 1e-10, e a conta deixa
            // de terminar. A tabela mostra ate onde cada um chega.
            var tolerancias = metodo.OrdemTeorica == 1 ? new[] { 1e-4 } : new[] { 1e-6, 1e-9 };

            foreach (var tolerancia in tolerancias)
            {
                var r = Adaptativo.Integrar(metodo, problema, 1, tolerancia);
                var (fixo, _) = Adaptativo.CustoDoPassoFixo(metodo, problema, 1, r.PiorErro);

                Console.WriteLine($"{metodo.Nome,-20}{tolerancia,12:E0}{r.PiorErro,14:E2}{r.Aceitos,10}" +
                                  $"{r.Rejeitados,12}{r.Avaliacoes,13:N0}{fixo,15:N0}");
            }
        }

        Console.WriteLine();
        Console.WriteLine("a estimativa de erro sai de graca, por extrapolacao: um passo de tamanho h");
        Console.WriteLine("e depois dois de tamanho h/2, do mesmo ponto. As duas respostas diferem");
        Console.WriteLine("pelo erro, e para ordem p o erro do passo fino e a diferenca sobre 2^p - 1");
        Console.WriteLine();
        Console.WriteLine("isso custa tres passos no lugar de um, e paga: o passo fixo precisa ser");
        Console.WriteLine("pequeno o bastante para o pico e gasta esse tamanho na planicie inteira");
        Console.WriteLine();

        Console.WriteLine("  e o tamanho do passo ao longo do intervalo:");
        Console.WriteLine($"  {"metodo",-20}{"menor passo",14}{"maior passo",14}{"razao",10}");

        foreach (var metodo in Metodos.Todos())
        {
            if (metodo.Implicito) continue;
            var r = Adaptativo.Integrar(metodo, problema, 1, 1e-7);
            Console.WriteLine($"  {metodo.Nome,-20}{r.MenorPasso,14:E2}{r.MaiorPasso,14:E2}{r.MaiorPasso / r.MenorPasso,10:N0}");
        }

        Console.WriteLine();
        Console.WriteLine("  o maior passo e dezenas de vezes o menor, e e exatamente essa razao que");
        Console.WriteLine("  o passo fixo joga fora: ele usa o menor dos dois do comeco ao fim");
        Console.WriteLine();

        Console.WriteLine("  e a armadilha de medir erro so no VALOR FINAL:");
        Console.WriteLine($"  {"metodo",-20}{"passos",8}{"erro no fim",14}{"pior do caminho",17}");

        foreach (var metodo in Metodos.Todos())
        {
            if (metodo.Implicito) continue;

            foreach (var passos in new[] { 2, 8 })
                Console.WriteLine($"  {metodo.Nome,-20}{passos,8}{Ordem.Erro(metodo, problema, 1, passos),14:E2}" +
                                  $"{Ordem.ErroMaximo(metodo, problema, 1, passos),17:E2}");
        }

        Console.WriteLine();
        Console.WriteLine("  a solucao do pico VOLTA para o valor em que comecou, entao um metodo que");
        Console.WriteLine("  mal sai do lugar acerta o valor final sem ter visto o pico. Com dois");
        Console.WriteLine("  passos o erro no fim e minusculo e o erro de verdade e quase um");
        Console.WriteLine();
        Console.WriteLine("  por isso a comparacao com o passo fixo usa o PIOR erro do caminho: medir");
        Console.WriteLine("  so o fim premiaria o metodo que nao faz nada");
        Console.WriteLine();

        Console.WriteLine("== e a AGULHA, mil vezes mais estreita que o pico ==");
        Console.WriteLine("  ela tem largura perto de 0,001 dentro de um intervalo de tamanho um");
        Console.WriteLine();
        Console.WriteLine($"{"metodo",-20}{"tolerancia",12}{"pior erro",14}{"adaptativo",13}{"fixo gastaria",15}{"razao",9}");

        var agulha = Problema.Agulha();

        foreach (var metodo in Metodos.Todos())
        {
            if (metodo.Implicito || metodo.OrdemTeorica == 1) continue;

            foreach (var tolerancia in new[] { 1e-6, 1e-9 })
            {
                var r = Adaptativo.Integrar(metodo, agulha, 1, tolerancia, 0.01);
                var (fixo, _) = Adaptativo.CustoDoPassoFixo(metodo, agulha, 1, r.PiorErro);
                var marca = r.PiorErro > tolerancia ? " <- estourou" : "";

                Console.WriteLine($"{metodo.Nome,-20}{tolerancia,12:E0}{r.PiorErro,14:E2}" +
                                  $"{r.Avaliacoes,13:N0}{fixo,15:N0}{(double)fixo / r.Avaliacoes,9:N1}{marca}");
            }
        }

        Console.WriteLine();
        Console.WriteLine("a linha marcada e a mais importante da tabela. Com tolerancia de 1e-6 o");
        Console.WriteLine("Runge-Kutta PULA a agulha: o passo cresce cinco vezes por vez enquanto nada");
        Console.WriteLine("acontece, atravessa a agulha de uma vez e termina reportando sucesso com");
        Console.WriteLine("erro tres mil vezes maior que a tolerancia pedida");
        Console.WriteLine();
        Console.WriteLine("controle de passo e local por construcao, e nenhum controle local sabe o");
        Console.WriteLine("que existe entre dois pontos que ele nunca avaliou. Tolerancia respeitada");
        Console.WriteLine("nao e garantia de nada quando o metodo nao olhou");

        Console.WriteLine();
        Console.WriteLine("com o pico largo o passo adaptativo PERDE em todas as linhas, e isso me");
        Console.WriteLine("corrigiu. Estimar o erro por duplicacao custa TRES passos onde um bastaria,");
        Console.WriteLine("e a razao entre o maior e o menor passo, que ali fica entre 19 e 384, nao");
        Console.WriteLine("chega a pagar os tres");
        Console.WriteLine();
        Console.WriteLine("com a agulha, chega. A escala muda por mil de um lado do intervalo para o");
        Console.WriteLine("outro, o passo acompanha, e a mesma conta que perdia passa a ganhar");
        Console.WriteLine();
        Console.WriteLine("o passo adaptativo nao e melhor nem pior: ele e uma aposta em que o");
        Console.WriteLine("problema tem regioes de escalas muito diferentes. Quando tem, paga; quando");
        Console.WriteLine("nao tem, e so trabalho a mais");
        Console.WriteLine();
    }
}
