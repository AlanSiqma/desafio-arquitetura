# ADR-0009 — OpenTelemetry para observabilidade

## Status

Aceito

## Contexto

A solução é composta por múltiplos serviços e componentes de infraestrutura, incluindo:

- `MicroService.ControleLancamentos`
- `MicroService.ConsolidadoDiario`
- `Consumer.ConsolidadoDiario`
- PostgreSQL
- MongoDB
- RabbitMQ
- Keycloak

A comunicação entre os serviços ocorre de forma assíncrona por meio de eventos e RabbitMQ.

Nesse cenário, apenas logs locais não são suficientes para acompanhar o comportamento da solução, identificar falhas e analisar o desempenho das requisições e do processamento assíncrono.

Também existe a necessidade de demonstrar monitoramento e observabilidade como parte da solução arquitetural.

## Decisão

Adotar OpenTelemetry como padrão de instrumentação para observabilidade da aplicação.

A solução utilizará os três principais sinais de observabilidade:

- Traces
- Metrics
- Logs

Os dados serão enviados por OTLP para um OpenTelemetry Collector.

O Collector será responsável por receber, processar e encaminhar os dados para as ferramentas de observabilidade utilizadas no ambiente.

A solução utilizará:

- Jaeger para visualização de traces.
- Prometheus para armazenamento e consulta de métricas.
- Grafana para criação de dashboards.
- Loki para armazenamento e consulta de logs.
- OpenTelemetry Collector como camada intermediária de coleta e distribuição dos sinais.

## Motivações

### Rastreamento distribuído

Os traces permitem acompanhar uma requisição através dos diferentes componentes envolvidos no processamento.

Isso é especialmente relevante para o fluxo que inicia no `ControleLancamentos`, passa pela publicação do evento no RabbitMQ e termina no processamento pelo consumidor.

### Monitoramento de desempenho

As métricas permitem acompanhar indicadores como quantidade de requisições e duração das operações.

Também possibilitam analisar percentis de latência, como P95, utilizados para avaliar o comportamento da API.

### Centralização de logs

Os logs dos serviços podem ser enviados para uma infraestrutura centralizada, facilitando a investigação de erros e comportamentos inesperados.

### Independência das aplicações

Os serviços não precisam conhecer diretamente cada ferramenta utilizada para armazenamento e visualização dos dados de observabilidade.

O OpenTelemetry Collector atua como uma camada intermediária.

### Padronização

A utilização do OpenTelemetry fornece um padrão comum de instrumentação para os diferentes serviços da solução.

## Alternativas consideradas

### Apenas logs locais

Descartada porque dificulta o acompanhamento de fluxos distribuídos e a análise centralizada dos serviços.

### Instrumentação específica para cada ferramenta

Descartada porque aumentaria o acoplamento das aplicações às ferramentas de observabilidade.

### Utilização direta de Prometheus, Jaeger e Loki pelas aplicações

Descartada porque faria com que cada aplicação tivesse maior responsabilidade sobre os mecanismos específicos de exportação e integração.

## Consequências

### Positivas

- Permite rastreamento distribuído.
- Permite coleta de métricas das aplicações.
- Permite centralização dos logs.
- Facilita investigação de falhas.
- Permite criação de dashboards operacionais.
- Reduz o acoplamento entre as aplicações e as ferramentas de observabilidade.
- Facilita a evolução futura da infraestrutura de observabilidade.

### Negativas

- Adiciona componentes de infraestrutura à solução.
- A operação passa a depender de ferramentas adicionais para armazenamento e visualização.
- Existe custo computacional e de armazenamento associado à coleta dos dados.
- É necessário definir retenção e volume adequado de dados em ambientes produtivos.

## Implementação

Os serviços ASP.NET Core utilizam instrumentação do OpenTelemetry para coleta de traces, métricas e logs.

Os dados são enviados utilizando OTLP para o OpenTelemetry Collector.

O Collector recebe os dados por OTLP utilizando os protocolos gRPC e HTTP.

Os traces são encaminhados para o Jaeger.

As métricas são disponibilizadas para coleta pelo Prometheus.

Os logs são encaminhados para o Loki.

O Grafana utiliza Prometheus e Loki como fontes de dados para os dashboards.

A solução possui métricas para acompanhamento das requisições HTTP e métricas de runtime das aplicações.

Também são coletados traces relacionados às requisições HTTP e ao processamento distribuído.

## Resultado esperado

A solução deve permitir acompanhar o comportamento dos serviços por meio de traces, métricas e logs centralizados.

Deve ser possível identificar requisições, analisar latência, acompanhar métricas dos serviços e consultar logs centralizados no Grafana.

O fluxo distribuído entre `ControleLancamentos` e `ConsolidadoDiario` deve ser observável, permitindo investigar problemas sem depender exclusivamente dos logs individuais de cada serviço.