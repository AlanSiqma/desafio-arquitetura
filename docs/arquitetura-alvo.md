# Arquitetura Alvo

## 1. Objetivo

Este documento apresenta a arquitetura alvo proposta para a solução de controle de fluxo de caixa diário.

A arquitetura foi definida a partir dos requisitos funcionais, requisitos não funcionais e capacidades de negócio identificados no desafio.

O objetivo principal é separar as responsabilidades de Controle de Lançamentos e Consolidação Diária, permitindo que cada capacidade possua autonomia de evolução, persistência e escalabilidade.

A representação visual detalhada da arquitetura será apresentada posteriormente por meio de diagramas C4 desenvolvidos no Draw.io.

## 2. Visão arquitetural

A solução adota uma arquitetura baseada em microserviços, composta por dois serviços de negócio:

- MicroService.ControleLancamentos
- MicroService.ConsolidadoDiario

Os serviços possuem responsabilidades independentes e bancos de dados próprios.

O Controle de Lançamentos utiliza PostgreSQL como fonte de verdade dos lançamentos.

O Consolidado Diário utiliza MongoDB para manter uma projeção própria dos lançamentos e os dados de consolidação.

A comunicação entre os serviços ocorre de forma assíncrona por meio do RabbitMQ.

A publicação dos eventos do Controle de Lançamentos utiliza Transactional Outbox para reduzir o risco de inconsistência entre a persistência do lançamento e a publicação da mensagem.

A autenticação e autorização são centralizadas no Keycloak.

A observabilidade é realizada por meio de OpenTelemetry, OpenTelemetry Collector, Prometheus, Grafana, Loki e Jaeger.

## 3. Componentes principais

### 3.1 MicroService.ControleLancamentos

Responsável pelo gerenciamento dos lançamentos financeiros.

Principais responsabilidades:

- criação de lançamentos;
- consulta de lançamentos;
- alteração de lançamentos;
- exclusão de lançamentos;
- controle de idempotência;
- controle de versão;
- persistência dos lançamentos;
- publicação dos eventos de integração.

Persistência:

PostgreSQL.

O serviço possui responsabilidade exclusiva sobre os dados de lançamento.

### 3.2 MicroService.ConsolidadoDiario

Responsável pela disponibilização da consolidação diária.

Principais responsabilidades:

- consulta do consolidado;
- geração do consolidado;
- cálculo de receitas;
- cálculo de despesas;
- cálculo do saldo;
- cálculo das quantidades de lançamentos;
- manutenção da projeção dos lançamentos.

Persistência:

MongoDB.

O serviço não acessa diretamente o PostgreSQL do Controle de Lançamentos.

### 3.3 Consumer.ConsolidadoDiario

Responsável pelo processamento dos eventos publicados pelo Controle de Lançamentos.

Principais responsabilidades:

- consumir eventos do RabbitMQ;
- criar a projeção de lançamentos;
- atualizar a projeção;
- marcar lançamentos como excluídos;
- controlar versões recebidas;
- ignorar eventos antigos ou repetidos.

A separação do consumidor permite desacoplar o processamento assíncrono da API de consolidação.

### 3.4 PostgreSQL

Responsável pela persistência transacional dos lançamentos.

Também armazena as informações utilizadas pelo mecanismo de idempotência e pela Transactional Outbox.

O PostgreSQL representa a fonte de verdade do domínio de Controle de Lançamentos.

### 3.5 MongoDB

Responsável pela persistência do modelo utilizado pelo Consolidado Diário.

São mantidas projeções específicas para leitura e consolidação.

O MongoDB não é utilizado como fonte de verdade dos lançamentos.

### 3.6 RabbitMQ

Responsável pela comunicação assíncrona entre o Controle de Lançamentos e o processamento do Consolidado Diário.

Os eventos utilizados são:

- LancamentoCriadoEvent;
- LancamentoAlteradoEvent;
- LancamentoExcluidoEvent.

O RabbitMQ permite que o produtor e o consumidor possuam ciclos de disponibilidade e processamento independentes.

### 3.7 Keycloak

Responsável pela autenticação e autorização.

Os serviços possuem clients independentes e utilizam tokens JWT para proteção das APIs.

As permissões são representadas por roles específicas de cada capacidade.

### 3.8 OpenTelemetry Collector

Responsável por receber os sinais de observabilidade enviados pelas aplicações e encaminhá-los para as ferramentas correspondentes.

São utilizados os sinais:

- traces;
- metrics;
- logs.

### 3.9 Prometheus

Responsável pela coleta e consulta das métricas disponibilizadas pela infraestrutura de observabilidade.

### 3.10 Grafana

Responsável pela visualização dos indicadores de observabilidade.

É utilizado para consultar métricas do Prometheus e logs do Loki.

### 3.11 Loki

Responsável pelo armazenamento e consulta centralizada dos logs.

### 3.12 Jaeger

Responsável pela visualização dos traces distribuídos.

Permite acompanhar o fluxo de processamento entre os componentes da solução.

## 4. Isolamento dos dados

Cada serviço possui responsabilidade sobre sua própria persistência.

