# Observabilidade

## 1. Objetivo

Este documento apresenta a estratégia de observabilidade adotada na solução, contemplando logs, métricas e traces distribuídos.

O objetivo é permitir acompanhar o comportamento dos serviços, identificar falhas, analisar desempenho e facilitar a investigação de problemas em ambiente local e produtivo.

## 2. Estratégia

A solução utiliza OpenTelemetry como padrão de instrumentação.

Os sinais de observabilidade são:

- logs;
- métricas;
- traces distribuídos.

Os sinais são enviados para um OpenTelemetry Collector, que realiza a distribuição para as ferramentas especializadas.

## 3. Componentes

A infraestrutura de observabilidade possui os seguintes componentes:

- OpenTelemetry Collector;
- Prometheus;
- Grafana;
- Loki;
- Jaeger.

O OpenTelemetry Collector funciona como ponto central de recebimento e encaminhamento dos sinais.

O Prometheus é utilizado para armazenamento e consulta das métricas.

O Grafana fornece os dashboards para visualização das métricas e dos logs.

O Loki é utilizado para armazenamento e consulta dos logs.

O Jaeger é utilizado para visualização dos traces distribuídos.

## 4. Instrumentação dos serviços

Os serviços ASP.NET Core utilizam instrumentação OpenTelemetry para capturar informações relevantes da execução.

São observados principalmente:

- requisições HTTP;
- duração das requisições;
- status das requisições;
- métricas do runtime;
- chamadas HTTP realizadas pela aplicação;
- traces relacionados ao processamento das requisições.

A instrumentação é configurada fora da lógica de negócio, mantendo a responsabilidade de observabilidade separada das regras da aplicação.

## 5. Traces distribuídos

Os traces permitem acompanhar uma operação através dos diferentes componentes envolvidos no processamento.

No fluxo de criação ou alteração de um lançamento, por exemplo, é possível observar o processamento realizado pelo Controle de Lançamentos e o processamento posterior realizado pelo Consumer.

A mensageria assíncrona permite correlacionar o processamento da operação original com o processamento posterior do evento.

Essa capacidade é importante para identificar onde uma operação apresentou lentidão ou falha.

## 6. Métricas

As métricas são coletadas pelo OpenTelemetry Collector e disponibilizadas ao Prometheus.

Entre as métricas observadas estão:

- quantidade de requisições HTTP;
- taxa de requisições;
- duração das requisições;
- percentis de latência;
- métricas do runtime do .NET;
- métricas dos processos e componentes instrumentados.

As métricas podem ser utilizadas para acompanhar tendências e identificar degradação de desempenho.

## 7. Dashboards

O Grafana é utilizado como camada de visualização.

Foram configuradas consultas para acompanhar a taxa de requisições por serviço e o percentil 95 de duração das requisições HTTP.

Exemplo de indicador utilizado:

- taxa de requisições por serviço;
- latência P95 por serviço.

A separação por serviço permite identificar individualmente o comportamento do Controle de Lançamentos e do Consolidado Diário.

## 8. Logs

Os logs das aplicações são encaminhados ao OpenTelemetry Collector e posteriormente ao Loki.

O Grafana permite consultar os logs centralizados.

A centralização facilita a investigação de problemas sem necessidade de acessar individualmente os containers das aplicações.

Os logs devem fornecer contexto suficiente para diagnosticar falhas, incluindo informações como:

- operação executada;
- identificador do lançamento quando aplicável;
- processamento de eventos;
- ocorrência de erros;
- tentativas de processamento;
- informações de infraestrutura relevantes.

Credenciais, tokens e secrets não devem ser registrados.

## 9. Correlação entre sinais

A utilização conjunta de logs, métricas e traces permite investigar um mesmo comportamento por diferentes perspectivas.

Um aumento de latência identificado nas métricas pode ser investigado por meio dos traces.

