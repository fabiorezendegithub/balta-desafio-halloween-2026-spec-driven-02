# Tarefas

| ID | Tarefa | Origem | Depende de | Concluída quando |
|---|---|---|---|---|
| T-01 | Criar projeto e estrutura de pastas | Constituição | | Projeto compila |
| T-02 | Modelar a entidade Link e a migração | RF-01, RF-08 | T-01 | Migração aplica no banco |
| T-03 | Gerar código aleatório de 7 caracteres | RF-02, D-03 | T-01 | Teste unitário cobre formato e alfabeto |
| T-04 | Validar URL de destino | RN-02, borda 2048 | T-01 | Teste cobre http, https e inválidos |
| T-05 | Criar link com código automático | RF-01 | T-02, T-03, T-04 | Teste de integração cria e retorna 201 |
| T-06 | Aceitar código próprio com unicidade | RF-03, RN-01, D-04 | T-05 | Código duplicado retorna 409 |
| T-07 | Redirecionar por código | RF-04 | T-05 | Teste de integração retorna 302 |
| T-08 | Incrementar contador de forma atômica | RF-05, RN-04, D-02 | T-07 | Teste concorrente não perde contagem |
| T-09 | Endpoint de estatísticas público por código | RF-07 | T-05 | Consulta por código existente, sem header, retorna o total de acessos |
| T-11 | Validar código próprio: 4 a 20 caracteres, A-Z e 1-9, sem espaços | RF-03, RN-05 | T-06 | Teste unitário cobre limites (3, 4, 20, 21 caracteres) e caracteres proibidos (0, minúsculas, espaço, especial) com 400 em ProblemDetails |
| T-12 | Gerar outro código quando o automático colidir com um existente | RN-06, RF-02 | T-03, T-05 | Teste unitário simula colisão e confirma que um novo código é gerado e usado |
| T-13 | Comparar códigos sensível a maiúsculas e minúsculas | RN-07 | T-07 | Teste de integração: código com letras minúsculas retorna 404 |
| T-14 | Responder 404 quando o código não existe no redirecionamento | Casos de borda (404) | T-07 | Teste de integração retorna 404 com mensagem de que o link não existe |
| T-15 | Devolver erros de validação como ProblemDetails com HTTP 400 | Constituição (Qualidade), Casos de borda | T-01 | Teste de integração com entrada inválida retorna 400 com corpo ProblemDetails |
| T-16 | Verificar que o destino é publicamente acessível na criação, com timeout | RN-02, Casos de borda (destino inacessível) | T-05; pendências da spec | Teste cobre destino acessível (201) e inacessível (400); timeout definido |
| T-17 | Aceitar data de expiração informada só como data, válida até 23:59:59 UTC | RF-06, Critérios de aceite | T-05 | Teste unitário cobre o último instante válido e a ausência de data (link não expira) |
| T-18 | Recusar data de expiração no passado | RF-06, Casos de borda (data passada) | T-17 | Teste de integração retorna 400 em ProblemDetails |
| T-19 | Responder 410 para link expirado sem redirecionar e sem contar acesso | RF-06, RN-03, Casos de borda (410) | T-07, T-17 | Teste de integração retorna 410 e o contador permanece igual |
| T-20 | Impedir alteração da data de expiração após a criação | RF-06, Fora de escopo | T-17 | Teste confirma que nenhuma rota aceita nova data para link existente |
| T-22 | Teste de caminho feliz para criação com código próprio e para estatísticas | Constituição (Qualidade) | T-06, T-09 | Testes de integração retornam 201 e o total de acessos correto |