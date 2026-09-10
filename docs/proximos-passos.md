# Próximos Passos e Evolução Arquitetural

## Objetivo

Este documento apresenta possibilidades de evolução da solução após a implementação da arquitetura proposta para o desafio.

A arquitetura atual foi estruturada para atender aos requisitos funcionais e não funcionais definidos, mantendo separação de responsabilidades, independência entre os serviços, comunicação assíncrona, resiliência, segurança, observabilidade e capacidade de evolução.

Os próximos passos apresentados não representam requisitos obrigatórios da implementação atual. São possibilidades de evolução que podem ser adotadas conforme o crescimento do produto, aumento de volume, novas jornadas, necessidades de integração ou requisitos operacionais.

---

## 1. Evolução da jornada com Micro Frontend

A arquitetura prevê um Micro Frontend para representar a jornada de fluxo de caixa.

A evolução dessa camada pode contemplar a implementação progressiva das funcionalidades necessárias para o usuário, como:

- cadastro de lançamentos;
- consulta de lançamentos;
- edição e exclusão;
- visualização do consolidado diário;
- filtros por período;
- indicadores financeiros;
- acompanhamento da atualização do consolidado.

O Micro Frontend deve permanecer responsável pela experiência do usuário, enquanto as regras e capacidades de negócio permanecem nos serviços de backend.

A adoção de Micro Frontends também permite que novas jornadas sejam desenvolvidas de forma independente, evitando a concentração de diferentes funcionalidades em um único frontend.

---

## 2. Evolução do BFF

O BFF previsto na arquitetura pode evoluir para atender especificamente às necessidades da jornada de fluxo de caixa.

Entre suas responsabilidades podem estar:

- adaptar contratos para as necessidades da interface;
- agregar informações provenientes de diferentes serviços;
- reduzir múltiplas chamadas realizadas pelo frontend;
- ocultar detalhes internos da topologia de serviços;
- aplicar políticas específicas da experiência;
- controlar a composição da resposta da jornada.

O BFF não deve assumir regras de negócio pertencentes aos serviços de domínio.

A separação de responsabilidades deve permanecer:

Micro Frontend → BFF → serviços de domínio

Dessa forma, mudanças na experiência do usuário podem ocorrer sem exigir alterações desnecessárias nos contratos internos dos serviços.

---

## 3. Integração com Protheus e sistemas legados

Considerando a presença do Protheus no contexto corporativo, uma evolução importante é estabelecer uma estratégia de integração desacoplada entre o sistema legado e os novos serviços.

A integração deve ser definida de acordo com as capacidades disponíveis no ambiente corporativo, evitando acoplamento direto entre o Protheus e os serviços internos sempre que possível.

Entre os aspectos a serem considerados estão:

- definição clara de ownership dos dados;
- contratos de integração;
- tratamento de indisponibilidade do sistema legado;
- processamento assíncrono quando aplicável;
- consistência eventual;
- controle de duplicidade;
- rastreabilidade das integrações;
- monitoramento de erros;
- reprocessamento de mensagens;
- evolução gradual da dependência do legado.

A integração com o Protheus deve ser tratada como uma fronteira arquitetural, evitando que detalhes do sistema legado sejam propagados para os demais contextos da solução.

---

## 4. API Gateway

Com o crescimento do ecossistema, pode ser introduzida uma camada de API Gateway na borda da arquitetura.

O Gateway poderia centralizar preocupações relacionadas ao acesso externo, como:

- roteamento;
- autenticação;
- rate limiting;
- proteção contra abuso;
- políticas de segurança;
- controle de acesso;
- observabilidade do tráfego;
- aplicação de políticas de entrada.

O Gateway e o BFF devem possuir responsabilidades distintas.

O Gateway representa uma camada de entrada e governança da plataforma, enquanto o BFF representa as necessidades específicas de uma determinada experiência ou canal.

Uma possível evolução seria:

Micro Frontend → API Gateway → BFF → serviços de domínio

---

## 5. Evolução da consolidação diária

A implementação atual permite gerar o consolidado a partir dos lançamentos persistidos na projeção MongoDB.

Com o crescimento do volume de dados, uma evolução possível seria transformar a consolidação em uma projeção incremental orientada a eventos.

Nesse modelo, eventos de criação, alteração e exclusão poderiam atualizar diretamente a projeção diária correspondente.

As vantagens potenciais seriam:

- menor volume de dados processado;
- menor custo computacional;
- menor latência;
- melhor escalabilidade;
- atualização mais próxima do tempo real.