Um erro identificado em um trace pode ser relacionado aos logs correspondentes ao processamento.

Essa abordagem reduz o tempo necessário para localizar a origem de problemas.

## 10. Observabilidade da mensageria

O processamento assíncrono é uma parte importante da arquitetura.

Os consumidores possuem logs relacionados ao recebimento e processamento dos eventos.

A estratégia de retry permite identificar situações em que uma mensagem precisou ser processada novamente.

A observabilidade da mensageria deve permitir identificar:

- falhas no processamento;
- retries;
- mensagens que não foram processadas com sucesso;
- comportamento anormal dos consumidores;
- indisponibilidade de dependências.

## 11. Resiliência e observabilidade

A observabilidade também é utilizada para validar os requisitos de resiliência.

Como o Controle de Lançamentos não possui dependência síncrona do Consolidado Diário, a indisponibilidade do serviço de consolidação não impede a criação de novos lançamentos.

Essa situação pode ser observada por meio dos logs, métricas e do estado do processamento assíncrono.

## 12. Monitoramento de desempenho

O requisito de desempenho do Consolidado Diário foi validado por meio de teste de carga utilizando k6.

O cenário executado utilizou uma taxa constante de 50 requisições por segundo durante 60 segundos.

O resultado final apresentou:

- 3.000 requisições;
- aproximadamente 50 requisições por segundo;
- 0% de falhas HTTP;
- 100% dos checks com sucesso;
- latência média de aproximadamente 4,81 ms;
- P90 de aproximadamente 5,91 ms;
- P95 de aproximadamente 6,54 ms;
- latência máxima de aproximadamente 19,31 ms.

O resultado demonstra o comportamento observado no ambiente utilizado para a validação do desafio.

## 13. Health checks

Os serviços possuem endpoints de health check para indicar a disponibilidade básica da aplicação e de suas dependências relevantes.

O health check pode ser utilizado por mecanismos de infraestrutura para identificar quando uma instância não está apta a receber tráfego.

No ambiente atual, o Controle de Lançamentos verifica sua infraestrutura de persistência e o Consolidado Diário verifica a disponibilidade do MongoDB.

## 14. Ambiente local

A infraestrutura de observabilidade pode ser executada juntamente com os demais componentes utilizando o Docker Compose.

Os componentes de observabilidade são mantidos em uma configuração separada da infraestrutura principal, permitindo iniciar a aplicação sem necessariamente executar toda a stack de observabilidade.

## 15. Evolução para produção

Em um ambiente produtivo, a estratégia pode evoluir para incluir:

- alertas baseados em métricas;
- alertas de erro e latência;
- monitoramento da disponibilidade dos serviços;
- monitoramento das filas;
- monitoramento de retries;
- retenção adequada de logs;
- retenção adequada de traces;
- controle de custo de armazenamento;
- dashboards específicos para operação;
- indicadores de negócio.

Também deve ser definido um período de retenção adequado para cada tipo de sinal, considerando requisitos operacionais, segurança e custo.

## 16. Indicadores operacionais

Os principais indicadores recomendados para acompanhamento da solução são:

- disponibilidade dos serviços;
- taxa de erros HTTP;
- latência P95;
- volume de requisições;
- utilização de CPU e memória;
- quantidade de mensagens processadas;
- quantidade de retries;
- tempo de processamento dos consumidores;
- disponibilidade das dependências;
- comportamento das filas.

## 17. Resultado esperado

A estratégia de observabilidade permite acompanhar a solução de forma centralizada e fornece mecanismos para:

- detectar problemas;
- diagnosticar falhas;
- acompanhar desempenho;
- investigar processamento assíncrono;
- validar requisitos não funcionais;
- apoiar operações futuras em produção.

A adoção de OpenTelemetry reduz o acoplamento da aplicação às ferramentas de observabilidade, permitindo alterar os backends de armazenamento e visualização sem modificar significativamente a instrumentação dos serviços.