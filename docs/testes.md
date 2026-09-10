# Estratégia de testes

## 1. Objetivo

Este documento apresenta a estratégia de testes utilizada para validar o comportamento da solução.

A estratégia prioriza testes automatizados das regras de negócio, serviços e consumidores, complementados por validações funcionais e teste de carga.

## 2. Objetivos da estratégia

Os testes têm como objetivos:

- validar as regras de negócio;
- validar o comportamento dos serviços;
- validar o processamento dos eventos;
- validar idempotência;
- validar versionamento;
- validar exclusão lógica na projeção;
- validar o cálculo da consolidação diária;
- validar os requisitos de desempenho;
- reduzir regressões durante a evolução da solução.

## 3. Organização

Os projetos de teste estão separados de acordo com os componentes da solução.

### Controle de Lançamentos

MicroService.ControleLancamentos.Test

Responsável pelos testes do serviço de controle de lançamentos.

### Consolidado Diário

MicroService.ConsolidadoDiario.Test

Responsável pelos testes do serviço responsável pela consolidação diária.

### Consumer

Consumer.ConsolidadoDiario.Test

Responsável pelos testes do processamento dos eventos recebidos pelo consumidor.

### Teste de carga

tests/LoadTest/consolidado-50rps.js

Contém o cenário de teste de carga utilizado para validar o requisito de 50 requisições por segundo.

## 4. Testes unitários

Os testes unitários são utilizados para validar comportamentos isolados dos componentes.

A estratégia utiliza xUnit como framework de testes e Moq para criação de mocks quando necessário.

Os testes não dependem de uma infraestrutura externa para validar as principais regras de negócio.

Isso permite execução rápida e repetível durante o desenvolvimento.

## 5. Controle de Lançamentos

Os testes do Controle de Lançamentos cobrem principalmente:

- criação de lançamento;
- publicação do evento de criação;
- alteração de lançamento;
- incremento da versão;
- publicação do evento de alteração;
- exclusão de lançamento;
- publicação do evento de exclusão;
- consulta por identificador;
- listagem;
- comportamento quando o lançamento não existe;
- idempotência;
- conflito de idempotência.

A idempotência possui cenários específicos para garantir que:

1. uma requisição repetida com a mesma chave e os mesmos dados retorne o lançamento existente;
2. uma mesma chave utilizada com dados diferentes seja rejeitada.

## 6. Consumer

Os testes do consumidor validam o comportamento da projeção no MongoDB.

São cobertos cenários como:

- criação de documento a partir de LancamentoCriadoEvent;
- atualização quando o evento possui versão mais recente;
- ignorar evento com versão igual;
- ignorar evento com versão anterior;
- atualização a partir de LancamentoAlteradoEvent;
- comportamento quando o lançamento projetado não existe;
- exclusão lógica a partir de LancamentoExcluidoEvent;
- ignorar exclusão de documento inexistente.

Essa estratégia valida a regra de versionamento utilizada para proteger a projeção contra eventos fora de ordem ou repetidos.

## 7. Consolidação diária

Os testes do Consolidado Diário validam:

- cálculo do total de receitas;
- cálculo do total de despesas;
- cálculo do saldo;
- quantidade de receitas;
- quantidade de despesas;
- exclusão de lançamentos marcados como excluídos;
- retorno de consolidado já existente;
- geração de consolidado quando ele ainda não existe.

O cálculo do saldo é validado pela diferença entre o total de receitas e o total de despesas.

## 8. Resultado dos testes automatizados

A suíte atual possui 19 testes automatizados.

Resultado obtido:

- 19 testes executados;
- 19 testes aprovados;
- 0 testes com falha.

Distribuição atual:

- 8 testes do Controle de Lançamentos;
- 4 testes do Consolidado Diário;
- 7 testes do Consumer.

## 9. Testes funcionais

Além dos testes automatizados, foi realizado um fluxo funcional utilizando Postman.

O fluxo validado contempla:

1. autenticação no Keycloak;
2. criação de lançamento;
3. captura do identificador do lançamento;
4. consulta do lançamento;
5. alteração do lançamento;
6. nova consulta;
7. consulta do consolidado diário;
8. exclusão do lançamento;
9. nova consulta do consolidado diário.

Esse fluxo permitiu validar a integração entre:

- Keycloak;
- Controle de Lançamentos;
- PostgreSQL;
- Outbox;
- RabbitMQ;
- Consumer;
- MongoDB;
- Consolidado Diário.

## 10. Validação da exclusão

A exclusão possui comportamento diferente nas duas persistências.

No PostgreSQL, o lançamento é removido.

No MongoDB, o documento projetado recebe a marcação Excluido.

O Consolidado Diário considera somente documentos que não estão marcados como excluídos.

O fluxo funcional confirmou que, após a exclusão de um lançamento, o consolidado deixa de considerá-lo no cálculo.

## 11. Teste de resiliência

Foi realizada uma validação de disponibilidade interrompendo o serviço Consolidado Diário.

Durante a indisponibilidade do Consolidado Diário, o Controle de Lançamentos permaneceu disponível.

Isso demonstra o comportamento esperado pela arquitetura, na qual o Controle de Lançamentos não possui dependência síncrona do serviço de consolidação.

Os eventos permanecem associados ao fluxo assíncrono de processamento.

## 12. Teste de carga

O requisito de desempenho do Consolidado Diário foi validado utilizando k6.

O cenário utilizado possui:

- taxa de 50 requisições por segundo;
- duração de 60 segundos;
- aproximadamente 3.000 requisições;
- autenticação por token;
- consulta do consolidado diário.

## 13. Resultado do teste de carga

O resultado final observado foi:

- 3.000 requisições;
- aproximadamente 50,03 requisições por segundo;
- 0% de falhas HTTP;
- 100% dos checks com sucesso;
- latência média de 4,81 ms;
- P90 de 5,91 ms;
- P95 de 6,54 ms;
- latência máxima de 19,31 ms.

O resultado observado atende ao cenário de validação utilizado para o requisito de 50 requisições por segundo e permanece abaixo do limite de 5% de falha definido para o teste.

## 14. Critérios de aceite

A solução é considerada validada quando:

- os testes automatizados são executados sem falhas;
- as regras principais do domínio possuem cobertura por testes;
- os eventos são processados corretamente;
- eventos antigos não sobrescrevem versões mais novas;
- a idempotência impede duplicação de operações;
- a exclusão é refletida na projeção;
- o consolidado apresenta os valores esperados;
- o fluxo funcional completo é executado com sucesso;
- a indisponibilidade do Consolidado Diário não derruba o Controle de Lançamentos;
- o teste de carga apresenta taxa de erro inferior a 5%.

## 15. Limitações

A estratégia atual não possui uma suíte dedicada de testes de integração automatizados.

A integração entre os componentes foi validada por meio do fluxo funcional executado com a infraestrutura real e pelo teste de carga.

Uma evolução possível é adicionar testes de integração automatizados utilizando containers temporários para PostgreSQL, MongoDB, RabbitMQ e Keycloak.

Essa evolução aumentaria a cobertura das interações entre infraestrutura e aplicação, mas não é necessária para validar as principais regras de negócio já cobertas pelos testes atuais.

## 16. Evolução da estratégia

Conforme a solução evoluir, a estratégia poderá incorporar:

- testes de integração automatizados;
- testes de contrato dos eventos;
- testes de segurança;
- testes de falha do RabbitMQ;
- testes de recuperação após indisponibilidade;
- testes de concorrência;
- testes de carga com diferentes volumes;
- testes de longa duração;
- execução automática da suíte no pipeline de CI.

## 17. Resultado esperado

A estratégia combina testes unitários, validação funcional, teste de resiliência e teste de carga.

Essa combinação permite validar tanto o comportamento interno dos componentes quanto os principais requisitos arquiteturais e não funcionais da solução.