# ADR-0001 — Adoção de arquitetura baseada em microserviços

## Status

Aceito

## Contexto

A solução deve atender ao fluxo de negócio de um comerciante que precisa
controlar seu fluxo de caixa diário por meio de lançamentos financeiros e
disponibilizar o saldo diário consolidado.

O desafio define duas capacidades principais:

- controle de lançamentos;
- consolidação diária.

Além da separação das capacidades, existem requisitos não funcionais que
influenciam diretamente a arquitetura:

- o serviço de controle de lançamentos não deve ficar indisponível caso o
  serviço de consolidação diária esteja indisponível;
- o serviço de consolidação diária deve suportar picos de 50 requisições por
  segundo;
- a perda máxima de requisições durante esses picos deve ser de 5%.

Dessa forma, a arquitetura precisa permitir a separação das responsabilidades
e, principalmente, evitar que uma indisponibilidade no serviço de consolidação
cause indisponibilidade no serviço responsável pelo controle dos lançamentos.

## Decisão

Adotar uma arquitetura baseada em microserviços, separando as duas principais
capacidades de negócio em serviços independentes:

- `MicroService.ControleLancamentos`
- `MicroService.ConsolidadoDiario`

Cada serviço será responsável por seu respectivo contexto de negócio e poderá
evoluir e ser dimensionado de forma independente.

A comunicação entre os serviços será realizada de forma assíncrona, evitando
uma dependência síncrona de disponibilidade entre o controle de lançamentos e
a consolidação diária.

## Motivações

A principal motivação para a adoção de microserviços é o requisito de
independência de disponibilidade entre as capacidades.

O serviço de controle de lançamentos precisa continuar operando mesmo quando
o serviço de consolidação diária estiver indisponível. A separação em
serviços independentes permite que uma falha em um contexto não interrompa
diretamente a operação do outro.

A separação também permite que as capacidades sejam dimensionadas de acordo
com suas características próprias de utilização. O requisito de pico de
50 requisições por segundo está associado ao serviço de consolidação diária,
portanto sua capacidade pode ser tratada independentemente da capacidade
necessária para o controle de lançamentos.

Outro fator é a separação de responsabilidades. Controle de lançamentos e
consolidação diária possuem objetivos e comportamentos distintos e, portanto,
podem ser tratados como capacidades independentes dentro da solução.

## Alternativas consideradas

### Monólito

Um monólito poderia concentrar as funcionalidades de controle de lançamentos
e consolidação diária em uma única aplicação.

Essa alternativa teria como vantagem uma menor complexidade operacional,
porém criaria um maior acoplamento entre as capacidades.

Uma indisponibilidade da aplicação poderia afetar simultaneamente o controle
de lançamentos e a consolidação diária, dificultando o atendimento do
requisito de disponibilidade independente.

Por esse motivo, a alternativa foi descartada.

### Monólito modular

Um monólito modular permitiria separar internamente as responsabilidades de
controle de lançamentos e consolidação diária, mantendo uma única unidade de
execução.

Embora forneça uma melhor separação lógica que um monólito tradicional, as
capacidades ainda compartilhariam a mesma unidade de implantação e os mesmos
recursos computacionais.

Considerando a necessidade de independência de disponibilidade e a
possibilidade de escalabilidade independente, essa alternativa não foi
adotada.

### SOA

Uma arquitetura orientada a serviços poderia atender à necessidade de
separação entre as capacidades.

Entretanto, para o escopo apresentado, as capacidades estão suficientemente
delimitadas para serem tratadas como serviços independentes, tornando a
abordagem de microserviços mais adequada à granularidade escolhida.

### Serverless

Uma arquitetura serverless poderia ser utilizada para determinados
componentes da solução, especialmente em cenários orientados a eventos.

Porém, o requisito apresentado não exige esse modelo de execução e sua
adoção como arquitetura principal acrescentaria complexidade sem uma
necessidade explícita do cenário.

## Consequências

### Positivas

- Separação das capacidades de negócio.
- Independência de disponibilidade entre os serviços.
- Possibilidade de escalabilidade independente.
- Evolução independente dos serviços.
- Isolamento de falhas entre os contextos.
- Maior flexibilidade para definir tecnologias e estratégias específicas para
  cada capacidade.

### Negativas

- Maior complexidade operacional em relação a um monólito.
- Necessidade de mecanismos de comunicação entre serviços.
- Necessidade de observabilidade distribuída.
- Necessidade de tratamento de falhas de comunicação.
- Consistência entre os serviços passa a exigir mecanismos específicos de
  integração.
- Maior complexidade para desenvolvimento, testes e troubleshooting.

## Relação com os requisitos do desafio

A decisão atende diretamente ao requisito de existência de um serviço para
controle de lançamentos e outro para consolidação diária.

Também atende ao requisito não funcional que determina que o serviço de
controle de lançamentos não deve ficar indisponível caso o serviço de
consolidação diária esteja indisponível.

A separação permite ainda tratar de forma independente o requisito de
capacidade do serviço de consolidação diária, que deve suportar picos de
50 requisições por segundo com perda máxima de 5%.

## Resultado esperado

A arquitetura deverá permitir que cada capacidade de negócio opere de forma
independente, reduzindo o acoplamento entre os serviços e permitindo que
disponibilidade, escalabilidade e evolução sejam tratadas de acordo com as
características específicas de cada capacidade.