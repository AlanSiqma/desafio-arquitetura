# Segurança e critérios de integração

## 1. Objetivo

Este documento apresenta os critérios de segurança adotados para proteger o acesso às APIs e a integração entre os componentes da solução.

A arquitetura considera autenticação, autorização, validação de tokens, isolamento entre os serviços e proteção das comunicações entre os componentes.

## 2. Princípios de segurança

A solução adota os seguintes princípios:

- autenticação centralizada;
- autorização baseada em permissões;
- menor privilégio;
- isolamento entre os contextos;
- validação de tokens;
- separação entre segurança e regras de negócio;
- não compartilhamento direto de bancos de dados;
- controle de acesso aos endpoints;
- utilização de credenciais específicas por componente.

## 3. Provedor de identidade

O Keycloak é utilizado como provedor de identidade e autorização.

Os serviços possuem clients independentes:

- controle-lancamentos;
- consolidado-diario.

O Keycloak é responsável pela emissão dos tokens de acesso utilizados pelas APIs.

Os serviços não precisam implementar gerenciamento próprio de usuários ou emissão de tokens.

## 4. Autenticação

As APIs utilizam autenticação baseada em JWT Bearer.

As requisições protegidas devem apresentar um token de acesso válido.

A validação considera:

- emissor do token;
- audiência do serviço;
- validade do token;
- permissões associadas ao usuário.

Um token emitido para um contexto diferente não deve ser considerado automaticamente válido para outro serviço.

Cada serviço valida sua própria audiência.

## 5. Autorização

A autorização é realizada por meio de políticas associadas às operações das APIs.

### Controle de Lançamentos

São utilizadas as seguintes permissões:

- lancamentos.read;
- lancamentos.write.

Operações de consulta exigem lancamentos.read.

Operações de criação, alteração e exclusão exigem lancamentos.write.

### Consolidado Diário

São utilizadas as seguintes permissões:

- consolidado.read;
- consolidado.write.

A consulta do consolidado exige consolidado.read.

A geração do consolidado exige consolidado.write.

## 6. Princípio do menor privilégio

Cada consumidor deve possuir somente as permissões necessárias para executar suas responsabilidades.

As permissões de leitura e escrita são separadas para evitar que um consumidor ou usuário tenha acesso superior ao necessário.

A separação também permite evoluir os níveis de acesso sem alterar a estrutura principal das APIs.

## 7. Segurança das APIs

Os endpoints devem ser protegidos por autenticação e autorização.

As políticas de autorização são aplicadas na camada de entrada das APIs.

As regras de negócio não devem depender diretamente do Keycloak.

Essa separação permite substituir ou evoluir o mecanismo de identidade sem alterar as principais regras do domínio.

## 8. Segurança na integração assíncrona

A comunicação entre Controle de Lançamentos e Consolidado Diário ocorre por meio do RabbitMQ.

O acesso ao RabbitMQ utiliza credenciais específicas para autenticação no broker.

As aplicações não devem utilizar credenciais administrativas para comunicação em produção.

O acesso às filas deve ser limitado aos componentes que efetivamente precisam publicar ou consumir as mensagens.

## 9. Isolamento dos dados

Cada serviço possui sua própria persistência.

O Controle de Lançamentos utiliza PostgreSQL.

O Consolidado Diário utiliza MongoDB.

O Consolidado Diário não acessa diretamente o PostgreSQL do Controle de Lançamentos.

O Controle de Lançamentos também não depende do MongoDB utilizado pelo Consolidado Diário.

Esse isolamento reduz o risco de acesso indevido aos dados e evita que um serviço contorne as regras de negócio do outro por meio do banco de dados.

## 10. Segurança das credenciais

Credenciais de infraestrutura não devem ser armazenadas diretamente no código-fonte.

Isso inclui:

- senhas do banco de dados;
- credenciais do RabbitMQ;
- credenciais administrativas do Keycloak;
- secrets utilizados pelos serviços.

Em ambiente produtivo, essas informações devem ser fornecidas por mecanismos apropriados de configuração e gerenciamento de secrets.

Os valores utilizados no ambiente local devem ser tratados como credenciais de desenvolvimento.

## 11. Comunicação HTTPS