O Controle de Lançamentos possui:

- PostgreSQL;
- tabela de lançamentos;
- controle de idempotência;
- estruturas relacionadas à Outbox.

O Consolidado Diário possui:

- MongoDB;
- projeção dos lançamentos;
- consolidados diários.

O compartilhamento direto de banco de dados entre os serviços não faz parte da arquitetura alvo.

Essa decisão reduz o acoplamento e permite que os modelos de persistência evoluam de maneira independente.

## 5. Comunicação entre os serviços

A comunicação entre os contextos ocorre por eventos.

O Controle de Lançamentos publica eventos quando ocorre uma alteração relevante no estado de um lançamento.

Os eventos são:

### LancamentoCriadoEvent

Publicado após a criação de um lançamento.

O consumidor utiliza o evento para criar a projeção correspondente no MongoDB.

### LancamentoAlteradoEvent

Publicado após a alteração de um lançamento.

O consumidor utiliza o evento para atualizar a projeção correspondente.

A versão do lançamento acompanha o evento.

### LancamentoExcluidoEvent

Publicado após a exclusão de um lançamento.

O consumidor marca a projeção correspondente como excluída no MongoDB.

A projeção não precisa ser removida fisicamente, permitindo manter o controle da versão processada.

## 6. Transactional Outbox

A publicação dos eventos é integrada ao processo transacional do Controle de Lançamentos.

Quando uma operação altera o lançamento, o registro do lançamento e o registro da mensagem na Outbox são tratados dentro do mesmo fluxo transacional.

Isso evita o cenário em que o lançamento é persistido com sucesso, mas o evento correspondente não é registrado para publicação.

Após a confirmação da transação, o mecanismo de mensageria realiza o processamento da Outbox e publica os eventos no RabbitMQ.

## 7. Idempotência

A criação de lançamentos utiliza Idempotency-Key.

A chave é armazenada no PostgreSQL juntamente com o hash dos dados relevantes da requisição.

Quando uma chave já existente é reutilizada:

- com os mesmos dados, a operação retorna o lançamento já criado;
- com dados diferentes, a operação retorna conflito.

A restrição de unicidade da chave no banco também contribui para o tratamento de requisições concorrentes.

## 8. Versionamento

Os lançamentos possuem uma propriedade de versão.

A versão inicial é 1.

Cada alteração incrementa a versão.

Os eventos carregam a versão correspondente ao estado publicado.

O consumidor compara a versão recebida com a versão atualmente armazenada na projeção.

Eventos com versão inferior ou igual são ignorados.

Eventos com versão superior atualizam a projeção.

Essa estratégia protege a projeção contra eventos duplicados, reprocessados ou recebidos fora de ordem.

## 9. Consistência

A arquitetura adota consistência eventual entre os serviços.

O PostgreSQL representa o estado oficial dos lançamentos.

O MongoDB representa uma projeção derivada desse estado.

Após uma alteração no Controle de Lançamentos, pode existir um intervalo até que o evento seja processado pelo consumidor e a projeção seja atualizada.

Esse comportamento é intencional e está relacionado à decisão de evitar dependência síncrona entre os serviços.

## 10. Disponibilidade e resiliência

O Controle de Lançamentos não possui uma dependência síncrona do Consolidado Diário.

Consequentemente, a indisponibilidade da API de consolidação não deve impedir a criação, alteração ou exclusão de lançamentos.

A utilização do RabbitMQ e da Transactional Outbox permite desacoplar a disponibilidade do produtor e do consumidor.

O processamento das mensagens utiliza retry para falhas transitórias.

O consumidor também utiliza controle de versão para evitar que reprocessamentos ou eventos atrasados prejudiquem a projeção.

## 11. Escalabilidade

Os serviços podem ser escalados independentemente.

O Consolidado Diário pode receber múltiplas instâncias conforme o aumento da demanda.

O processamento assíncrono permite aumentar a capacidade do consumidor sem alterar a lógica do Controle de Lançamentos.

A arquitetura também permite que os componentes de infraestrutura sejam dimensionados independentemente conforme o crescimento da carga.

## 12. Segurança

As APIs são protegidas por autenticação baseada em JWT.

O Keycloak atua como provedor de identidade.

A autorização é realizada por meio de roles específicas.

No Controle de Lançamentos são utilizadas permissões para leitura e escrita dos lançamentos.

No Consolidado Diário são utilizadas permissões específicas para leitura e geração da consolidação.

Os tokens são validados quanto ao emissor, audiência e validade.

A segurança é aplicada na camada de entrada das APIs, mantendo as regras de negócio desacopladas do provedor de identidade.

## 13. Observabilidade

A solução utiliza OpenTelemetry para instrumentação.

Os dados são encaminhados para o OpenTelemetry Collector.

Os traces são disponibilizados no Jaeger.

As métricas são coletadas pelo Prometheus.

Os logs são enviados para o Loki.

O Grafana centraliza a visualização de métricas e logs.

A observabilidade permite acompanhar:

