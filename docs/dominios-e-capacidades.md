# Domínios funcionais e capacidades de negócio

## 1. Objetivo

Este documento apresenta o mapeamento dos domínios funcionais e das capacidades de negócio identificadas a partir do cenário proposto no desafio.

O objetivo é separar as responsabilidades de negócio de forma clara, permitindo definir limites de contexto, responsabilidades dos serviços e estratégias de integração.

O cenário apresenta a necessidade de um comerciante controlar seu fluxo de caixa diário por meio de lançamentos financeiros e disponibilizar um relatório com o saldo diário consolidado.

## 2. Domínio de negócio

O domínio de negócio considerado é o controle do fluxo de caixa diário.

Dentro desse domínio foram identificadas duas capacidades principais:

- Controle de Lançamentos
- Consolidação Diária

Essas capacidades possuem responsabilidades distintas e podem evoluir de maneira independente.

## 3. Capacidade: Controle de Lançamentos

### Objetivo

Gerenciar os lançamentos que compõem o fluxo de caixa do comerciante.

### Responsabilidades

A capacidade de Controle de Lançamentos é responsável por:

- registrar lançamentos;
- consultar lançamentos;
- alterar lançamentos;
- excluir lançamentos;
- controlar a versão dos lançamentos;
- garantir idempotência na criação;
- publicar eventos referentes às alterações dos lançamentos.

### Dados principais

Um lançamento possui:

- identificador;
- descrição;
- valor;
- data;
- tipo do lançamento;
- categoria;
- versão.

O tipo do lançamento pode ser:

- Receita
- Despesa

### Serviço responsável

MicroService.ControleLancamentos

### Fonte de verdade

O PostgreSQL é utilizado como persistência principal dos lançamentos.

O serviço de Controle de Lançamentos é a fonte de verdade para os dados dos lançamentos.

## 4. Capacidade: Consolidação Diária

### Objetivo

Disponibilizar uma visão consolidada dos lançamentos de determinado dia.

### Responsabilidades

A capacidade de Consolidação Diária é responsável por:

- receber as alterações dos lançamentos por meio de eventos;
- manter uma projeção própria dos lançamentos;
- gerar o consolidado de determinado dia;
- calcular o total de receitas;
- calcular o total de despesas;
- calcular o saldo;
- calcular a quantidade de receitas;
- calcular a quantidade de despesas;
- disponibilizar o consolidado para consulta.

### Dados principais

O consolidado diário possui:

- identificador;
- data;
- total de receitas;
- total de despesas;
- saldo;
- quantidade de receitas;
- quantidade de despesas;
- data de atualização.

### Serviço responsável

MicroService.ConsolidadoDiario

### Persistência

O MongoDB é utilizado para armazenar a projeção dos lançamentos e os consolidados diários.

O serviço possui seu próprio modelo de persistência e não acessa diretamente o PostgreSQL utilizado pelo Controle de Lançamentos.

## 5. Relação entre as capacidades

As capacidades possuem dependência funcional, mas não possuem dependência síncrona de disponibilidade.

O Controle de Lançamentos produz os eventos necessários para que a Consolidação Diária mantenha sua projeção atualizada.

A Consolidação Diária consome esses eventos de forma assíncrona.

Os principais eventos são:

- LancamentoCriadoEvent
- LancamentoAlteradoEvent
- LancamentoExcluidoEvent

Dessa forma, uma indisponibilidade temporária do serviço de Consolidação Diária não impede o funcionamento do Controle de Lançamentos.

## 6. Limites de responsabilidade

### Controle de Lançamentos

É responsável pelo estado oficial dos lançamentos.

Não é responsabilidade desse serviço:

- calcular ou armazenar o consolidado diário;
- acessar o MongoDB utilizado pelo Consolidado Diário;
- depender de uma resposta síncrona do Consolidado Diário para concluir uma operação de lançamento.

### Consolidação Diária

É responsável pela visão consolidada dos lançamentos.

Não é responsabilidade desse serviço:

- alterar diretamente os lançamentos no PostgreSQL;
- manter a fonte de verdade dos lançamentos;
- acessar diretamente o banco de dados do Controle de Lançamentos.

## 7. Capacidades e componentes

| Domínio funcional | Capacidade | Serviço | Persistência |
|---|---|---|---|
| Fluxo de Caixa | Controle de Lançamentos | MicroService.ControleLancamentos | PostgreSQL |
| Fluxo de Caixa | Consolidação Diária | MicroService.ConsolidadoDiario | MongoDB |

## 8. Eventos de negócio

A comunicação entre as capacidades ocorre por eventos.

### Lançamento criado

Evento:

LancamentoCriadoEvent

Indica que um novo lançamento foi registrado no Controle de Lançamentos.

O evento é utilizado pelo Consolidado Diário para criar a respectiva projeção.

### Lançamento alterado

Evento:

LancamentoAlteradoEvent

Indica que os dados de um lançamento foram alterados.

O evento contém a versão atualizada do lançamento para permitir que o consumidor identifique o estado mais recente.

### Lançamento excluído

Evento:

LancamentoExcluidoEvent

Indica que um lançamento foi excluído.

A projeção do Consolidado Diário utiliza essa informação para marcar o lançamento como excluído.

## 9. Características arquiteturais resultantes

A separação das capacidades resulta nos seguintes princípios:

- cada capacidade possui responsabilidade bem definida;
- cada serviço possui seu próprio modelo de persistência;
- o Controle de Lançamentos é a fonte de verdade dos lançamentos;
- o Consolidado Diário mantém uma projeção própria;
- a integração ocorre de forma assíncrona;
- os serviços podem ser escalados independentemente;
- a indisponibilidade do Consolidado Diário não deve impedir operações de lançamento;
- a consistência entre as projeções é eventual.

## 10. Relação com os requisitos do desafio

O documento-base determina a existência de um serviço responsável pelo controle de lançamentos e outro responsável pelo consolidado diário.

O mapeamento realizado transforma essas necessidades em duas capacidades de negócio claramente delimitadas.

Essa separação também fornece a base para a arquitetura de microserviços adotada na solução e para a definição das responsabilidades de cada serviço.

A arquitetura proposta considera ainda os requisitos não funcionais definidos no desafio, principalmente a independência de disponibilidade entre os serviços e a necessidade de suportar o volume de 50 requisições por segundo no Consolidado Diário.