<img width="100%" alt="Halloween 2026" src="https://baltaio.blob.core.windows.net/static/images/v4/challenges/halloween-2026/banner.jpg" />

## 🎃 Halloween - Desafio 2

Oi, eu sou o Fabio Rezende e este é o espaço onde compartilho minha jornada de aprendizado durante o desafio **Halloween 2026**, realizado pelo [balta.io](https://balta.io). 👻

Aqui você vai encontrar projetos, exercícios e códigos que estou desenvolvendo durante o desafio.

### Sobre este desafio
Neste desafio o objetivo é criar um encurtador de URLs completo utilizando os conceitos de Spec-Driven Development

#### O que foi implementado
- Criação de link curto a partir de uma URL de destino, com código automático de 7 caracteres ou código próprio de 4 a 20 caracteres
- Verificação, na criação, de que o destino responde com sucesso (timeout de 5 segundos)
- Redirecionamento 302 pelo código curto, diferenciando maiúsculas de minúsculas
- Data de expiração opcional (`AAAA-MM-DD`), válida até 23:59:59 UTC; links expirados respondem 410 e não contam acesso
- Contador de acessos com incremento atômico no banco, sem perda sob acessos concorrentes
- Endpoint público de estatísticas por código
- Persistência em SQLite com migrações do EF Core
- Erros de validação sempre em ProblemDetails com HTTP 400
- Suíte de testes com xUnit: unitários para as regras de negócio e integração para os endpoints

#### Como rodar

Pré-requisito: .NET SDK 10.

```bash
cd src/Encurtador.Api
dotnet run
```

A API sobe em `http://localhost:5033`. O banco `encurtador.db` é criado automaticamente na primeira execução.

#### Endpoints

| Método | Rota | Descrição |
|---|---|---|
| `POST` | `/api/links` | Cria um link (`targetUrl`, `code` opcional, `expiresOn` opcional) |
| `GET` | `/{code}` | Redireciona (302) para o destino; 404 se não existe; 410 se expirou |
| `GET` | `/api/links/{code}/stats` | Retorna o total de acessos do link |

#### Testes

```bash
dotnet test
```

Para testar manualmente com o Thunder Client (extensão do VS Code), siga o passo a passo em [docs/como-testar.md](docs/como-testar.md).

#### Estrutura do projeto

```
specs/                   Constituição, especificação, plano e tarefas (Spec-Driven Development)
docs/                    Manual de testes manuais
src/Encurtador.Api/      API em Minimal API: endpoints, serviços, EF Core e migrações
tests/Encurtador.Tests/  Testes unitários e de integração (xUnit)
```

#### Documentação de referência
- [Constituição](specs/constitution.md): stack, arquitetura e regras de qualidade
- [Especificação](specs/spec.md): problema, requisitos funcionais, regras de negócio e casos de borda
- [Plano técnico](specs/plan.md): decisões técnicas (D-01 a D-07) e dependências
- [Tarefas](specs/tasks.md): lista de tarefas que originou a implementação

Neste processo eu aprendi:
* ✅

## Bagde
<img src="https://baltaio.blob.core.windows.net/static/images/v4/challenges/halloween-2026/02.png" width="200" />

## Problema
Links longos quebram em mensagens, e-mails e materiais impressos, e não dá para medir quantos cliques eles recebem. O encurtador transforma uma URL longa em um link curto, que pode ser compartilhado e acompanhado pelo número de acessos.

## Sobre o Halloween 2026
O desafio **Halloween 2026** consiste em implementar implementar o modelo Spec Driven Development de ponta a ponta, criando apps completas com IA.

### Veja meu progresso no desafio
[Incluir link para o repositório central]
