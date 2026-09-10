# ADR-0005 — RabbitMQ para comunicação assíncrona entre os serviços

## Status

Aceito

## Contexto

A solução possui dois serviços com responsabilidades independentes:

- `MicroService.ControleLancamentos`: responsável pelo controle dos lançamentos.
- `MicroService.ConsolidadoDiario`: responsável pela consolidação diária dos lançamentos.

O serviço de controle deve permanecer disponível mesmo quando o serviço de consolidação estiver indisponível.

As alterações realizadas nos lançamentos precisam ser propagadas para o serviço de consolidação.

Uma comunicação síncrona entre os serviços criaria uma dependência direta de disponibilidade. Caso o `ConsolidadoDiario` estivesse indisponível, uma operação realizada no `ControleLancamentos` poderia ser afetada.

## Decisão

Adotar o RabbitMQ como broker de mensagens para realizar a comunicação assíncrona entre os serviços.

O `MicroService.ControleLancamentos` publica eventos quando um lançamento é criado, alterado ou excluído:

- `LancamentoCriadoEvent`
- `LancamentoAlteradoEvent`
- `LancamentoExcluidoEvent`

O `MicroService.ConsolidadoDiario` consome esses eventos e atualiza sua projeção no MongoDB.

A comunicação será assíncrona, eliminando a dependência de uma chamada direta entre os serviços.

O consumidor utilizará políticas de retry para tratar falhas transitórias durante o processamento das mensagens.

## Motivações

### Desacoplamento

O `ControleLancamentos` não precisa realizar chamadas diretas ao `ConsolidadoDiario`.

Os serviços possuem contratos de integração baseados em eventos, reduzindo o acoplamento entre suas implementações.

### Independência de disponibilidade

A indisponibilidade do `ConsolidadoDiario` não impede o `ControleLancamentos` de realizar suas operações.

As mensagens podem permanecer disponíveis no broker para serem processadas posteriormente.

### Comunicação assíncrona

A atualização da projeção no MongoDB não precisa acontecer dentro da mesma requisição responsável pela criação, alteração ou exclusão do lançamento.

### Tolerância a falhas

A utilização de retry permite realizar novas tentativas quando ocorre uma falha transitória no processamento de uma mensagem.

### Escalabilidade

O consumidor pode ser escalado de forma independente do serviço responsável pelos lançamentos.

## Alternativas consideradas

### Comunicação HTTP síncrona

Descartada porque criaria uma dependência direta entre os serviços.

Uma indisponibilidade do `ConsolidadoDiario` poderia afetar as operações do `ControleLancamentos`, contrariando o requisito de disponibilidade.

### Comunicação direta entre bancos de dados

Descartada porque faria com que um serviço dependesse da estrutura de persistência do outro.

Cada serviço deve possuir responsabilidade sobre seu próprio modelo de dados.

### Apache Kafka

Considerado como alternativa para comunicação orientada a eventos.

Foi descartado neste cenário por apresentar uma complexidade operacional maior do que a necessária para o volume e as características da solução proposta.

### Jobs ou processamento por arquivos

Descartados por aumentarem a latência da propagação das alterações e não representarem uma integração orientada a eventos adequada ao cenário.

## Consequências

### Positivas

- O `ControleLancamentos` permanece independente da disponibilidade do `ConsolidadoDiario`.
- A comunicação entre os serviços é assíncrona.
- As mensagens podem ser processadas posteriormente em caso de indisponibilidade do consumidor.
- O consumidor pode utilizar retry para falhas transitórias.
- Os serviços podem ser escalados independentemente.
- O banco de dados de um serviço não precisa ser acessado pelo outro.

### Negativas

- A consistência entre os serviços passa a ser eventual.
- O sistema precisa considerar o reprocessamento de mensagens.
- O consumidor precisa controlar versões dos eventos para evitar que mensagens antigas sobrescrevam informações mais recentes.
- O RabbitMQ adiciona um componente de infraestrutura à solução.
- É necessário monitorar o processamento das mensagens e a saúde do broker.

## Implementação

A integração utiliza MassTransit sobre RabbitMQ.

O `ControleLancamentos` publica os eventos utilizando `IPublishEndpoint`.

O `ConsolidadoDiario` possui endpoints de recebimento específicos:

- `lancamento-criado`
- `lancamento-alterado`
- `lancamento-excluido`

Cada endpoint possui uma política de retry para falhas transitórias.

Os eventos de criação e alteração carregam a versão do lançamento. O evento de exclusão também possui a versão correspondente.

O consumidor utiliza a versão recebida para evitar que eventos antigos sobrescrevam uma versão mais recente da projeção armazenada no MongoDB.

A publicação dos eventos utiliza Transactional Outbox, permitindo que a persistência do lançamento e o registro da mensagem para publicação sejam tratados de forma transacional no serviço de controle.

## Resultado esperado

O `ControleLancamentos` deve continuar disponível e operando mesmo quando o `ConsolidadoDiario` estiver indisponível.

Quando o consumidor estiver novamente disponível, as mensagens pendentes devem ser processadas e a projeção do MongoDB deve convergir para o estado correspondente aos lançamentos persistidos no serviço de controle.