# ADR-0002 — Separação dos Domínios

## Status

Aceito

## Contexto

O cenário possui duas necessidades de negócio principais:

- Controle dos lançamentos financeiros realizados pelo comerciante;
- Consolidação diária desses lançamentos para obtenção do saldo consolidado.

Embora exista relação entre essas necessidades, elas possuem responsabilidades, regras e características técnicas distintas.

O controle dos lançamentos é responsável pelo registro e manutenção dos eventos financeiros, enquanto a consolidação diária possui como responsabilidade processar esses eventos e disponibilizar uma visão consolidada por data.

Manter essas responsabilidades em um único serviço aumentaria o acoplamento entre os processos e faria com que alterações ou indisponibilidades na consolidação pudessem impactar diretamente o registro dos lançamentos.

## Decisão

Separar a solução em dois domínios funcionais principais:

### Domínio de Controle de Lançamentos

Responsável por:

- Criar lançamentos;
- Alterar lançamentos;
- Excluir lançamentos;
- Garantir idempotência das operações;
- Persistir os dados dos lançamentos;
- Publicar eventos relacionados às alterações dos lançamentos.

Esse domínio será implementado pelo serviço `MicroService.ControleLancamentos`.

### Domínio de Consolidação Diária

Responsável por:

- Consumir os eventos de lançamentos;
- Manter uma projeção dos dados necessários para consolidação;
- Gerar o consolidado diário;
- Disponibilizar o saldo e os totais por data.

Esse domínio será implementado pelo serviço `MicroService.ConsolidadoDiario`.

A comunicação entre os domínios será assíncrona, utilizando mensageria. O domínio de consolidação não será uma dependência síncrona do domínio de controle de lançamentos.

## Justificativa

A separação permite que cada domínio evolua de forma independente e que suas características técnicas sejam tratadas de acordo com sua responsabilidade.

O serviço de controle de lançamentos deve priorizar consistência das operações de escrita e disponibilidade para o registro dos lançamentos.

O serviço de consolidação, por sua vez, pode utilizar uma projeção específica para consultas e processamento das informações, permitindo otimizações independentes do modelo transacional de lançamentos.

A comunicação assíncrona também reduz o acoplamento entre os serviços. Dessa forma, uma indisponibilidade temporária do serviço de consolidação não impede que novos lançamentos sejam registrados.

Essa decisão também permite escalar os serviços de forma independente conforme a necessidade de cada domínio.

## Consequências

### Positivas

- Separação clara de responsabilidades;
- Menor acoplamento entre os domínios;
- Evolução independente dos serviços;
- Escalabilidade independente;
- Possibilidade de utilizar modelos de persistência adequados a cada responsabilidade;
- Maior resiliência diante da indisponibilidade de um dos serviços;
- Facilidade para evoluir a consolidação para uma arquitetura orientada a projeções.

### Negativas

- A solução passa a possuir maior complexidade operacional;
- É necessário utilizar mensageria e mecanismos de processamento assíncrono;
- Existe consistência eventual entre o registro do lançamento e sua representação na consolidação;
- Monitoramento, tratamento de falhas e reprocessamento passam a ser responsabilidades necessárias da arquitetura.

## Alternativas consideradas

### Serviço único

Manter controle de lançamentos e consolidação dentro de uma única aplicação.

Foi descartado por aumentar o acoplamento entre responsabilidades distintas e dificultar a evolução e a escala independente dos processos.

### Serviços separados com comunicação síncrona

Separar os serviços, mas realizar chamadas HTTP síncronas entre controle de lançamentos e consolidação.

Foi descartado porque criaria uma dependência de disponibilidade entre os serviços. Uma indisponibilidade da consolidação poderia impactar o fluxo de lançamento.

### Separação por domínio com comunicação assíncrona

Separar os serviços e utilizar eventos para integração entre os domínios.

Foi escolhida por proporcionar menor acoplamento, independência de disponibilidade e possibilidade de evolução e escala independentes.

## Resultado esperado

A arquitetura passa a possuir dois contextos funcionais bem definidos:

`Controle de Lançamentos` → registra e mantém os lançamentos.

`Consolidação Diária` → processa os eventos e mantém a visão consolidada.

A separação estabelece uma fronteira clara entre as responsabilidades de negócio e cria uma base para evolução independente dos dois domínios.