# edo

Métodos para equações diferenciais do zero em C# e .NET 8: Euler explícito e
implícito, ponto médio, Heun, Runge-Kutta 4 e o Euler **simplético**. Todo
problema vem com a solução **exata** junto, e é ela que julga.

```
$ dotnet medidor.dll energia

metodo                energia em t=200   maior vista   menor vista
Euler explicito             2.175E+004    2.175E+004    1.000E+000
Euler implicito             4.597E-005    1.000E+000    4.597E-005
Euler simpletico            1.023E+000    1.026E+000    9.756E-001

  e o erro de POSICAO em tempo curto, que e a medida usual:
  Euler explicito            1.44E-002
  Euler implicito            1.26E-002
  Euler simpletico           2.11E-002
```

A energia do oscilador harmônico vale **um** e fica em um para sempre. O Euler
explícito **ganha** energia e simula um pêndulo que acelera sozinho; o implícito
**perde** e simula um que para sem atrito nenhum no problema; o simplético
mantém a energia entre 0,976 e 1,026, para sempre.

E na segunda tabela, que é a medida com que método costuma ser escolhido, o
simplético é o **pior dos três**.

## A diferença entre explodir e conservar é uma linha de lugar

```csharp
// Euler explicito: anda a posicao com a velocidade VELHA
(x + h * v, v - h * x)

// Euler simpletico: atualiza a velocidade primeiro e anda com a NOVA
var novaV = v - h * x;
(x + h * novaV, novaV)
```

Os dois são de ordem um, avaliam a função o mesmo número de vezes e custam o
mesmo. Trocar a ordem das duas linhas faz o método preservar área no espaço de
fase, e é isso que mantém a energia limitada.

```
$ dotnet medidor.dll longo

   ate t =     Euler explicito     Euler implicito    Euler simpletico
        10          1.648E+000          6.069E-001          9.775E-001
       100          1.475E+002          6.780E-003          1.022E+000
      1000          4.871E+021          2.053E-022          9.801E-001
     10000          7.521E+216          1.330E-217          9.906E-001
```

As duas primeiras colunas vão embora em direções opostas, e cada vez mais
rápido. A terceira fica no lugar.

É por isso que simulação de órbita de planeta, de molécula e de partícula usa
método simplético e não Runge-Kutta, mesmo com o Runge-Kutta sendo três ordens
mais preciso em cada passo.

## A ordem, medida em vez de citada

```
$ dotnet medidor.dll ordem

metodo                teorica        y' = y     y' = -2ty    y' = t - y
Euler explicito             1          1.00          1.00          1.00
Euler implicito             1          1.00          1.00          1.00
ponto medio                 2          2.00          2.01          2.00
Heun                        2          2.00          2.00          2.00
Runge-Kutta 4               4          3.99          4.00          4.00
```

Um método de ordem p tem o erro dividido por 2^p quando o passo cai pela metade,
então o logaritmo da razão na base dois **é** a ordem. Esse número não vem de
teoria nenhuma: sai da conta, e bate com a teoria até a segunda casa.

A medida só vale numa faixa de passo. Grande demais e o termo dominante do erro
deixa de mandar; pequeno demais e o arredondamento manda. Fora dela o número
deixa de significar o que diz.

## Quatro avaliações por passo é barato

```
$ dotnet medidor.dll custo

metodo                 erro alvo      passos    avaliacoes
Euler explicito           1E-004       2,048         2,048
Runge-Kutta 4             1E-004           4            16

Euler explicito           1E-008  16,777,216    16,777,216
Runge-Kutta 4             1E-008          64           256
```

Para errar menos que um centésimo de milionésimo, o Runge-Kutta gasta **256**
avaliações: menos do que o Euler gasta para errar um decimilésimo, que é dez mil
vezes mais erro.

E a distância cresce com a exigência. Cada dígito a mais custa dez vezes mais
passos no Euler e a raiz quarta de dez no Runge-Kutta.

## Rigidez: quando o passo é ditado pela estabilidade

```
$ dotnet medidor.dll rigidez

metodo                 maior passo estavel  passos ate t = 1
Euler explicito                 2.000E-003               500
Euler implicito                 sem limite                 1
ponto medio                     2.000E-003               500
Heun                            2.000E-003               500
Runge-Kutta 4                   2.785E-003               360
```

