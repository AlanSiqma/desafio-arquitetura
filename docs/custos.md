# Estimativa de custos de infraestrutura

## 1. Objetivo

Este documento apresenta uma estimativa de infraestrutura para a solução e identifica os principais componentes que influenciam o custo operacional.

A estimativa tem caráter arquitetural e não representa uma cotação comercial definitiva.

## 2. Componentes

A solução é composta pelos seguintes grupos de infraestrutura:

- Controle de Lançamentos;
- Consolidado Diário;
- Consumer;
- PostgreSQL;
- MongoDB;
- RabbitMQ;
- Keycloak;
- OpenTelemetry Collector;
- Prometheus;
- Grafana;
- Loki;
- Jaeger.

Em um ambiente produtivo, esses componentes podem ser executados em máquinas virtuais, containers gerenciados ou plataformas Kubernetes, dependendo dos requisitos de disponibilidade, escala e operação.

## 3. Estratégia de dimensionamento

O dimensionamento deve considerar principalmente:

- volume de lançamentos;
- volume de consultas;
- taxa de eventos;
- quantidade de mensagens pendentes;
- crescimento dos dados;
- retenção de logs;
- retenção de traces;
- quantidade de instâncias dos serviços;
- requisitos de disponibilidade.

O requisito de maior impacto inicial é a capacidade do Consolidado Diário de atender aproximadamente 50 requisições por segundo.

## 4. Serviços de aplicação

Os serviços de aplicação são stateless do ponto de vista da infraestrutura.

Isso permite aumentar horizontalmente a quantidade de instâncias conforme o crescimento da demanda.

Uma configuração inicial de produção poderia utilizar:

- duas instâncias do Controle de Lançamentos;
- duas instâncias do Consolidado Diário;
- duas instâncias do Consumer.

O número de instâncias deve ser ajustado conforme métricas reais de utilização.

## 5. Bancos de dados

### PostgreSQL

O PostgreSQL armazena os lançamentos, as informações de idempotência e os dados relacionados ao Outbox.

Os principais fatores de custo são:

- CPU;
- memória;
- armazenamento;
- IOPS;
- backup;
- alta disponibilidade;
- retenção de backups.

Por representar a fonte de verdade dos lançamentos, o PostgreSQL deve receber prioridade em disponibilidade, backup e recuperação.

### MongoDB

O MongoDB armazena as projeções dos lançamentos e os consolidados diários.

Os principais fatores de custo são:

- armazenamento;
- memória;
- capacidade de leitura;
- capacidade de escrita;
- replicação;
- backup;
- retenção dos dados.

A utilização de uma projeção separada permite dimensionar a camada de consulta independentemente da persistência transacional.

## 6. RabbitMQ

O RabbitMQ é utilizado para a comunicação assíncrona entre os componentes.

Os custos estão relacionados principalmente a:

- memória;
- CPU;
- armazenamento das mensagens;
- volume de eventos;
- retenção de mensagens;
- alta disponibilidade.

Em uma implantação produtiva, o broker deve possuir persistência adequada e mecanismos de recuperação compatíveis com o nível de disponibilidade esperado.

## 7. Keycloak

O Keycloak é utilizado como provedor de identidade.

Seu dimensionamento depende principalmente de:

- quantidade de usuários;
- quantidade de autenticações;
- quantidade de tokens emitidos;
- quantidade de clients;
- necessidade de alta disponibilidade.

Para o cenário do desafio, a carga de autenticação tende a ser significativamente inferior à carga de consultas do Consolidado Diário.

## 8. Observabilidade

A stack de observabilidade possui componentes adicionais:

- OpenTelemetry Collector;
- Prometheus;
- Grafana;
- Loki;
- Jaeger.

Esses componentes podem representar uma parcela relevante do custo em ambientes com alto volume de logs e traces.

Os principais fatores são:

- quantidade de métricas;
- cardinalidade das métricas;
- volume de logs;
- volume de traces;
- retenção;
- armazenamento.

A retenção deve ser definida de acordo com a necessidade operacional para evitar custos desnecessários.

## 9. Estimativa inicial

Como referência arquitetural, uma implantação inicial de pequeno porte poderia ser dimensionada da seguinte forma:

