# Plano técnico

## Contexto
Implementa a especificação do encurtador de links, respeitando a constituição
do projeto.

## Arquitetura
- API HTTP em Minimal API, um arquivo de endpoints por recurso
- Camada de aplicação com LinkService, dona das regras RN-01 a RN-04
- Camada de dados com EF Core e um repositório por agregado
- Endpoint só valida entrada, chama o serviço e traduz o resultado em resposta

## Decisões
- D-01: SQLite como armazenamento, porque o RF-08 exige persistência
  entre reinícios
- D-02: contador incrementado por UPDATE atômico no banco, e não lido e
  reescrito na aplicação, para atender a RN-04 sob acesso concorrente
- D-03: código do link gerado com alfabeto sem caracteres ambíguos, para
  reduzir erro de digitação em link lido em voz alta
- D-04: unicidade do código garantida por índice único no banco, além da
  checagem no serviço, porque a checagem sozinha tem corrida
- D-05: alfabeto do código automático (RF-02, D-03) = A-Z sem I, L e O, mais
  2 a 9 (31 caracteres). É um subconjunto de "A-Z e 1-9", então cumpre RF-02, e
  remove caracteres confundíveis (I/1/l, O/0). O alfabeto do código próprio
  continua sendo A-Z e 1-9 inteiro (RN-05)
- D-06: critério de "publicamente acessível" (RN-02, pendência da spec): uma
  requisição GET ao destino, seguindo redirecionamentos, é aceita somente se a
  resposta final for 2xx. Qualquer outro status, erro de rede ou timeout recusa
  o link (HTTP 400)
- D-07: tempo limite da verificação de acessibilidade = 5 segundos (pendência
  da spec)

## Dependências
Cada pacote novo precisa de justificativa (constituição, Arquitetura):
- Microsoft.EntityFrameworkCore.Sqlite: provedor do EF Core para SQLite (D-01)
- Microsoft.EntityFrameworkCore.Design: geração de migrações pelo `dotnet ef`.
  Só é necessário em tempo de desenvolvimento
- dotnet-ef (ferramenta global): executa as migrações; versão alinhada ao EF Core
- Microsoft.AspNetCore.Mvc.Testing: sobe a API em memória nos testes de
  integração, exigido pela constituição (todo endpoint tem teste de integração)
- xUnit: framework de testes definido na constituição (Stack)

## Modelo de dados
Link
- Id, Code (único), TargetUrl, CreatedAtUtc, ExpiresAtUtc (nulo; guarda 23:59:59 UTC do dia informado), AccessCount

## Contratos
- POST /api/links entrada: targetUrl, code opcional, expiresOn opcional (data no formato AAAA-MM-DD, em UTC; o link vale até 23:59:59 UTC desse dia)
- POST /api/links saída: shortUrl, code
- GET /{code} redireciona com 302, ou 404 se não existe, ou 410 se expirou
- GET /api/links/{code}/stats devolve o total de acessos a qualquer pessoa que conheça o código, sem autenticação

## Riscos
- Concorrência na criação de código próprio, mitigada por D-04
- Crescimento da tabela de links sem política de limpeza, fora de escopo por ora