A geração completa do consolidado poderia continuar existindo como mecanismo de reconstrução ou reconciliação.

---

## 6. Reprocessamento e Dead Letter Queue

A infraestrutura de mensageria pode evoluir com mecanismos específicos para falhas permanentes.

Entre eles:

- Dead Letter Queue;
- identificação de mensagens que excederam o número de tentativas;
- reprocessamento manual ou automático;
- observabilidade do backlog;
- classificação de erros transitórios e permanentes;
- replay de eventos.

Essa evolução permitiria separar falhas temporárias, tratadas pelas políticas de retry, de falhas que exigem intervenção ou tratamento específico.

---

## 7. Evolução dos contratos de eventos

Com o crescimento do número de consumidores, os contratos de eventos podem receber mecanismos adicionais de governança.

Possíveis evoluções:

- catálogo de eventos;
- versionamento formal dos contratos;
- validação de schemas;
- compatibilidade entre versões;
- documentação dos eventos;
- Schema Registry;
- políticas de evolução backward compatible.

A estratégia de versionamento já utilizada na solução cria uma base para essa evolução.

---

## 8. Escalabilidade horizontal

Os serviços foram construídos de maneira stateless, permitindo que novas instâncias sejam adicionadas conforme a demanda.

Uma evolução para ambientes de maior escala poderia utilizar:

- múltiplas instâncias dos serviços;
- balanceamento de carga;
- escalabilidade automática;
- aumento do número de consumidores;
- particionamento de filas quando necessário;
- escalabilidade independente de cada capacidade.

O Serviço do Consolidado Diário, por exemplo, poderia receber mais instâncias independentemente do Controle de Lançamentos caso seu volume de consultas crescesse de maneira desproporcional.

---

## 9. Cache

Caso as consultas ao consolidado apresentem alto volume e baixa frequência de alteração, poderia ser introduzida uma camada de cache.

Uma solução como Redis poderia ser avaliada para:

- consolidado diário;
- consultas recorrentes;
- dados de referência;
- informações utilizadas por diferentes jornadas.

A adoção de cache deve ser baseada em métricas reais de utilização.

Também seria necessário definir:

- TTL;
- estratégia de invalidação;
- comportamento em caso de indisponibilidade;
- consistência aceitável;
- limites de memória.

---

## 10. Resiliência avançada

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

---

## 11. Segurança em ambientes produtivos

A configuração atual utiliza Keycloak para autenticação e autorização.

Para produção, a evolução poderia incluir:

- HTTPS obrigatório;
- armazenamento de secrets em Secret Manager ou Vault;
- rotação automática de credenciais;
- políticas de expiração;
- escopos mais específicos;
- autenticação service-to-service utilizando Client Credentials;
- segregação de permissões por serviço;
- auditoria de operações sensíveis;
- proteção da camada de entrada com WAF.

Credenciais utilizadas exclusivamente no ambiente local devem permanecer separadas das credenciais de produção.

---

## 12. Observabilidade orientada a SLOs

A solução já possui métricas, logs e distributed tracing.

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
- divergências entre dados de origem e projeções;
- disponibilidade das integrações com sistemas externos.

A observabilidade passaria de uma capacidade de diagnóstico para uma ferramenta de operação baseada em objetivos de serviço.

---

## 13. Reconciliação de dados

Como a arquitetura utiliza PostgreSQL como fonte de dados do Controle de Lançamentos e MongoDB como projeção utilizada pela Consolidação Diária, existe consistência eventual entre os dois contextos.

Uma evolução importante seria criar um processo de reconciliação.

Esse processo poderia:

1. identificar períodos ou registros inconsistentes;
2. comparar a fonte de dados com a projeção;
3. identificar eventos não processados;
4. reconstruir a projeção;
5. registrar o resultado da reconciliação.

Esse mecanismo aumentaria a capacidade de recuperação operacional sem transformar a comunicação entre os serviços em uma dependência síncrona.

---

## 14. Auditoria

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

---

## 15. Governança de APIs

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

O objetivo seria garantir consistência entre os diferentes serviços sem criar dependência de implementação entre eles.

---

## 16. Testes de contrato

A solução possui testes unitários e validações funcionais.

Como evolução, testes de contrato poderiam ser introduzidos principalmente na comunicação assíncrona e nas integrações externas.

Os contratos dos eventos e das APIs poderiam ser validados para garantir que alterações em um produtor não quebrem consumidores existentes.

Essa abordagem se torna especialmente relevante com a entrada de novos consumidores, novos canais ou integrações com sistemas corporativos.

