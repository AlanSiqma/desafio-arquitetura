# ADR-0007 — Idempotência nas operações de criação de lançamentos

## Status

Aceito

## Contexto

O `MicroService.ControleLancamentos` disponibiliza uma operação para criação de lançamentos.

Em uma comunicação distribuída, uma mesma requisição pode ser enviada mais de uma vez devido a retry do cliente, falha de rede ou repetição da operação.

Sem um mecanismo de idempotência, uma mesma operação poderia resultar na criação de múltiplos lançamentos, gerando duplicidade de dados e eventos.

A solução também utiliza mensageria assíncrona, o que reforça a necessidade de garantir que uma solicitação de criação possa ser identificada de forma única.

## Decisão

Adotar o padrão de idempotência utilizando o header `Idempotency-Key` na criação de lançamentos.

Cada requisição de criação deve fornecer uma chave de idempotência única.

O serviço armazenará a chave juntamente com um hash dos dados da requisição.

Quando uma nova requisição receber uma chave já utilizada:

- Se a chave estiver associada ao mesmo conteúdo da requisição, o lançamento existente será retornado.
- Se a chave estiver associada a um conteúdo diferente, a requisição será rejeitada com conflito.

A tabela `IdempotencyKeys` será persistida no PostgreSQL e possuirá uma restrição de unicidade para a chave.

## Motivações

### Evitar duplicidade

Uma mesma solicitação não deve criar múltiplos lançamentos caso seja processada mais de uma vez.

### Segurança contra retry

Clientes e intermediários podem repetir requisições devido a falhas de comunicação.

A chave permite identificar que as requisições representam a mesma operação.

### Consistência

A chave de idempotência e o lançamento são persistidos dentro do mesmo fluxo transacional.

Isso reduz o risco de registrar a chave sem registrar o lançamento correspondente ou vice-versa.

### Detecção de reutilização incorreta

A utilização de um hash da requisição permite diferenciar:

- repetição legítima da mesma operação;
- reutilização da mesma chave para uma operação diferente.

## Alternativas consideradas

### Não utilizar idempotência

Descartada porque uma repetição da requisição poderia criar lançamentos duplicados.

### Utilizar somente o identificador do lançamento

Descartada porque o identificador é gerado pelo serviço e não representa necessariamente a mesma operação solicitada pelo cliente.

### Utilizar somente um identificador enviado pelo cliente

Descartada porque não seria possível detectar se uma mesma chave foi reutilizada com dados diferentes.

### Utilizar somente hash da requisição

Descartada porque requisições diferentes poderiam possuir o mesmo conteúdo e, portanto, o mesmo hash, apesar de representarem operações distintas.

## Consequências

### Positivas

- Evita criação duplicada para uma mesma operação.
- Permite retry seguro da requisição.
- Detecta reutilização da chave com dados diferentes.
- Mantém o controle de idempotência no mesmo banco da operação de negócio.
- A restrição de unicidade no banco ajuda a tratar requisições concorrentes.

### Negativas

- Adiciona uma estrutura de persistência para controle das chaves.
- As chaves precisam possuir uma política de retenção adequada.
- O cliente precisa enviar uma chave de idempotência para as operações que utilizam esse mecanismo.
- É necessário calcular e armazenar o hash dos dados relevantes da requisição.

## Implementação

A entidade `IdempotencyKey` possui os seguintes dados principais:

- `Id`
- `Key`
- `RequestHash`
- `LancamentoId`
- `CreatedAt`

A coluna `Key` possui índice único no PostgreSQL.

O hash é calculado a partir dos dados relevantes do lançamento:

- `Descricao`
- `Valor`
- `Data`
- `TipoLancamento`
- `Categoria`

O fluxo de criação verifica inicialmente se a chave já existe.

Caso exista e o hash seja igual ao da nova requisição, o lançamento anteriormente criado é retornado.

Caso exista e o hash seja diferente, a operação retorna conflito.

Caso não exista, o serviço cria o lançamento e registra a chave de idempotência dentro da mesma transação.

A restrição de unicidade do banco também é utilizada para tratar situações de concorrência em que duas requisições utilizam simultaneamente a mesma chave.

## Resultado esperado

Uma mesma operação de criação pode ser repetida sem gerar lançamentos duplicados.

Uma mesma `Idempotency-Key` associada a dados diferentes deve ser rejeitada.

O mecanismo deve continuar funcionando mesmo quando houver retry ou processamento concorrente da mesma solicitação.