Em `y' = -1000y` a solução já é uma linha reta em zero antes de `t = 0,01`, e o
Euler explícito precisa de **quinhentos** passos para atravessar o intervalo. Não
é precisão: é que com passo maior ele explode.

O limite sai numa linha. O explícito multiplica por `1 - h*lambda` a cada passo,
e o módulo precisa ser menor que um: `h < 2/lambda`. Em `h = 2/lambda` exatamente
o fator mede **-1,000000**, e a solução fica pulando entre mais e menos um para
sempre.

O implícito multiplica por `1/(1 + h*lambda)`, que tem módulo menor que um para
**qualquer** passo. Ele é de ordem um, custa Newton a cada passo e não compra
precisão nenhuma: compra isso.

Ordem alta não compra estabilidade. O Runge-Kutta 4 tem a maior região dos
explícitos e ela é `2,785/lambda`: um terço a mais de folga, não quatro vezes.

## O passo adaptativo perde na metade das vezes

```
$ dotnet medidor.dll adaptativo

metodo                tolerancia     pior erro   avaliacoes  fixo gastaria
ponto medio               1E-006     5.30E-005       21,252          8,192
Runge-Kutta 4             1E-006     5.17E-005          972            512
Runge-Kutta 4             1E-009     4.69E-008        4,824          2,048
```

Medir o próprio erro a cada passo e mudar o tamanho do passo por causa dele é a
ideia certa, e no problema do pico ela **perde** em todas as linhas. A estimativa
sai por extrapolação de Richardson, dando um passo de tamanho `h` e depois dois de
`h/2` do mesmo ponto, e isso custa **três passos onde um bastaria**. A razão entre
o maior e o menor passo, que ali fica entre 19 e 384, não chega a pagar os três.

Com uma agulha mil vezes mais estreita, a mesma conta e o mesmo código ganham:

```
metodo                tolerancia     pior erro   avaliacoes  fixo gastaria    razao
ponto medio               1E-006     3.79E-007        3,732         32,768      8.8
Heun                      1E-009     7.22E-010      158,514      1,048,576      6.6
Runge-Kutta 4             1E-006     3.06E-003           60            512      8.5 <- estourou
Runge-Kutta 4             1E-009     4.12E-010        2,628         32,768     12.5
```

O passo adaptativo não é melhor nem pior: ele é uma aposta em que o problema tem
regiões de escalas muito diferentes. Quando tem, paga; quando não tem, é só
trabalho a mais.

### E a linha marcada é a mais importante da tabela

Com tolerância de `1e-6`, o Runge-Kutta **pula a agulha inteira**. O passo cresce
cinco vezes por vez enquanto nada acontece, atravessa a agulha de uma só vez, e
termina reportando sucesso com erro **três mil vezes** maior que a tolerância
pedida.

Controle de passo é local por construção, e nenhum controle local sabe o que
existe entre dois pontos que ele nunca avaliou. Tolerância respeitada não é
garantia de nada quando o método não olhou.

## O que as medidas me corrigiram

**Eu esperava que o passo adaptativo ganhasse, e ele perde.** A estimativa de erro
por duplicação custa três passos onde um bastaria, e o pico, apesar de estreito,
não tem escala variada o bastante para repagar os três. O caminho que a biblioteca
de verdade segue, que é usar um par encaixado tipo Runge-Kutta-Fehlberg, existe
exatamente por isso: ali a estimativa custa quase nada, e a conta inverte.

**Medir o erro só no valor final premia o método que não faz nada.** A solução do
pico volta para o valor em que começou, então um método que mal sai do lugar
acerta o fim. Com dois passos, o Heun erra `1,05E-001` no valor final e
`9,25E-001` no caminho. Por isso a comparação com o passo fixo usa o pior erro do
caminho inteiro.

**E o caso extremo disso é engraçado: o ponto médio com UM passo acerta o pico com
erro zero.** Ele avalia a função exatamente no centro do pico, onde ela vale zero,
não sai do lugar e termina no valor certo. Acertou por não andar. A busca do custo
fixo começa em 128 passos por causa dele.

