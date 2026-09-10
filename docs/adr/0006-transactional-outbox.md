# ADR-0006 — Transactional Outbox para publicação de eventos

## Status

Aceito

## Contexto

O `MicroService.ControleLancamentos` utiliza PostgreSQL como banco de dados principal e RabbitMQ para comunicação assíncrona com o `MicroService.ConsolidadoDiario`.

Quando um lançamento é criado, alterado ou excluído, duas operações precisam ocorrer:

1. Persistir a alteração no PostgreSQL.
2. Publicar o evento correspondente no RabbitMQ.

A realização dessas operações de forma independente poderia gerar inconsistência.

Por exemplo, o lançamento poderia ser persistido com sucesso no PostgreSQL, mas a publicação do evento poderia falhar. Nesse cenário, o `ConsolidadoDiario` não receberia a alteração e sua projeção ficaria inconsistente.

## Decisão

Adotar o padrão **Transactional Outbox** no `MicroService.ControleLancamentos`.

Os eventos de integração serão registrados em uma Outbox dentro da mesma transação do PostgreSQL que persiste a alteração do lançamento.

Os eventos utilizados são:

- `LancamentoCriadoEvent`
- `LancamentoAlteradoEvent`
- `LancamentoExcluidoEvent`

O MassTransit será utilizado para integrar o padrão Outbox ao fluxo de publicação das mensagens.

O envio para o RabbitMQ ocorrerá posteriormente, a partir das mensagens armazenadas na Outbox.

## Motivações

### Consistência entre banco e mensageria

A principal motivação é garantir que a alteração do lançamento e o registro do evento sejam confirmados juntos.

Se a transação for revertida, tanto a alteração quanto o evento serão revertidos.

### Tolerância a falhas

Uma indisponibilidade temporária do RabbitMQ não deve fazer com que um lançamento já confirmado seja perdido.

O evento permanece registrado na Outbox até que possa ser publicado.

### Desacoplamento da persistência

A operação de negócio não precisa depender da disponibilidade imediata do RabbitMQ para concluir a persistência do lançamento.

### Recuperação automática

Após a recuperação do broker, as mensagens armazenadas na Outbox podem continuar o fluxo de publicação.

### Confiabilidade da integração

O padrão reduz o risco de ocorrer uma alteração persistida no banco sem o respectivo evento de integração.

## Alternativas consideradas

### Publicação direta no RabbitMQ após o SaveChanges

Descartada porque ainda existe uma janela de falha entre a persistência do lançamento e a publicação da mensagem.

O banco poderia confirmar a operação enquanto a publicação do evento falharia.

### Publicação antes da persistência

Descartada porque o evento poderia ser publicado e posteriormente a transação do banco poderia falhar.

Nesse caso, o consumidor receberia um evento referente a uma alteração que não foi efetivamente persistida.

### Dupla transação independente

Descartada porque não existe uma transação distribuída simples entre PostgreSQL e RabbitMQ neste cenário.

A solução adotada evita a necessidade de coordenar diretamente uma transação distribuída entre os dois componentes.

## Consequências

### Positivas

- Reduz o risco de perda de eventos.
- Mantém o registro do evento associado à transação de negócio.
- Permite que o RabbitMQ fique temporariamente indisponível sem impedir a persistência do lançamento.
- Permite recuperação posterior das mensagens.
- Mantém o serviço de controle desacoplado da disponibilidade imediata do consumidor.

### Negativas

- Adiciona armazenamento e processamento da Outbox ao serviço.
- O evento não é necessariamente publicado imediatamente após a alteração.
- É necessário monitorar o processamento das mensagens da Outbox.
- O consumidor precisa ser preparado para possíveis reprocessamentos.
- A solução continua utilizando consistência eventual entre os serviços.

## Implementação

O `AppDbContext` utiliza as entidades de Outbox fornecidas pelo MassTransit:

- `InboxState`
- `OutboxMessage`
- `OutboxState`

A configuração do MassTransit utiliza o Entity Framework Outbox com PostgreSQL e Bus Outbox.

As operações de criação, alteração e exclusão dos lançamentos publicam os respectivos eventos utilizando `IPublishEndpoint`.

O registro do evento é realizado dentro do fluxo transacional do PostgreSQL.

Após a confirmação da transação, o MassTransit realiza o processamento da Outbox e publica as mensagens no RabbitMQ.

O fluxo de integração é, portanto:

```text
Operação de negócio
        ↓
PostgreSQL + Outbox
        ↓
Commit da transação
        ↓
Processamento da Outbox
        ↓
RabbitMQ
        ↓
ConsolidadoDiario
        ↓
MongoDB
```

## Resultado esperado

Toda alteração de lançamento confirmada no PostgreSQL deve possuir seu respectivo evento registrado na Outbox.

Uma indisponibilidade temporária do RabbitMQ não deve causar perda do evento.

Após a recuperação da infraestrutura de mensageria, os eventos pendentes devem ser publicados e processados pelo ConsolidadoDiario, permitindo que sua projeção no MongoDB converja para o estado do serviço de controle.