# Próximos Passos e Evolução Arquitetural

## Objetivo

Este documento apresenta as possibilidades de evolução da solução após a implementação do cenário proposto no desafio.

A arquitetura foi estruturada para atender aos requisitos funcionais e não funcionais definidos, mantendo separação de responsabilidades, independência entre os serviços, comunicação assíncrona, resiliência, segurança, observabilidade e capacidade de evolução.

A arquitetura alvo também considera a jornada com Micro Frontend e BFF. Esses componentes fazem parte da visão arquitetural da solução, enquanto a implementação atual está concentrada nos serviços de backend e na infraestrutura necessária para demonstrar as capacidades principais do desafio.

Os próximos passos descritos neste documento não representam requisitos obrigatórios da implementação atual. São possibilidades de evolução que podem ser adotadas conforme o crescimento do produto, aumento de volume, surgimento de novas jornadas ou necessidades operacionais.

---

## 1. Evolução da experiência do usuário

### 1.1 Micro Frontend

A arquitetura alvo prevê um Micro Frontend para representar a jornada de fluxo de caixa.

O Micro Frontend seria responsável pela experiência do usuário, enquanto as regras e capacidades de negócio permaneceriam nos serviços de backend.

Entre as funcionalidades previstas para essa jornada estão:

- cadastro de lançamentos;
- consulta de lançamentos;
- edição e exclusão;
- visualização do consolidado diário;
- filtros por período;
- indicadores financeiros;
- acompanhamento da atualização do consolidado.

A adoção de Micro Frontends permite que diferentes jornadas do produto evoluam de maneira independente, evitando a criação de um frontend monolítico conforme novas capacidades sejam adicionadas.

A tecnologia específica pode ser definida de acordo com os padrões corporativos e requisitos do produto.

---

## 2. BFF para a jornada

A arquitetura alvo prevê um Backend for Frontend (BFF) como camada de adaptação entre a experiência do usuário e os serviços de domínio.

O frontend não precisa conhecer diretamente a topologia interna dos microserviços.

Entre as possíveis responsabilidades do BFF estão:

- adaptar contratos para as necessidades da interface;
- agregar informações provenientes de diferentes serviços;
- controlar o fluxo da jornada;
- reduzir múltiplas chamadas realizadas pelo frontend;
- ocultar detalhes internos da arquitetura;
- aplicar políticas específicas da experiência.

O BFF não deve assumir regras de negócio pertencentes aos serviços de domínio.

A jornada pode ser representada conceitualmente como:

Micro Frontend → BFF → APIs de domínio → serviços

Essa abordagem mantém a experiência desacoplada da organização interna dos serviços.

---

## 3. API Gateway

Com o crescimento do ecossistema, pode ser introduzida uma camada de API Gateway na borda da arquitetura.

O Gateway poderia centralizar preocupações relacionadas ao acesso externo, como:

- roteamento;
- autenticação;
- rate limiting;
- proteção contra abuso;
- políticas de segurança;
- controle de acesso;
- observabilidade de tráfego;
- aplicação de políticas de entrada.

O Gateway e o BFF devem possuir responsabilidades distintas.

O Gateway representa uma camada de entrada e governança da plataforma, enquanto o BFF representa as necessidades específicas de uma determinada experiência ou canal.

A adoção do Gateway deve ser avaliada conforme o número de consumidores, APIs e políticas que precisem ser centralizadas.

---

## 4. Evolução da consolidação diária

A implementação atual permite gerar o consolidado a partir dos lançamentos persistidos na projeção MongoDB.

Com o crescimento do volume de dados, uma evolução possível seria transformar a consolidação em uma projeção incremental orientada a eventos.

Nesse modelo, eventos de criação, alteração e exclusão poderiam atualizar diretamente a projeção diária correspondente.

As principais vantagens seriam:

- menor volume de dados processado por solicitação;
- menor custo computacional;
- menor latência;
- melhor escalabilidade;
- atualização mais próxima do tempo real.

A geração completa do consolidado poderia continuar existindo como mecanismo de reconstrução, recuperação ou reconciliação da projeção.

Essa evolução também permitiria separar ainda mais claramente o fluxo de escrita do fluxo de leitura.

---

## 5. Reprocessamento e Dead Letter Queue

A infraestrutura de mensageria pode evoluir com mecanismos específicos para falhas permanentes.

Entre eles:

- Dead Letter Queue;
- identificação de mensagens que excederam o número de tentativas;
- reprocessamento manual ou automático;
- observabilidade do backlog;
- classificação de erros transitórios e permanentes;
- replay de eventos.

