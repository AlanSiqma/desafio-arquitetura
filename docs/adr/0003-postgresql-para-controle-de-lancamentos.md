# ADR-0003 — PostgreSQL para o controle de lançamentos

## Status

Aceito

## Contexto

O contexto de Controle de Lançamentos é responsável pelo ciclo de vida dos
lançamentos financeiros, incluindo criação, consulta, alteração e exclusão.

Essas operações representam dados transacionais do sistema e precisam manter
consistência entre as informações do lançamento e os mecanismos utilizados
para garantir idempotência e publicação dos eventos.

A solução também utiliza o padrão Transactional Outbox. Portanto, a
persistência escolhida precisa permitir que os dados do lançamento e as
mensagens da Outbox participem da mesma transação.

## Decisão

Adotar PostgreSQL como banco de dados do contexto
`MicroService.ControleLancamentos`.

O PostgreSQL será utilizado para armazenar:

- lançamentos financeiros;
- chaves de idempotência;
- mensagens relacionadas ao Transactional Outbox.

O acesso aos dados será realizado utilizando Entity Framework Core.

A integridade transacional será mantida pelo banco de dados, permitindo que
a persistência do lançamento, da chave de idempotência e da Outbox seja
coordenada dentro da mesma transação.

## Motivações

### Consistência transacional

O Controle de Lançamentos possui operações que precisam ser tratadas de
forma transacional.

Ao criar um lançamento, por exemplo, é necessário garantir a consistência
entre:

- o lançamento persistido;
- o registro de idempotência;
- o evento destinado à integração com o contexto de Consolidação Diária.

O PostgreSQL fornece suporte a transações relacionais adequado para esse
cenário.

### Integridade dos dados

Os lançamentos possuem relacionamentos entre atributos que precisam ser
mantidos de maneira consistente.

O modelo relacional permite utilizar:

- restrições;
- índices;
- chaves;
- tipos de dados;
- transações.

Esses recursos são adequados para o caráter transacional do contexto.

### Idempotência

O Controle de Lançamentos utiliza uma chave de idempotência para evitar o
processamento duplicado de uma mesma requisição.

A chave é persistida no PostgreSQL e possui restrição de unicidade.

Essa restrição permite que o próprio banco participe da garantia de
unicidade mesmo em situações de concorrência.

### Transactional Outbox

A solução utiliza o mecanismo de Outbox integrado ao Entity Framework Core.

A utilização do PostgreSQL permite que a alteração do estado transacional
e a persistência da mensagem da Outbox ocorram dentro da mesma transação do
banco.

Isso reduz o risco de persistir um lançamento sem registrar o evento
necessário para sua propagação aos demais componentes.

## Alternativas consideradas

### MongoDB

O MongoDB foi considerado como alternativa para o armazenamento dos
lançamentos.

Entretanto, o contexto de Controle de Lançamentos possui características
transacionais e utiliza restrições de unicidade e Transactional Outbox.

Para esse contexto, o modelo relacional oferece uma abordagem mais adequada
para garantir integridade e consistência das operações.

O MongoDB foi utilizado em outro contexto da solução, onde o modelo de
projeção para consultas consolidadas é mais adequado.

### Banco relacional compartilhado com o Consolidado Diário

Outra possibilidade seria utilizar o mesmo banco de dados para os dois
contextos.

Essa abordagem reduziria a quantidade de componentes de persistência, porém
criaria acoplamento entre os contextos e permitiria que o modelo de dados de
um serviço se tornasse uma dependência do outro.

Foi descartada.

## Consequências

### Positivas

- Suporte adequado a operações transacionais.
- Garantia de unicidade para chaves de idempotência.
- Suporte a índices e restrições relacionais.
- Integração adequada com Entity Framework Core.
- Possibilidade de utilizar transações para o padrão Transactional Outbox.
- Separação da persistência do contexto de Controle de Lançamentos.

### Negativas

- Necessidade de operação e manutenção de um banco relacional.
- O modelo relacional exige evolução controlada do schema.
- A solução passa a depender de infraestrutura adicional para o PostgreSQL.

## Implementação

O contexto utiliza:

- PostgreSQL;
- Entity Framework Core;
- migrations para evolução do schema;
- Transactional Outbox;
- índice único para a chave de idempotência.

A estrutura de persistência está localizada no projeto:

`Data.MongoDb`

e o contexto específico do Controle de Lançamentos utiliza o
`AppDbContext` baseado em Entity Framework Core.

## Resultado esperado

O PostgreSQL deve fornecer a persistência transacional necessária para o
Controle de Lançamentos, garantindo a consistência dos dados e permitindo
a implementação do mecanismo de idempotência e Transactional Outbox sem
introduzir dependências de persistência no contexto de Consolidação Diária.