# Requisitos funcionais e não funcionais

## 1. Objetivo

Esta documentação apresenta o refinamento dos requisitos do desafio de arquitetura para uma solução de controle de fluxo de caixa diário.

A solução deve permitir que um comerciante registre seus lançamentos financeiros e consulte um consolidado diário do fluxo de caixa.

O desafio define dois serviços de negócio:

- Serviço de controle de lançamentos.
- Serviço de consolidado diário.

Além dos requisitos funcionais, a solução deve considerar disponibilidade, desempenho, confiabilidade, segurança, observabilidade e capacidade de evolução.

---

## 2. Escopo da solução

A solução será composta por dois contextos funcionais independentes.

### Controle de Lançamentos

Responsável pelo gerenciamento dos lançamentos financeiros.

O serviço deve permitir:

- criação de lançamentos;
- consulta de lançamento por identificador;
- consulta de lançamentos;
- alteração de lançamentos;
- exclusão de lançamentos.

Cada lançamento representa uma entrada ou saída do fluxo de caixa.

### Consolidado Diário

Responsável por disponibilizar a consolidação dos lançamentos de um determinado dia.

O serviço deve permitir:

- consulta do consolidado diário;
- geração ou atualização do consolidado diário.

A consolidação deve considerar os lançamentos válidos correspondentes à data consultada.

---

## 3. Requisitos funcionais

### RF01 — Criar lançamento

O sistema deve permitir a criação de um lançamento financeiro.

O lançamento deve possuir, no mínimo:

- descrição;
- valor;
- data;
- tipo do lançamento;
- categoria.

O tipo do lançamento deve representar uma receita ou uma despesa.

### RF02 — Consultar lançamento

O sistema deve permitir consultar um lançamento individual por seu identificador.

### RF03 — Listar lançamentos

O sistema deve permitir consultar os lançamentos registrados.

### RF04 — Alterar lançamento

O sistema deve permitir alterar os dados de um lançamento existente.

A alteração deve gerar uma nova versão do lançamento para permitir o controle da evolução do estado publicado para os demais componentes da solução.

### RF05 — Excluir lançamento

O sistema deve permitir excluir um lançamento existente.

A exclusão deve ser propagada para o modelo utilizado pelo consolidado diário.

### RF06 — Consolidar lançamentos por dia

O sistema deve permitir gerar o consolidado de um determinado dia.

O consolidado deve apresentar, no mínimo:

- total de receitas;
- total de despesas;
- saldo;
- quantidade de receitas;
- quantidade de despesas.

O saldo deve ser calculado pela diferença entre o total de receitas e o total de despesas.

### RF07 — Consultar consolidado diário

O sistema deve permitir consultar o consolidado de uma determinada data.

Caso ainda não exista uma consolidação armazenada para a data solicitada, o serviço poderá gerar a consolidação a partir dos lançamentos disponíveis.

### RF08 — Propagar alterações entre os serviços

As operações de criação, alteração e exclusão de lançamentos devem gerar eventos para atualização da projeção utilizada pelo serviço de consolidado diário.

Os eventos previstos são:

- LancamentoCriadoEvent;
- LancamentoAlteradoEvent;
- LancamentoExcluidoEvent.

### RF09 — Garantir idempotência na criação

A criação de um lançamento deve aceitar uma chave de idempotência.

Uma nova requisição utilizando uma chave já processada e com os mesmos dados deve retornar o lançamento anteriormente criado, sem criar um novo registro.

A reutilização da mesma chave com dados diferentes deve ser rejeitada.

### RF10 — Controlar versões dos lançamentos

O sistema deve manter uma versão do lançamento.

A versão deve ser incrementada a cada alteração.

O consumidor deve utilizar a versão para evitar que eventos antigos ou repetidos sobrescrevam uma versão mais recente da projeção.

---

## 4. Requisitos não funcionais

### RNF01 — Disponibilidade

O serviço de Controle de Lançamentos não deve ficar indisponível caso o serviço de Consolidado Diário esteja indisponível.

A comunicação entre os serviços deve ser assíncrona para evitar uma dependência síncrona de disponibilidade.

### RNF02 — Desempenho

Em períodos de pico, o serviço de Consolidado Diário deve suportar 50 requisições por segundo.

A solução deve ser dimensionada para suportar esse volume sem degradação significativa do serviço.

### RNF03 — Perda de requisições

Em períodos de pico, o serviço de Consolidado Diário deve apresentar no máximo 5% de perda de requisições.

### RNF04 — Confiabilidade da integração

A indisponibilidade temporária de componentes envolvidos na comunicação assíncrona não deve resultar automaticamente na perda dos eventos de negócio.