A finalidade é separar falhas temporárias, tratadas por mecanismos de retry, de falhas que exigem intervenção ou tratamento específico.

Em um ambiente produtivo, o reprocessamento deve possuir controles para evitar duplicidade, preservar idempotência e permitir rastreabilidade das mensagens.

---

## 6. Evolução dos contratos de eventos

Com o crescimento do número de consumidores, os contratos de eventos podem receber mecanismos adicionais de governança.

Possíveis evoluções:

- catálogo de eventos;
- versionamento formal dos contratos;
- validação de schemas;
- compatibilidade entre versões;
- documentação dos eventos;
- Schema Registry;
- políticas de evolução backward compatible.

A estratégia de versionamento utilizada na solução cria uma base para essa evolução.

O objetivo é permitir que novos consumidores sejam adicionados sem exigir alterações coordenadas em todos os produtores e consumidores existentes.

---

## 7. Escalabilidade horizontal

Os serviços foram estruturados de maneira a permitir execução em múltiplas instâncias, desde que as dependências externas e a infraestrutura sejam configuradas adequadamente.

Uma evolução para ambientes de maior escala poderia utilizar:

- múltiplas instâncias dos serviços;
- balanceamento de carga;
- escalabilidade automática;
- aumento do número de consumidores;
- particionamento de filas quando necessário;
- escalabilidade independente de cada capacidade.

O serviço de Consolidação Diária, por exemplo, poderia receber mais instâncias independentemente do Controle de Lançamentos caso seu volume de consultas crescesse de maneira desproporcional.

A escalabilidade deve ser orientada por métricas reais de consumo e pelos requisitos de desempenho definidos para cada serviço.

---

## 8. Cache

Caso as consultas ao consolidado apresentem alto volume e baixa frequência de alteração, poderia ser introduzida uma camada de cache.

Uma solução como Redis poderia ser avaliada para:

- consolidado diário;
- consultas recorrentes;
- dados de referência;
- informações utilizadas por diferentes jornadas.

A adoção de cache deve ser baseada em métricas reais de utilização e não apenas como mecanismo preventivo.

Também seria necessário definir:

- TTL;
- estratégia de invalidação;
- comportamento em caso de indisponibilidade;
- consistência aceitável;
- limites de memória;
- estratégia de aquecimento do cache, quando aplicável.

O cache não deve substituir a fonte de dados nem introduzir uma dependência crítica que comprometa a disponibilidade dos serviços.

---

## 9. Resiliência avançada

A arquitetura atual já utiliza comunicação assíncrona, retry e isolamento entre os serviços.

Em um ambiente com maior quantidade de integrações, poderiam ser adicionados:

- timeout explícito;
- Circuit Breaker;
- Bulkhead;
- rate limiting;
- políticas de degradação;
- fallback para operações não críticas;
- isolamento de recursos.

Esses mecanismos seriam aplicados principalmente em integrações síncronas futuras.

A comunicação assíncrona continuaria sendo preferencial para fluxos que não exigem resposta imediata entre os serviços.

A aplicação das políticas deve considerar o comportamento esperado de cada dependência, evitando a utilização indiscriminada de mecanismos de resiliência.

---

## 10. Segurança em ambientes produtivos

A solução atual utiliza Keycloak para autenticação e autorização.

Para produção, a evolução poderia incluir:

- HTTPS obrigatório;
- armazenamento de secrets em Secret Manager ou Vault;
- rotação automática de credenciais;
- políticas de expiração;
- escopos mais específicos;
- autenticação service-to-service utilizando Client Credentials;
- segregação de permissões por serviço;
- auditoria de operações sensíveis;
- proteção da camada de entrada com WAF;
- políticas específicas para comunicação interna.

Credenciais utilizadas exclusivamente no ambiente local devem permanecer separadas das credenciais de produção.

Também deve ser evitado o armazenamento de segredos reais em código-fonte, arquivos de configuração ou repositórios.

---

## 11. Observabilidade orientada a SLOs

A solução atual possui métricas, logs e distributed tracing utilizando OpenTelemetry.

O OpenTelemetry Collector funciona como camada de coleta e encaminhamento dos dados de observabilidade.

As métricas podem ser acompanhadas pelo Prometheus, os logs pelo Loki e os traces pelo Jaeger, com o Grafana como camada de visualização para métricas e logs.

Em uma evolução operacional, esses dados poderiam ser utilizados para definir SLOs e alertas.