**A primeira agulha que escrevi media o problema, não o método.** Ela era o pico
apertado mil vezes, com `y'` proporcional a `y` ao quadrado, e nela uma
perturbação minúscula perto do topo volta multiplicada por `1e10`. Todos os
métodos erravam `0,8` e o número não dizia nada sobre nenhum deles. A agulha boa é
uma integral pura: o lado direito não depende de `y`, então o erro só soma.

**O limite do Euler explícito deu 0,278 em vez de 0,2, e a culpa era do meu
critério.** Eu media "explodir" por limiar: o valor não pode passar de dez. Com
`lambda = 1000` isso dá o número certo, porque o intervalo tem quinhentos passos
e qualquer crescimento chega a dez bem antes do fim. Com `lambda = 10` o
intervalo tem **quatro** passos, e um fator de 1,78 por passo termina em 9,99:
passava no teste. O limiar media o tamanho do intervalo junto com a estabilidade.
Medir **crescimento por passo** não mede: a solução decai, então qualquer passo
que aumente o módulo já é o método indo para o lado errado, aconteça no primeiro
passo ou no quingentésimo. Com o critério certo o limite dá 2,000E-003 cravado
para os três explícitos de ordem um.

**O ponto médio e o Heun têm exatamente o mesmo limite do Euler explícito.** Eu
esperava que ordem dois comprasse alguma folga, e a região na reta real é a mesma
`[-2, 0]`: o fator de amplificação dos dois em `h = 2/lambda` mede **1,000000**,
igual em módulo ao -1 do Euler. Duas avaliações por passo compram ordem, e
estabilidade nenhuma.

**O Euler implícito no `h = 2/lambda` dá o mesmo 0,333333 do Runge-Kutta 4.** Os
dois números são o mesmo por coincidência de `z = -2`, e a coincidência acaba em
qualquer outro passo: com `h` cem vezes maior o implícito continua abaixo de um e
o Runge-Kutta já foi.

**Medir erro em tempo curto não distingue conservar de explodir.** Essa é a que
mais me corrigiu. Os três métodos do oscilador são de ordem um e erram a posição
na mesma casa decimal em `t = 1`; o simplético erra **mais** que os outros dois.
Se a escolha for por tabela de erro, ela cai no explícito, que em `t = 10000`
reporta uma energia de `7,5E+216` num sistema onde ela deveria valer um.

**A solução exata precisa ser conferida também.** Um dos testes deriva a própria
fórmula fechada por diferença central e compara com `f(t, y)`. Se a fórmula
estivesse errada, todo o resto do repositório estaria medindo erro contra a coisa
errada com muita precisão.

## Por que a solução exata é o juiz

Método de equação diferencial erra de um jeito traiçoeiro. A saída é uma curva
plausível mesmo quando está errada, e uma curva plausível que diverge devagar não
parece errada em nada: ela parece a solução de outro problema parecido.

Com a solução exata junto, "qual é a ordem deste método?" para de ser citação e
vira medida. E as três coisas que este repositório mostra, a ordem, o limite de
estabilidade e a energia, são todas número contra número.

Nenhuma medida aqui usa relógio. Todas são erro contra a solução exata ou
contagem de avaliações da função, então a saída é a mesma no Linux, no Windows e
no macOS, e a integração contínua roda nos três.

## As peças

| arquivo | o que faz |
| --- | --- |
| `Problema.cs` | os problemas de valor inicial, cada um com a solução exata junto |
| `Oscilador.cs` | o sistema de duas equações e a energia que deveria ficar parada |
| `Metodo.cs` | o que um método de passo é: ordem, custo e a função do passo |
| `Metodos.cs` | os cinco métodos de passo |
| `Ordem.cs` | integrar, o erro contra a exata e a ordem medida |
| `Estabilidade.cs` | a região de estabilidade e o fator de amplificação |
| `Orbita.cs` | o oscilador nos três Euler, e a energia que eles tratam de três jeitos |
| `Adaptativo.cs` | a estimativa de erro por extrapolação e o controle de passo |

## Como rodar

```
dotnet test testes/Edo.Testes/Edo.Testes.csproj -c Release
dotnet run --project ferramentas/Medidor/Medidor.csproj -c Release -- tudo
```

As medidas aceitam `ordem`, `custo`, `rigidez`, `energia`, `longo`, `adaptativo` e
`tudo`.

## Licença

MIT.