As APIs devem utilizar HTTPS em ambientes produtivos para proteger os tokens e os dados transportados durante as requisições.

A configuração atual do ambiente local possui características específicas para facilitar a execução do desafio, incluindo configurações que permitem execução simplificada dos componentes em containers.

Essas configurações devem ser revisadas antes de uma implantação produtiva.

## 12. Configuração do Keycloak

O ambiente local utiliza um realm específico para o projeto:

desafio-microservices

Nesse realm são configurados os clients e as roles necessários para os serviços.

O fluxo utilizado para obtenção de token por usuário e senha é destinado aos testes locais realizados com Postman.

Esse mecanismo não deve ser considerado a estratégia recomendada para autenticação de usuários em um ambiente produtivo.

## 13. Segurança do consumidor

O Consumer.ConsolidadoDiario não expõe uma API de negócio para receber diretamente alterações de lançamentos.

Sua responsabilidade é consumir os eventos publicados no RabbitMQ.

O consumidor deve aceitar somente mensagens provenientes da infraestrutura de mensageria configurada para a solução.

O processamento também considera a versão do evento para evitar que mensagens antigas ou repetidas alterem indevidamente a projeção.

## 14. Idempotência e segurança operacional

A utilização de Idempotency-Key na criação dos lançamentos também contribui para a segurança operacional da API.

A mesma operação pode ser repetida sem resultar em múltiplos lançamentos.

A reutilização de uma chave com dados diferentes é identificada e rejeitada.

Isso reduz riscos associados a retries, duplicação de requisições e processamento concorrente.

## 15. Segurança dos eventos

Os eventos de integração devem conter somente as informações necessárias para que o consumidor execute sua responsabilidade.

Os eventos não devem carregar credenciais, tokens ou informações sensíveis que não sejam necessárias para o processamento.

Os contratos de eventos devem ser tratados como contratos de integração entre os contextos.

## 16. Auditoria e observabilidade

Os mecanismos de observabilidade devem permitir identificar falhas de autenticação, autorização e processamento das integrações.

Os logs devem evitar o registro de:

- tokens de acesso;
- senhas;
- secrets;
- credenciais de infraestrutura.

As informações registradas devem ser suficientes para investigação sem expor credenciais ou dados que não sejam necessários.

## 17. Ambiente local versus produção

O ambiente local possui configurações simplificadas para permitir a execução do desafio.

Entre essas configurações estão:

- credenciais padrão para componentes de infraestrutura;
- execução do Keycloak em modo de desenvolvimento;
- utilização de HTTP entre componentes locais;
- fluxo de usuário e senha para obtenção de tokens durante testes.

Essas configurações têm finalidade exclusivamente local e não devem ser replicadas sem avaliação em um ambiente produtivo.

Em produção, devem ser adotados:

- HTTPS;
- credenciais específicas por serviço;
- gerenciamento seguro de secrets;
- políticas de menor privilégio;
- configuração adequada de rede;
- controle de acesso à infraestrutura;
- rotação de credenciais;
- monitoramento de eventos de segurança.

## 18. Critérios para consumo de serviços

A integração entre componentes deve observar os seguintes critérios:

1. O consumidor deve possuir somente as permissões necessárias.
2. O acesso deve utilizar credenciais específicas do componente.
3. Os bancos de dados não devem ser compartilhados entre contextos.
4. Os eventos devem conter somente os dados necessários.
5. Tokens e credenciais não devem ser enviados como conteúdo de eventos.
6. Falhas de autenticação e autorização devem ser registradas para investigação.
7. Segredos não devem ser registrados nos logs.
8. A comunicação produtiva deve utilizar canais protegidos.
9. Os contratos de integração devem ser versionados quando necessário.
10. O processamento deve ser resiliente a mensagens repetidas.

## 19. Resultado esperado

A arquitetura deve garantir que somente usuários e componentes autorizados consigam acessar os recursos correspondentes às suas responsabilidades.

Os serviços devem permanecer isolados entre si, utilizando contratos de integração controlados e sem acesso direto às persistências de outros contextos.

As configurações simplificadas utilizadas no ambiente local devem ser substituídas ou endurecidas antes de qualquer implantação em produção.