Alguns indicadores relevantes seriam:

- disponibilidade dos serviços;
- percentual de erros HTTP;
- latência p95 e p99;
- throughput;
- backlog das filas;
- quantidade de retries;
- quantidade de mensagens em Dead Letter Queue;
- tempo entre publicação e processamento do evento;
- tempo de atualização do consolidado;
- divergências entre dados de origem e projeções.

A observabilidade passaria de uma capacidade de diagnóstico para uma ferramenta de operação baseada em objetivos de serviço.

---

## 12. Reconciliação de dados

Como a arquitetura utiliza PostgreSQL como fonte de dados do Controle de Lançamentos e MongoDB como projeção utilizada pela Consolidação Diária, existe consistência eventual entre os dois contextos.

Uma evolução importante seria criar um processo de reconciliação.

Esse processo poderia:

1. identificar períodos ou registros inconsistentes;
2. comparar a fonte de dados com a projeção;
3. identificar eventos não processados;
4. reconstruir a projeção;
5. registrar o resultado da reconciliação.

Esse mecanismo aumentaria a capacidade de recuperação operacional sem transformar a comunicação entre os serviços em uma dependência síncrona.

A reconstrução também pode ser utilizada como mecanismo de recuperação em cenários de perda ou inconsistência da projeção.

---

## 13. Auditoria

Operações relevantes poderiam gerar informações de auditoria, como:

- criação de lançamento;
- alteração;
- exclusão;
- usuário responsável;
- data e hora;
- origem da operação;
- identificador de correlação;
- versão anterior e nova versão.

A auditoria poderia evoluir para uma capacidade independente caso requisitos regulatórios ou de negócio justificassem essa separação.

O armazenamento e a retenção dos dados de auditoria devem considerar requisitos legais, segurança e necessidade operacional.

---

## 14. Governança de APIs

Com o crescimento da quantidade de serviços, recomenda-se estabelecer uma governança formal para APIs.

Essa governança poderia definir:

- padrões REST;
- nomenclatura;
- códigos HTTP;
- formato de erros;
- versionamento;
- compatibilidade de contratos;
- autenticação;
- autorização;
- limites de consumo;
- documentação OpenAPI.

O objetivo é garantir consistência entre os diferentes serviços sem criar dependência de implementação entre eles.

A governança também deve estabelecer critérios para evolução e descontinuação de APIs.

---

## 15. Testes de contrato

A solução possui testes unitários e validações funcionais.

Como evolução, testes de contrato poderiam ser introduzidos principalmente na comunicação assíncrona.

Os contratos dos eventos poderiam ser validados para garantir que alterações em um produtor não quebrem consumidores existentes.

Isso se torna especialmente relevante quando novos consumidores forem adicionados ao RabbitMQ.

Os testes poderiam validar:

- estrutura das mensagens;
- campos obrigatórios;
- tipos;
- versões;
- compatibilidade;
- regras de evolução;
- comportamento esperado dos consumidores.

---

## 16. Infraestrutura como código

Em ambientes produtivos, a infraestrutura poderia ser provisionada utilizando Infrastructure as Code.

Uma possível evolução seria utilizar Terraform para provisionar:

- bancos de dados;
- filas;
- redes;
- serviços;
- observabilidade;
- recursos de segurança;
- ambientes de execução.

Isso permitiria reproduzir ambientes de maneira controlada e versionada.

Também facilitaria a criação de ambientes distintos para desenvolvimento, homologação e produção.

---

## 17. Kubernetes

Caso a escala e os requisitos operacionais justifiquem, o Docker Compose utilizado no ambiente local poderia evoluir para uma plataforma baseada em Kubernetes.

Nesse cenário poderiam ser utilizados:

- Deployments;
- Services;
- Horizontal Pod Autoscaler;
- ConfigMaps;
- Secrets;
- probes de liveness e readiness;
- Ingress;
- políticas de rede;
- observabilidade integrada.

A migração não exigiria alteração no domínio ou nas responsabilidades dos serviços.

A infraestrutura de execução seria substituída mantendo os princípios arquiteturais definidos.

A adoção de Kubernetes deve ser justificada por necessidades reais de escala, disponibilidade, operação ou padronização da plataforma, evitando adicionar complexidade sem benefício proporcional.

---

## 18. Alta disponibilidade

Para ambientes críticos, os componentes de infraestrutura poderiam evoluir para configurações de alta disponibilidade.

Entre as possibilidades:

- PostgreSQL com mecanismos de replicação e failover;
- MongoDB em configuração de alta disponibilidade;
- RabbitMQ em cluster;
- múltiplas instâncias dos serviços;
- múltiplas zonas de disponibilidade;
- backups automatizados;
- disaster recovery;
- testes periódicos de restauração.

O nível de redundância deve ser proporcional ao SLA e ao impacto de indisponibilidade.

Também devem ser definidos objetivos de RPO e RTO para orientar as estratégias de backup e recuperação.

---

## 19. Evolução das capacidades de negócio

Novas capacidades podem surgir sem aumentar excessivamente a responsabilidade do Controle de Lançamentos.

Exemplos:

- relatórios financeiros;
- categorização avançada;
- metas financeiras;
- fechamento de períodos;
- notificações;
- auditoria;
- análise financeira;
- exportação de dados;
- dashboards gerenciais.

Cada nova capacidade deve ser avaliada individualmente para determinar se pertence a um contexto existente ou se justifica a criação de um novo contexto.

A divisão deve ser orientada pelas responsabilidades e capacidades de negócio, e não apenas pela quantidade de funcionalidades.

---

## 20. Arquitetura de referência futura

Uma possível evolução da arquitetura pode incorporar novas camadas sem modificar os limites atuais dos serviços de domínio.

A jornada pode evoluir conceitualmente para:

Micro Frontend → API Gateway → BFF → serviços de domínio → eventos → projeções

Nesse cenário:

- o Micro Frontend representa a experiência;
- o Gateway representa a entrada e governança da plataforma;
- o BFF adapta a API para a jornada;
- os microserviços continuam responsáveis pelas capacidades de negócio;
- o RabbitMQ continua permitindo comunicação assíncrona;
- as projeções continuam isolando necessidades de leitura;
- a observabilidade acompanha todo o fluxo;
- o Prometheus permanece responsável pelo acompanhamento das métricas;
- o Loki permanece responsável pela centralização dos logs;
- o Jaeger permanece responsável pela visualização dos traces.

Essa evolução preserva os principais princípios utilizados na solução atual:

- baixo acoplamento;
- separação de responsabilidades;
- independência dos contextos;
- resiliência;
- segurança;
- observabilidade;
- capacidade de evolução.

---

## 21. Priorização sugerida

As evoluções podem ser implementadas de acordo com a necessidade do produto.

### Curto prazo

- implementação da jornada de Micro Frontend;
- implementação do BFF;
- melhoria da documentação dos contratos;
- Dead Letter Queue;
- métricas de backlog e processamento de eventos;
- testes de contrato;
- alertas baseados nos indicadores existentes.

### Médio prazo

- consolidação incremental orientada a eventos;
- reconciliação de dados;
- cache para consultas de alta frequência;
- API Gateway;
- gestão centralizada de secrets;
- auditoria;
- evolução dos SLOs.

### Longo prazo

- escalabilidade automática;
- Kubernetes;
- Infrastructure as Code;
- alta disponibilidade dos componentes de infraestrutura;
- disaster recovery;
- evolução para múltiplas jornadas e novos contextos de negócio.

A priorização deve ser orientada por:

- volume;
- criticidade;
- custo;
- experiência do usuário;
- requisitos não funcionais;
- necessidade operacional;
- maturidade do produto.

---

## Conclusão

A arquitetura atual foi construída para atender ao cenário proposto mantendo os serviços independentes e permitindo evolução incremental.

A arquitetura alvo contempla a jornada com Micro Frontend e BFF, enquanto a implementação atual concentra-se nos serviços de backend, mensageria, persistência, segurança e observabilidade necessários para demonstrar a solução.

A camada de observabilidade já está estruturada com OpenTelemetry, OpenTelemetry Collector, Prometheus, Loki, Jaeger e Grafana. A evolução natural é utilizar esses dados para estabelecer SLOs, alertas e indicadores operacionais.

Os próximos passos não precisam ser implementados antecipadamente. A principal característica desejada é que a arquitetura permita incorporar essas capacidades quando houver uma necessidade real.

A evolução para Micro Frontend, BFF e API Gateway pode ampliar a experiência e a governança da plataforma, enquanto mecanismos como consolidação incremental, reconciliação, Dead Letter Queue, testes de contrato, escalabilidade automática e alta disponibilidade podem ser incorporados progressivamente conforme o produto e a operação amadureçam.

Dessa forma, a solução mantém uma base arquitetural adequada ao cenário atual e apresenta caminhos claros de evolução sem exigir uma reestruturação completa dos serviços existentes.