---

## 17. Infraestrutura como código

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

---

## 18. Kubernetes

Caso a escala e os requisitos operacionais justifiquem, o Docker Compose utilizado no ambiente local poderia evoluir para uma plataforma baseada em Kubernetes.

Nesse cenário poderiam ser utilizados:

- Deployments;
- Services;
- Horizontal Pod Autoscaler;
- ConfigMaps;
- Secrets;
- probes de liveness e readiness;
- ingress;
- políticas de rede;
- observabilidade integrada.

A migração não exigiria alteração no domínio ou nas responsabilidades dos serviços. A infraestrutura de execução seria substituída mantendo os princípios arquiteturais definidos.

---

## 19. Alta disponibilidade e Disaster Recovery

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

O nível de redundância deve ser proporcional ao SLA, ao RTO, ao RPO e ao impacto de indisponibilidade.

---

## 20. Evolução das capacidades de negócio

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

---

## 21. Evolução da integração com sistemas corporativos

À medida que novos sistemas corporativos forem incorporados à solução, recomenda-se manter uma estratégia de integração desacoplada.

Novas integrações devem considerar:

- contratos bem definidos;
- isolamento dos sistemas externos;
- tratamento de indisponibilidade;
- idempotência;
- retries;
- rastreabilidade;
- monitoramento;
- controle de versões;
- reprocessamento;
- ownership dos dados.

O objetivo é evitar que a arquitetura dos sistemas externos determine diretamente a arquitetura interna dos novos serviços.

---

## 22. Arquitetura de referência futura

Uma possível evolução da arquitetura pode incorporar novas camadas sem modificar os limites atuais dos serviços de domínio.

A jornada poderia evoluir para:

Micro Frontend → API Gateway → BFF → serviços de domínio → eventos → projeções

Nesse cenário:

- o Micro Frontend representa a experiência;
- o Gateway representa a entrada e governança da plataforma;
- o BFF adapta a API para a jornada;
- os microserviços continuam responsáveis pelas capacidades de negócio;
- o RabbitMQ continua permitindo comunicação assíncrona;
- as projeções continuam isolando necessidades de leitura;
- as integrações externas permanecem protegidas por fronteiras bem definidas;
- a observabilidade acompanha todo o fluxo.

Essa evolução preserva os principais princípios utilizados na solução atual: baixo acoplamento, separação de responsabilidades, independência dos contextos, resiliência e capacidade de evolução.

---

## 23. Priorização sugerida

As evoluções podem ser implementadas de acordo com a necessidade do produto.

### Curto prazo

- evolução da implementação do Micro Frontend da jornada;
- evolução do BFF;
- melhoria da documentação dos contratos;
- Dead Letter Queue;
- métricas de backlog e processamento de eventos;
- testes de contrato;
- observabilidade das integrações externas;
- alertas baseados nos indicadores existentes.

### Médio prazo

- integração estruturada com o Protheus e demais sistemas corporativos;
- consolidação incremental orientada a eventos;
- reconciliação de dados;
- cache para consultas de alta frequência;
- API Gateway;
- gestão centralizada de secrets;
- auditoria.

### Longo prazo

- escalabilidade automática;
- Kubernetes;
- Infrastructure as Code;
- alta disponibilidade dos componentes de infraestrutura;
- disaster recovery;
- evolução para múltiplas jornadas e novos contextos de negócio;
- expansão das integrações corporativas.

A priorização deve ser orientada por volume, criticidade, custo, experiência do usuário e requisitos não funcionais.

---

## Conclusão

A arquitetura atual foi construída para atender ao cenário proposto mantendo os serviços independentes e permitindo evolução incremental.

Os próximos passos não precisam ser implementados antecipadamente. A principal característica desejada é que a arquitetura permita incorporar essas capacidades quando houver uma necessidade real.

A jornada já contempla conceitualmente Micro Frontend e BFF, permitindo que a experiência de fluxo de caixa evolua sem alterar os limites dos serviços de domínio.

A integração com o Protheus e outros sistemas corporativos deve evoluir mantendo uma fronteira clara entre o legado e os novos serviços.

Mecanismos como API Gateway, consolidação incremental, reconciliação, governança de eventos, escalabilidade automática, alta disponibilidade e disaster recovery podem ser incorporados progressivamente conforme o produto e a operação amadureçam.

Dessa forma, a solução mantém uma base arquitetural adequada ao cenário atual e possui caminhos claros para evolução sem exigir uma reestruturação completa dos serviços existentes.