# ADR-0004 — MongoDB para a consolidação diária

## Status

Aceito

## Contexto

O contexto de Consolidação Diária é responsável por disponibilizar uma visão
consolidada dos lançamentos financeiros de determinado dia.

Diferentemente do contexto de Controle de Lançamentos, seu principal objetivo
é disponibilizar dados derivados para consulta.

A consolidação é composta por informações como:

- total de receitas;
- total de despesas;
- saldo;
- quantidade de receitas;
- quantidade de despesas;
- data da consolidação;
- data da última atualização.

Os lançamentos utilizados para essa visão são recebidos por meio de eventos
assíncronos e mantidos em uma projeção própria do contexto de Consolidação
Diária.

A solução também precisa permitir que o serviço de consolidação seja
dimensionado para atender picos de 50 requisições por segundo.

## Decisão

Adotar MongoDB como mecanismo de persistência do contexto
`MicroService.ConsolidadoDiario`.

O MongoDB será utilizado para armazenar duas projeções:

- lançamentos recebidos pelo contexto de consolidação;
- consolidações diárias.

O contexto não acessará diretamente o banco de dados do
`MicroService.ControleLancamentos`.

Os dados necessários para a consolidação serão mantidos localmente no
MongoDB por meio do processamento dos eventos publicados pelo contexto de
Controle de Lançamentos.

## Motivações

### Modelo orientado à consulta

A consolidação diária representa uma visão derivada dos lançamentos.

O modelo armazenado no MongoDB pode ser estruturado de acordo com as
necessidades de consulta do serviço, sem precisar reproduzir o modelo
transacional utilizado pelo Controle de Lançamentos.

### Projeção independente

O contexto de Consolidação Diária mantém sua própria representação dos
lançamentos.

Essa abordagem evita o compartilhamento direto do banco de dados do
Controle de Lançamentos e reduz o acoplamento entre os contextos.

### Escalabilidade

O requisito de negócio estabelece picos de 50 requisições por segundo para o
serviço de consolidação diária.

A utilização de uma persistência própria permite que o serviço seja
dimensionado de maneira independente do contexto transacional.

### Separação entre modelo transacional e modelo de leitura

O modelo utilizado para registrar e alterar lançamentos não precisa ser
igual ao modelo utilizado para realizar consultas de consolidação.

O MongoDB permite manter uma projeção específica para o cenário de leitura
do Consolidado Diário.

### Consistência eventual

Como os lançamentos são propagados por eventos assíncronos, a projeção do
Consolidado Diário pode ser atualizada após a confirmação da operação no
Controle de Lançamentos.

Essa característica é compatível com a decisão arquitetural de desacoplar a
disponibilidade dos dois serviços.

## Alternativas consideradas

### PostgreSQL compartilhado

Uma alternativa seria manter os lançamentos e os dados consolidados no
mesmo PostgreSQL.

Essa abordagem simplificaria a infraestrutura, porém criaria acoplamento
entre os contextos e permitiria que o Consolidado Diário dependesse
diretamente do modelo de dados do Controle de Lançamentos.

Foi descartada.

### PostgreSQL independente

Outra alternativa seria utilizar uma segunda instância ou banco PostgreSQL
exclusivo para o Consolidado Diário.

Essa abordagem atenderia à separação dos contextos, porém o modelo de
persistência relacional não oferece uma vantagem significativa para a
projeção de leitura adotada neste contexto.

Foi considerada, mas o MongoDB foi escolhido pela flexibilidade do modelo
documental e pela adequação ao armazenamento das projeções.

### Consulta direta ao Controle de Lançamentos

O Consolidado Diário poderia consultar diretamente o serviço ou banco do
Controle de Lançamentos sempre que uma consolidação fosse solicitada.

Essa abordagem criaria dependência síncrona e acoplamento entre os contextos,
além de fazer com que a disponibilidade do Controle de Lançamentos fosse
necessária para a consulta do consolidado.

Foi descartada.

## Consequências

### Positivas

- Persistência independente para o contexto de Consolidação Diária.
- Modelo de dados orientado às necessidades de consulta.
- Menor acoplamento entre os contextos.
- Possibilidade de dimensionamento independente.
- Projeção local dos lançamentos.
- Adequação ao modelo de consistência eventual adotado pela integração.

### Negativas

- Necessidade de manter uma projeção dos lançamentos.
- Os dados podem apresentar consistência eventual entre os contextos.
- É necessário processar corretamente eventos de criação, alteração e
  exclusão.
- A infraestrutura passa a possuir um segundo mecanismo de persistência.
- É necessário tratar reprocessamento e idempotência no consumidor dos
  eventos.

## Implementação

O contexto mantém documentos específicos para a projeção dos dados.

O documento de lançamento contém informações como:

- identificador;
- descrição;
- valor;
- data;
- tipo do lançamento;
- categoria;
- versão;
- indicador de exclusão.

O documento de consolidação diária contém:

- identificador;
- data;
- total de receitas;
- total de despesas;
- saldo;
- quantidade de receitas;
- quantidade de despesas;
- data de atualização.

Os lançamentos marcados como excluídos não participam do cálculo da
consolidação.

A atualização da projeção é realizada pelo
`Consumer.ConsolidadoDiario`, que processa os eventos publicados pelo
Controle de Lançamentos.

## Resultado esperado

O MongoDB deve fornecer uma persistência independente e adequada ao modelo
de projeção utilizado pelo Consolidado Diário, permitindo que o serviço
realize suas consultas sem depender diretamente do banco de dados do
Controle de Lançamentos.

A solução deve manter a separação entre o modelo transacional e o modelo
utilizado para consulta, permitindo evolução e dimensionamento independentes
dos dois contextos.