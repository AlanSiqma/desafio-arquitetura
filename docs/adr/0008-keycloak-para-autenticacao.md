# ADR-0008 — Keycloak para autenticação e autorização

## Status

Aceito

## Contexto

A solução possui dois serviços independentes que precisam controlar o acesso às suas APIs.

O `MicroService.ControleLancamentos` disponibiliza operações de leitura e escrita sobre os lançamentos.

O `MicroService.ConsolidadoDiario` disponibiliza operações de consulta e geração da consolidação diária.

É necessário garantir que somente clientes autenticados possam acessar as APIs e que cada operação seja protegida de acordo com suas permissões.

A autenticação e a autorização não devem ser implementadas diretamente dentro das regras de negócio dos serviços.

## Decisão

Adotar o Keycloak como provedor de identidade e autorização para os serviços.

A comunicação com as APIs será protegida utilizando tokens JWT emitidos pelo Keycloak.

Cada serviço possuirá seu próprio client no Keycloak:

- `controle-lancamentos`
- `consolidado-diario`

Serão utilizadas roles específicas para representar as permissões:

- `lancamentos.read`
- `lancamentos.write`
- `consolidado.read`
- `consolidado.write`

As APIs utilizarão políticas de autorização baseadas nessas roles.

No `ControleLancamentos`:

- operações de consulta exigem `lancamentos.read`;
- operações de criação, alteração e exclusão exigem `lancamentos.write`.

No `ConsolidadoDiario`:

- consulta exige `consolidado.read`;
- geração da consolidação exige `consolidado.write`.

## Motivações

### Centralização da identidade

A autenticação fica concentrada em um componente especializado, evitando que cada serviço implemente seu próprio mecanismo de gerenciamento de identidade.

### Separação entre autenticação e negócio

Os serviços validam o token e as permissões necessárias, enquanto as regras de negócio permanecem independentes do mecanismo de autenticação.

### Controle de acesso por serviço

As roles permitem definir permissões específicas para cada contexto de negócio.

### Uso de padrão aberto

A integração utiliza OpenID Connect e OAuth 2.0, permitindo que os serviços trabalhem com padrões amplamente utilizados para autenticação e autorização.

### Segurança na comunicação

Os serviços não precisam receber ou armazenar diretamente credenciais dos usuários para validar cada requisição. A API recebe o token e valida sua assinatura, emissor, audiência e validade.

## Alternativas consideradas

### Implementar autenticação dentro de cada microserviço

Descartada porque duplicaria responsabilidades de autenticação e gerenciamento de usuários em diferentes serviços.

### Utilizar autenticação baseada em API Key

Descartada porque oferece um modelo mais limitado para representar identidade, expiração de credenciais e permissões por usuário.

### Utilizar um serviço próprio de autenticação

Descartada devido ao custo adicional de desenvolvimento, manutenção e operação de uma solução própria de identidade.

## Consequências

### Positivas

- Autenticação centralizada.
- Autorização baseada em roles.
- Separação entre segurança e regras de negócio.
- Uso de padrões OAuth 2.0 e OpenID Connect.
- Possibilidade de adicionar novos serviços utilizando o mesmo provedor de identidade.
- Cada serviço pode possuir seu próprio client e suas próprias permissões.

### Negativas

- O Keycloak passa a ser um componente adicional da infraestrutura.
- É necessário configurar clients, roles e usuários.
- A indisponibilidade do Keycloak pode impactar a obtenção de novos tokens.
- É necessário gerenciar corretamente as configurações de segurança dos tokens.

## Implementação

A autenticação das APIs utiliza JWT Bearer.

Cada serviço configura:

- `Authority`;
- `MetadataAddress`;
- `Audience`;
- validação do emissor;
- validação da audiência;
- validação da validade do token.

As roles do Keycloak são utilizadas nas policies de autorização da aplicação.

No `ControleLancamentos`, as policies são:

- `LancamentosRead`;
- `LancamentosWrite`.

No `ConsolidadoDiario`, as policies são:

- `ConsolidadoRead`;
- `ConsolidadoWrite`.

As policies são aplicadas diretamente aos endpoints.

O `ControleLancamentos` também utiliza uma transformação de claims para disponibilizar as roles presentes no `realm_access.roles` do Keycloak como claims de role reconhecidas pelo mecanismo de autorização do ASP.NET Core.

Para o ambiente local, o fluxo de obtenção de token por usuário e senha pode ser utilizado para facilitar os testes via Postman. Esse fluxo é destinado ao ambiente de desenvolvimento e não representa a configuração recomendada para produção.

## Resultado esperado

Todas as APIs devem exigir autenticação.

Cada operação deve aceitar somente tokens que possuam as permissões necessárias para sua execução.

Um usuário autenticado sem a role necessária não deve conseguir executar a operação protegida.

A autenticação e autorização devem permanecer desacopladas das regras de negócio dos microserviços.