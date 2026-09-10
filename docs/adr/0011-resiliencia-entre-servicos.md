# ADR-0011 — Estratégia de resiliência e retry no processamento de mensagens

## Status

Aceito

## Contexto

A comunicação entre o MicroService.ControleLancamentos e o MicroService.ConsolidadoDiario ocorre de forma assíncrona por meio do RabbitMQ.

O processamento das mensagens pode sofrer falhas temporárias, como indisponibilidade momentânea do MongoDB, problemas de conectividade ou falhas transitórias durante o processamento do evento.

Uma falha temporária não deve necessariamente resultar na perda da mensagem ou exigir intervenção manual.

Além disso, a arquitetura precisa manter o MicroService.ControleLancamentos independente da disponibilidade do serviço de consolidação.

## Decisão

Adotar uma estratégia de retry no consumidor das mensagens utilizando os recursos de mensageria configurados com MassTransit.

Os endpoints responsáveis pelo processamento dos eventos utilizarão três tentativas de processamento, com intervalo de cinco segundos entre as tentativas.

Os eventos processados pelo consumidor são:

- LancamentoCriadoEvent
- LancamentoAlteradoEvent
- LancamentoExcluidoEvent

O retry será aplicado individualmente aos endpoints de recebimento:

- lancamento-criado
- lancamento-alterado
- lancamento-excluido

Caso o processamento continue falhando após as tentativas configuradas, a mensagem seguirá o comportamento de falha definido pela infraestrutura de mensageria.

## Motivações

### Tratamento de falhas transitórias

Problemas temporários de infraestrutura não devem causar uma falha definitiva no processamento de uma mensagem.

### Confiabilidade do processamento

As tentativas adicionais aumentam a possibilidade de processamento bem-sucedido quando a falha possui caráter temporário.

### Independência entre os serviços

O produtor não precisa aguardar o processamento do consumidor para concluir sua operação.

### Simplicidade operacional

Uma política de retry limitada e com intervalo fixo atende ao cenário atual sem introduzir uma estratégia excessivamente complexa.

## Alternativas consideradas

### Não utilizar retry

Descartada porque qualquer falha transitória poderia interromper o processamento imediatamente.

### Retry implementado manualmente dentro dos consumidores

Descartada porque adicionaria lógica de infraestrutura aos consumidores e duplicaria mecanismos que já são oferecidos pela plataforma de mensageria.

### Retry com número elevado de tentativas

Descartada porque poderia manter mensagens problemáticas em processamento por períodos excessivos e dificultar a identificação de falhas persistentes.

### Retry com backoff mais complexo

Considerado como possibilidade futura caso o comportamento da infraestrutura demonstre necessidade de intervalos progressivos entre as tentativas.

## Consequências

### Positivas

- Permite recuperação automática de falhas transitórias.
- Reduz a necessidade de intervenção manual para problemas temporários.
- Mantém o processamento dos eventos desacoplado das requisições do serviço produtor.
- Centraliza a política de retry na configuração dos endpoints de mensageria.
- Funciona em conjunto com a estratégia de Transactional Outbox.

### Negativas

- Uma mensagem pode ser processada mais de uma vez.
- O processamento precisa ser tolerante a reprocessamento.
- Falhas persistentes continuam exigindo tratamento adicional.
- O tempo total de processamento pode aumentar quando ocorrem falhas.

## Implementação

Os endpoints de recebimento do consumidor utilizam uma política de retry com três tentativas e intervalo de cinco segundos.

A configuração é aplicada aos endpoints de criação, alteração e exclusão de lançamentos.

O consumidor também utiliza a propriedade Versao dos eventos para impedir que mensagens antigas ou repetidas sobrescrevam uma versão mais recente da projeção no MongoDB.

Dessa forma, o retry é combinado com o controle de versão para tornar o processamento mais resiliente a falhas e reprocessamentos.

## Resultado esperado

Falhas transitórias durante o processamento dos eventos devem ser tratadas automaticamente por meio das tentativas configuradas.

O processamento de mensagens repetidas ou fora de ordem não deve comprometer a consistência da projeção mantida no MongoDB.

O serviço de Controle de Lançamentos deve continuar independente da disponibilidade momentânea do consumidor.