A solução deve utilizar mecanismos de persistência e retry para permitir a recuperação do processamento.

### RNF05 — Consistência

O Controle de Lançamentos deve ser a fonte de verdade dos lançamentos.

O Consolidado Diário deve manter uma projeção própria, atualizada de forma assíncrona.

A consistência entre os serviços será eventual.

### RNF06 — Segurança

As APIs devem exigir autenticação.

As operações devem possuir autorização de acordo com as permissões necessárias para cada contexto.

A solução deve utilizar tokens de acesso e validar, no mínimo:

- emissor;
- audiência;
- validade;
- permissões do usuário.

### RNF07 — Observabilidade

A solução deve permitir acompanhar:

- requisições;
- latência;
- métricas de aplicação;
- logs;
- traces distribuídos;
- falhas de processamento.

A observabilidade deve contemplar tanto as APIs quanto o processamento assíncrono.

### RNF08 — Escalabilidade

Os serviços devem poder ser escalados de forma independente.

O Consolidado Diário deve permitir aumento de capacidade de processamento sem exigir alteração no serviço de Controle de Lançamentos.

### RNF09 — Resiliência

Falhas transitórias no processamento das mensagens devem possuir mecanismo de recuperação automática.

O consumidor deve utilizar retry para novas tentativas de processamento.

### RNF10 — Evolução independente

Os serviços devem possuir responsabilidades e modelos de persistência independentes.

O Consolidado Diário não deve acessar diretamente o banco de dados do Controle de Lançamentos.

### RNF11 — Testabilidade

As principais regras de negócio e mecanismos de integração devem possuir testes automatizados.

Devem ser contemplados cenários relacionados a:

- criação;
- alteração;
- exclusão;
- idempotência;
- versionamento;
- processamento dos eventos;
- consolidação diária.

### RNF12 — Operação local

A solução deve possuir instruções claras para execução local dos serviços e de suas dependências de infraestrutura.

---

## 5. Requisitos de integração

A integração entre os contextos será realizada de forma assíncrona.

O Controle de Lançamentos publica eventos relacionados às alterações dos lançamentos.

O Consolidado Diário consome esses eventos e mantém uma projeção própria.

A integração deve considerar:

- entrega assíncrona;
- retry;
- reprocessamento;
- versionamento dos eventos;
- idempotência do processamento;
- observabilidade.

---

## 6. Critérios de aceite

### Controle de Lançamentos

O serviço deve permitir criar, consultar, alterar e excluir lançamentos.

Uma operação de criação repetida com a mesma chave de idempotência não deve criar lançamentos duplicados.

Uma alteração deve gerar uma nova versão do lançamento.

Uma exclusão deve ser propagada para a projeção utilizada pelo consolidado.

### Consolidado Diário

O serviço deve permitir consultar e gerar o consolidado de uma determinada data.

O cálculo deve apresentar receitas, despesas, saldo e quantidades correspondentes aos lançamentos válidos.

Lançamentos excluídos não devem participar da consolidação.

### Disponibilidade

A indisponibilidade do Consolidado Diário não deve impedir a operação do Controle de Lançamentos.

### Desempenho

O serviço de Consolidado Diário deve ser capaz de processar o cenário de pico definido no desafio:

- 50 requisições por segundo;
- no máximo 5% de perda de requisições.

### Segurança

As APIs devem rejeitar requisições sem autenticação válida.

Operações protegidas devem exigir as permissões correspondentes.

### Observabilidade

Deve ser possível acompanhar o comportamento das APIs e do processamento assíncrono por meio de métricas, logs e traces.

---

## 7. Premissas

A solução considera que:

- o Controle de Lançamentos é a fonte de verdade dos lançamentos;
- o Consolidado Diário possui uma projeção própria;
- a comunicação entre os serviços é assíncrona;
- a consistência entre os contextos é eventual;
- falhas transitórias podem ocorrer na infraestrutura;
- o ambiente de execução pode ser escalado conforme a necessidade.

---

## 8. Relação com os requisitos do desafio

Os requisitos acima refinam os requisitos apresentados no documento-base do desafio, que determina a existência de um serviço de controle de lançamentos e de um serviço de consolidado diário.

Também detalham os requisitos não funcionais definidos no desafio, principalmente a independência de disponibilidade entre os serviços e a necessidade de suportar 50 requisições por segundo com no máximo 5% de perda.

Os requisitos refinados servem como base para o desenho da arquitetura alvo, para as decisões arquiteturais registradas nos ADRs e para os testes apresentados no projeto.