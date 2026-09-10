# ADR-0010 — Versionamento dos eventos de integração

## Status

Aceito

## Contexto

A comunicação entre os serviços ocorre de forma assíncrona por meio de eventos publicados pelo serviço de Controle de Lançamentos e consumidos pelo serviço de Consolidado Diário.

Os lançamentos podem ser criados, alterados e excluídos. Como o processamento das mensagens ocorre de forma assíncrona, eventos podem ser processados posteriormente ou fora da ordem esperada.

Uma alteração de um lançamento pode gerar uma nova versão do registro. Sem uma informação de versão, uma mensagem antiga poderia sobrescrever uma informação mais recente na projeção mantida pelo Consolidado Diário.

## Decisão

Adotar versionamento dos lançamentos nos eventos de integração.

A entidade Lancamento possui o campo Versao, iniciado em 1 na criação.

A cada alteração do lançamento, a versão é incrementada.

Os eventos de integração carregam a versão correspondente do lançamento:

- LancamentoCriadoEvent
- LancamentoAlteradoEvent
- LancamentoExcluidoEvent

O consumidor utiliza a versão recebida para determinar se o evento deve atualizar a projeção existente no MongoDB.

Eventos com versão igual ou inferior à versão já armazenada são ignorados.

Eventos com versão superior podem atualizar a projeção.

## Motivações

### Proteção contra eventos fora de ordem

Como a comunicação é assíncrona, não deve ser assumido que os eventos serão processados exatamente na ordem em que foram gerados.

A versão permite identificar qual estado é mais recente.

### Proteção contra reprocessamento

Uma mesma mensagem pode ser processada novamente.

A comparação da versão evita que um processamento repetido altere indevidamente uma projeção que já esteja atualizada.

### Consistência da projeção

A projeção no MongoDB deve representar a versão mais recente conhecida do lançamento.

### Evolução da arquitetura

O versionamento cria uma base para futuras evoluções dos contratos de eventos e para cenários em que diferentes consumidores possam processar mensagens em ritmos distintos.

## Alternativas consideradas

### Confiar somente na ordem das mensagens

Descartada porque a arquitetura distribuída não deve depender exclusivamente da ordem de processamento.

### Utilizar somente o timestamp do evento

Descartada porque timestamps podem não ser suficientes para representar de forma determinística a sequência das alterações.

### Não versionar os eventos

Descartada porque um evento antigo poderia sobrescrever uma versão mais recente da projeção.

## Consequências

### Positivas

- Evita que eventos antigos sobrescrevam informações mais recentes.
- Permite reprocessamento seguro de eventos.
- Torna o consumidor mais resiliente a processamento fora de ordem.
- Mantém a projeção alinhada com a versão mais recente do lançamento.
- Facilita futuras evoluções relacionadas ao histórico das alterações.

### Negativas

- A entidade de domínio precisa manter uma informação adicional de versão.
- Os eventos precisam transportar a versão do lançamento.
- O consumidor precisa implementar a lógica de comparação de versões.
- A estratégia de versionamento precisa ser mantida de forma consistente pelos produtores.

## Implementação

A entidade Lancamento possui a propriedade Versao.

O valor inicial é 1.

Durante uma atualização, a versão é incrementada antes da publicação do evento correspondente.

Os eventos LancamentoCriadoEvent e LancamentoAlteradoEvent possuem a propriedade Versao.

O LancamentoExcluidoEvent também possui a propriedade Versao, permitindo que o consumidor determine se a exclusão representa um estado mais recente.

No MongoDB, a projeção LancamentoDocument mantém a versão atualmente processada.

O consumidor segue a seguinte regra:

- evento com versão menor que a versão armazenada: ignorar;
- evento com versão igual à versão armazenada: ignorar;
- evento com versão maior que a versão armazenada: processar.

No caso de exclusão, a projeção não é removida fisicamente do MongoDB. O documento é marcado como excluído por meio do campo Excluido, mantendo a versão correspondente.

## Resultado esperado

A projeção mantida pelo Consolidado Diário deve sempre preservar a versão mais recente processada de cada lançamento.

Eventos atrasados, duplicados ou reprocessados não devem fazer com que uma versão antiga sobrescreva uma versão mais recente.