- volume de requisições;
- duração das requisições;
- percentis de latência;
- métricas de runtime;
- logs das aplicações;
- traces distribuídos;
- comportamento do processamento assíncrono.

## 14. Atendimento aos requisitos não funcionais

### Disponibilidade

O requisito de independência entre Controle de Lançamentos e Consolidado Diário é atendido pela ausência de comunicação síncrona entre os serviços.

### Desempenho

O Consolidado Diário foi submetido a teste de carga utilizando 50 requisições por segundo durante 60 segundos.

O teste executado produziu 3.000 requisições, com 0% de falhas HTTP e P95 de aproximadamente 6,54 ms.

Esse resultado representa uma validação no ambiente de teste utilizado e não uma garantia de desempenho em qualquer ambiente de produção.

### Perda de requisições

No teste realizado, as 3.000 requisições foram concluídas sem falhas HTTP e sem interrupções reportadas.

O resultado ficou dentro do limite de perda definido pelo desafio.

### Resiliência

A solução utiliza:

- comunicação assíncrona;
- Transactional Outbox;
- retry;
- versionamento de eventos;
- processamento independente entre produtor e consumidor.

## 15. Fluxo de criação de lançamento

O cliente realiza uma requisição autenticada para o Controle de Lançamentos.

O serviço valida a autenticação e autorização.

A chave de idempotência é validada.

O lançamento é persistido no PostgreSQL.

O evento correspondente é registrado na Outbox dentro do mesmo fluxo transacional.

Após a confirmação da transação, a mensagem é publicada no RabbitMQ.

O consumidor recebe o evento.

A projeção do lançamento é criada no MongoDB.

O Consolidado Diário passa a considerar o lançamento em suas consultas e consolidações.

## 16. Fluxo de alteração de lançamento

O cliente realiza uma requisição autenticada para alterar o lançamento.

O Controle de Lançamentos atualiza os dados no PostgreSQL.

A versão do lançamento é incrementada.

O evento de alteração é registrado na Outbox.

O evento é publicado no RabbitMQ.

O consumidor verifica a versão recebida.

Caso seja uma versão mais recente, a projeção no MongoDB é atualizada.

O Consolidado Diário passa a utilizar os novos dados.

## 17. Fluxo de exclusão de lançamento

O cliente realiza uma requisição autenticada para excluir o lançamento.

O Controle de Lançamentos atualiza a versão e remove o registro do PostgreSQL.

O evento de exclusão é registrado na Outbox.

O evento é publicado no RabbitMQ.

O consumidor recebe o evento e marca a projeção correspondente no MongoDB como excluída.

O Consolidado Diário desconsidera projeções marcadas como excluídas durante o cálculo.

## 18. Fluxo de consolidação

O cliente realiza uma requisição autenticada ao Consolidado Diário informando a data desejada.

O serviço consulta o MongoDB.

Os lançamentos válidos da data são considerados no cálculo.

Lançamentos marcados como excluídos não participam da consolidação.

São calculados:

- total de receitas;
- total de despesas;
- saldo;
- quantidade de receitas;
- quantidade de despesas.

O consolidado é armazenado no MongoDB e disponibilizado para consulta.

## 19. Justificativa do estilo arquitetural

A arquitetura baseada em microserviços foi escolhida principalmente pela separação clara das capacidades de negócio e pelo requisito de independência de disponibilidade entre Controle de Lançamentos e Consolidado Diário.

A solução não utiliza microserviços apenas como divisão técnica da aplicação. Cada serviço possui uma responsabilidade de negócio definida, modelo de persistência próprio e possibilidade de evolução independente.

A comunicação assíncrona foi escolhida para evitar dependência síncrona entre as capacidades.

A separação das persistências permite que cada serviço utilize uma tecnologia adequada às suas necessidades.

O PostgreSQL atende ao cenário transacional do Controle de Lançamentos.

O MongoDB atende ao modelo orientado à leitura utilizado pela consolidação.

O RabbitMQ fornece a infraestrutura de mensageria necessária para desacoplar produtor e consumidor.

## 20. Evolução futura

A arquitetura permite evoluções sem alteração dos limites fundamentais dos serviços.

Entre as possíveis evoluções estão:

- escalabilidade horizontal independente dos serviços;
- processamento paralelo dos eventos;
- mecanismos mais avançados de controle de mensagens;
- políticas de retenção para dados históricos;
- otimização do cálculo da consolidação;
- criação de novos consumidores para eventos existentes;
- expansão dos dashboards de observabilidade;
- implantação em infraestrutura de produção com alta disponibilidade;
- adoção de mecanismos adicionais de cache quando houver necessidade comprovada.

## 21. Representação C4

A arquitetura será representada visualmente utilizando o modelo C4 no Draw.io.

Os diagramas deverão complementar este documento apresentando, de forma visual:

- Contexto do sistema;
- Containers;
- principais componentes dos serviços;
- relações entre APIs, banco de dados, mensageria, autenticação e observabilidade.

A documentação textual deste arquivo representa as decisões e responsabilidades arquiteturais, enquanto os diagramas C4 serão utilizados para facilitar a comunicação visual da arquitetura.