| Componente | Quantidade inicial | Observação |
| --- | ---: | --- |
| Controle de Lançamentos | 2 instâncias | Permitir disponibilidade e escala horizontal |
| Consolidado Diário | 2 instâncias | Atender crescimento de consultas |
| Consumer | 2 instâncias | Permitir processamento paralelo |
| PostgreSQL | 1 instância primária + backup | Fonte de verdade dos lançamentos |
| MongoDB | 1 instância ou cluster inicial | Projeção e consolidação |
| RabbitMQ | 1 instância ou cluster | Mensageria assíncrona |
| Keycloak | 1 instância ou cluster | Autenticação e autorização |
| OpenTelemetry Collector | 1 ou mais instâncias | Conforme volume de telemetria |
| Prometheus | 1 instância | Métricas |
| Grafana | 1 instância | Visualização |
| Loki | 1 instância | Logs |
| Jaeger | 1 instância | Traces |

Essa configuração representa um ponto de partida. Componentes críticos podem ser distribuídos em múltiplas instâncias conforme os requisitos de disponibilidade.

## 10. Faixa de custo

Sem definir um provedor de nuvem, região, modelo de contratação e política de retenção, não é possível estabelecer um valor financeiro confiável.

A estimativa deve ser construída a partir de:

- custo das instâncias de aplicação;
- custo dos bancos gerenciados;
- custo do broker;
- custo do Keycloak;
- custo de armazenamento;
- custo de transferência de dados;
- custo de observabilidade;
- custo de backups;
- custo de alta disponibilidade.

Para uma implantação pequena, a principal estratégia de redução de custo é evitar dimensionamento excessivo e utilizar escala horizontal somente conforme os indicadores demonstrarem necessidade.

## 11. Estratégia de otimização

A arquitetura permite otimizar custos sem alterar os limites de negócio.

O Controle de Lançamentos pode ser escalado independentemente do Consolidado Diário.

O Consumer pode aumentar a quantidade de instâncias quando o volume de eventos crescer.

O Consolidado Diário pode ser escalado de acordo com a quantidade de consultas.

A infraestrutura de observabilidade pode possuir políticas de retenção diferentes para métricas, logs e traces.

## 12. Alta disponibilidade e custo

Alta disponibilidade aumenta o custo de infraestrutura.

A decisão deve considerar o impacto financeiro de uma indisponibilidade.

Para os componentes críticos, uma implantação produtiva pode utilizar:

- múltiplas instâncias de aplicação;
- banco com mecanismo de alta disponibilidade;
- broker redundante;
- backups automatizados;
- monitoramento e alertas.

O nível de redundância deve ser definido conforme o SLA esperado.

## 13. Licenciamento

A solução utiliza componentes de código aberto ou com opções de uso que não exigem licença comercial específica para o cenário apresentado.

Entre os principais componentes estão:

- .NET;
- ASP.NET Core;
- Entity Framework Core;
- PostgreSQL;
- MongoDB;
- RabbitMQ;
- MassTransit 8.3.6;
- Keycloak;
- OpenTelemetry;
- Prometheus;
- Grafana;
- Loki;
- Jaeger;
- xUnit;
- Moq;
- k6.

A adoção de componentes open source reduz o custo direto de licenciamento, mas não elimina custos de infraestrutura, operação, suporte e manutenção.

As licenças e condições de uso devem ser verificadas novamente antes de uma implantação comercial.

## 14. Ambiente do desafio

O ambiente utilizado para desenvolvimento e validação é executado localmente com containers.

Essa abordagem reduz o custo durante o desenvolvimento e permite reproduzir a infraestrutura necessária sem depender de serviços gerenciados.

O ambiente local não deve ser interpretado como equivalente ao dimensionamento de produção.

## 15. Evolução do dimensionamento

O dimensionamento deve ser orientado por métricas reais.

Os principais indicadores para decidir novos recursos são:

- CPU;
- memória;
- latência P95;
- taxa de erro;
- quantidade de mensagens pendentes;
- tempo de processamento;
- crescimento do armazenamento;
- volume de logs;
- volume de traces.

A expansão de infraestrutura deve ocorrer quando os indicadores demonstrarem necessidade, evitando provisionamento antecipado excessivo.

## 16. Conclusão

A arquitetura foi estruturada para permitir crescimento independente dos componentes.

A separação entre serviços, persistências e mensageria permite ajustar capacidade conforme o perfil de cada componente.

O custo final depende principalmente do nível de disponibilidade, volume de dados, volume de telemetria e estratégia de hospedagem escolhida.

A estimativa apresentada deve ser utilizada como referência para planejamento inicial e substituída por uma cotação baseada no ambiente de implantação escolhido.