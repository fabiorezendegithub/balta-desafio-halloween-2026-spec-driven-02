# Como testar o encurtador de links com o Thunder Client

Este manual mostra, passo a passo, como subir a API e testar todos os endpoints
usando o **Thunder Client**, extensão do Visual Studio Code.

## 1. Pré-requisitos

- .NET SDK 10 instalado (`dotnet --version` deve mostrar `10.x`)
- Visual Studio Code com a extensão **Thunder Client** instalada
  (ícone de raio na barra lateral; se não aparecer, instale pelo Marketplace)

## 2. Subir a aplicação

No terminal, na raiz do projeto:

```bash
cd src/Encurtador.Api
dotnet run
```

Espere a mensagem indicando que a aplicação está escutando. A API fica disponível em:

```
http://localhost:5033
```

Na primeira execução, o banco SQLite (`encurtador.db`) é criado automaticamente
na pasta `src/Encurtador.Api` e as migrações são aplicadas. Não é preciso fazer nada manualmente.

> Para parar a aplicação, pressione `Ctrl+C` no terminal.

## 3. Criar a coleção de requisições

1. Abra o Thunder Client (ícone de raio na barra lateral do VS Code).
2. Em **Collections**, clique em **New Collection** e dê o nome `Encurtador de links`.
3. Dentro da coleção, crie uma requisição com **New Request** para cada passo deste manual.

Você pode usar a mesma coleção para todos os testes abaixo. Os passos são em ordem,
porque alguns dependem de links criados antes.

## 4. Passo 1 — Criar um link com código automático

- **Método:** `POST`
- **URL:** `http://localhost:5033/api/links`
- **Body:** aba **Body** → **JSON**:

```json
{
  "targetUrl": "https://www.google.com"
}
```

- **Resposta esperada:** status **201 Created** e um corpo parecido com:

```json
{
  "shortUrl": "http://localhost:5033/K7MQ2XD",
  "code": "K7MQ2XD"
}
```

Anote o `code` retornado. Os próximos passos usam o código como exemplo `K7MQ2XD`:
troque pelo código que você recebeu.

> O código automático tem 7 caracteres (letras maiúsculas A-Z e números 2 a 9).

## 5. Passo 2 — Criar um link com código próprio

- **Método:** `POST`
- **URL:** `http://localhost:5033/api/links`
- **Body (JSON):**

```json
{
  "targetUrl": "https://github.com",
  "code": "MEULINK"
}
```

- **Resposta esperada:** **201 Created**, com `"code": "MEULINK"`.

Regras do código próprio:

| Regra | Exemplo que falha (400) |
|---|---|
| Entre 4 e 20 caracteres | `ABC` |
| Só letras maiúsculas A-Z e números 1 a 9 | `ABC0` (zero), `abcd` (minúscula), `ABC D` (espaço), `ABC-D` (símbolo) |

## 6. Passo 3 — Testar código repetido (409)

Envie de novo o mesmo body do passo 2 (`"code": "MEULINK"`).

- **Resposta esperada:** **409 Conflict**, com um corpo `application/problem+json`
  contendo `"detail": "Este código já está em uso. Escolha outro código."`.

## 7. Passo 4 — Testar validação de URL (400)

- **Método:** `POST`, **URL:** `http://localhost:5033/api/links`
- **Body (JSON):**

```json
{
  "targetUrl": "ftp://exemplo.com/arquivo"
}
```

- **Resposta esperada:** **400 Bad Request** em `application/problem+json`.

Outros casos que retornam 400 com ProblemDetails:

- `targetUrl` sem `http://` ou `https://`, como `"www.google.com"`
- URL com mais de 2048 caracteres
- Corpo JSON malformado (por exemplo, uma vírgula sobrando ou uma chave sem fechar)

## 8. Passo 5 — Testar destino inacessível (400)

O sistema verifica, na criação, se o destino responde com sucesso (2xx) em até 5 segundos.

- **Método:** `POST`, **URL:** `http://localhost:5033/api/links`
- **Body (JSON):**

```json
{
  "targetUrl": "https://este-dominio-nao-existe-123456.com"
}
```

- **Resposta esperada:** **400 Bad Request**, com `detail` dizendo que o destino não está acessível publicamente.

## 9. Passo 6 — Criar um link com data de expiração

- **Método:** `POST`
- **URL:** `http://localhost:5033/api/links`
- **Body (JSON):** a data deve estar no formato `AAAA-MM-DD`, em UTC

```json
{
  "targetUrl": "https://www.wikipedia.org",
  "code": "VALIDO2A",
  "expiresOn": "2030-12-31"
}
```

- **Resposta esperada:** **201 Created**. O link vale até `23:59:59 UTC` do dia informado.

Casos de erro (400):

- Data no passado, como `"2000-01-01"`
- Formato errado, como `"31/12/2030"` ou `"2030-12-31T10:00:00Z"`

## 10. Passo 7 — Acessar o link curto (redirecionamento 302)

- **Método:** `GET`
- **URL:** `http://localhost:5033/MEULINK`
- **Resposta esperada:** **302 Found**, com o cabeçalho `Location: https://github.com`.

> No Thunder Client, o redirecionamento automático costuma vir desligado ou ligado conforme a
> versão. Se a resposta mostrar a página de destino em vez do 302, veja a seção de dicas no final.

Cada `GET` bem-sucedido soma 1 ao contador de acessos. Acesse esse link mais duas vezes
para ter 3 acessos ao final deste manual.

Teste também o código com letras minúsculas. Ele não existe, porque códigos diferenciam maiúsculas de minúsculas:

- **Método:** `GET`, **URL:** `http://localhost:5033/meulink`
- **Resposta esperada:** **404 Not Found**, com `detail` "O link não existe.".

## 11. Passo 8 — Consultar as estatísticas

- **Método:** `GET`
- **URL:** `http://localhost:5033/api/links/MEULINK/stats`
- **Resposta esperada:** **200 OK**:

```json
{
  "code": "MEULINK",
  "accessCount": 3
}
```

O endpoint é público: não exige autenticação nem cabeçalhos especiais.

Código inexistente (`GET /api/links/NAOEXISTE1/stats`) retorna **404**.

## 12. Passo 9 — Testar link expirado (410)

Este caso depende da data atual, então é mais fácil de testar com uma data que já
passou **depois** de criar o link. Como o sistema não aceita data no passado, use uma
data de **hoje**, que vale até `23:59:59 UTC`:

1. Descubra a data de hoje em UTC (no terminal: `date -u +%F` no Linux/macOS, ou
   `Get-Date -AsUTC -Format yyyy-MM-dd` no PowerShell).
2. Crie um link com `"expiresOn"` igual a essa data, por exemplo com `"code": "EXPIRA22"`.
3. Acesse `GET http://localhost:5033/EXPIRA22` e confirme o 302.
4. Espere a meia-noite UTC passar e acesse de novo.
5. **Resposta esperada:** **410 Gone**, com `detail` "O link expirou.", e nenhum redirecionamento.
6. Consulte `GET /api/links/EXPIRA22/stats`: o contador não deve ter subido no acesso expirado.

> Se não quiser esperar a meia-noite, pule este passo. Os testes automatizados do projeto
> cobrem esse caso com um relógio controlado.

## 13. Passo 10 — Confirmar que a data de expiração não pode ser alterada

A API não tem nenhuma rota para alterar um link existente. Teste:

- **Método:** `PUT` ou `PATCH`, **URL:** `http://localhost:5033/api/links/MEULINK`
- **Body (JSON):** `{ "expiresOn": "2040-01-01" }`
- **Resposta esperada:** **404** ou **405**, nunca 2xx.

## 14. Resumo dos resultados esperados

| # | Requisição | Resposta |
|---|---|---|
| 1 | `POST /api/links` com `targetUrl` válida | 201 |
| 2 | `POST /api/links` com `code` próprio válido | 201 |
| 3 | `POST /api/links` com `code` já usado | 409 |
| 4 | `POST /api/links` com URL `ftp://` | 400 |
| 5 | `POST /api/links` com destino inacessível | 400 |
| 6 | `POST /api/links` com `expiresOn` válida | 201 |
| 7 | `POST /api/links` com `expiresOn` no passado | 400 |
| 8 | `GET /{codigo}` existente | 302 |
| 9 | `GET /{codigo}` inexistente ou com minúsculas | 404 |
| 10 | `GET /{codigo}` expirado | 410 |
| 11 | `GET /api/links/{codigo}/stats` | 200 com `accessCount` |
| 12 | `PUT` ou `PATCH` em um link existente | 404 ou 405 |

## Dicas

- **Corpo de erro:** em respostas de erro, abra a aba **Response** do Thunder Client.
  O corpo `application/problem+json` traz `status`, `title` e `detail`, que explicam o motivo.
- **Redirecionamento:** se o Thunder Client seguir o 302 e mostrar o destino, procure a
  configuração de *follow redirects* nas configurações da extensão (ícone de engrenagem)
  e desligue-a. Assim você verá o 302 e o cabeçalho `Location`.
- **Banco limpo:** para começar do zero, pare a aplicação e apague `src/Encurtador.Api/encurtador.db`.
  Ele é recriado na próxima execução.
- **Porta diferente:** se a porta 5033 estiver ocupada, a API não sobe. Ajuste `applicationUrl` em
  `src/Encurtador.Api/Properties/launchSettings.json` e use a nova porta nas URLs.
