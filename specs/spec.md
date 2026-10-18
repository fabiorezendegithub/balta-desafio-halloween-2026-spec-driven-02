# Encurtador de links

## Problema
Links longos quebram em mensagens, e-mails e materiais impressos, e não dá para
medir quantas pessoas clicaram neles.

## Objetivo
Permitir que uma pessoa transforme uma URL longa em um link curto, compartilhe
esse link e acompanhe quantos acessos ele recebeu.

## Usuários
- Criador do link: gera links curtos e acompanha os acessos
- Visitante: clica no link curto e chega ao destino

## Histórias
- Como criador, quero encurtar uma URL para compartilhar um link menor
- Como criador, quero escolher o código do link para usar um nome legível
- Como criador, quero ver quantos acessos o link que criei recebeu
- Como visitante, quero ser redirecionado ao destino sem passar por tela intermediária

## Requisitos funcionais
- RF-01: o sistema recebe uma URL de destino e devolve um link curto
- RF-02: o código do link tem 7 caracteres gerados automaticamente, usando apenas
  letras maiúsculas de A a Z e números de 1 a 9
- RF-03: o criador pode informar um código próprio de 4 a 20 caracteres, usando
  apenas letras maiúsculas de A a Z e números de 1 a 9, sem espaços
- RF-04: acessar um link curto existente redireciona para a URL de destino
- RF-05: cada requisição GET a um código válido incrementa o contador daquele
  link, incluindo acessos repetidos e automatizados
- RF-06: o criador pode definir uma data de expiração para o link, informada
  apenas como data (sem hora); a data é interpretada em UTC e o link permanece
  válido até 23:59:59 UTC do dia informado, inclusive. Sem data de expiração
  informada, o link não expira. A data de expiração não pode ser alterada após
  a criação do link
- RF-07: o sistema devolve o total de acessos de um link a qualquer pessoa que
  conheça seu código
- RF-08: os links e seus dados devem ser persistidos em banco de dados para que
  não se percam após reinício da aplicação

## Regras de negócio
- RN-01: o código do link é único em todo o sistema
- RN-02: a URL de destino precisa usar http ou https e ser publicamente acessível.
  A acessibilidade é verificada somente na criação do link
- RN-03: link expirado não redireciona e não conta acesso
- RN-04: o contador de acessos nunca diminui
- RN-05: códigos automáticos e personalizados aceitam somente A-Z maiúsculo e
  os dígitos de 1 a 9; o dígito 0, minúsculas, espaços e caracteres especiais
  não são permitidos
- RN-06: se um código gerado automaticamente já estiver em uso, o sistema tenta
  gerar outro código
- RN-07: códigos são sensíveis a maiúsculas e minúsculas; um acesso com letras
  minúsculas não corresponde a nenhum código válido e responde que o link não
  existe

## Casos de borda
- Código já em uso: recusar a criação e explicar o motivo (HTTP 409)
- Código gerado automaticamente já em uso: gerar outro código e tentar
  novamente
- Código inexistente no acesso: responder que o link não existe (HTTP 404)
- Link expirado no acesso: responder que o link expirou (HTTP 410), sem
  redirecionar e sem contar acesso
- URL de destino com mais de 2048 caracteres: recusar (HTTP 400, contagem em
  caracteres)
- URL de destino não publicamente acessível: recusar (HTTP 400)
- Data de expiração no passado: recusar na criação (HTTP 400)
- Erros de validação sempre retornam HTTP 400 com ProblemDetails

## Fora de escopo
- Cadastro e login de usuários
- Domínio personalizado
- Relatório com origem geográfica do acesso
- Edição do destino de um link já criado
- Edição da data de expiração de um link já criado

## Critérios de aceite
- Criar um link e acessá-lo redireciona para o destino informado
- Dois links não podem ter o mesmo código
- Link com data de expiração vencida responde HTTP 410 e não incrementa o contador
- Um link ainda válido funciona até 23:59:59 UTC do último dia de sua data de expiração
- Qualquer pessoa que conheça o código pode consultar o total de acessos
- Após reiniciar a aplicação, os links persistidos no banco continuam funcionando

## Pendências
- Tempo limite da verificação de acessibilidade do destino na criação
- Critério exato de "publicamente acessível" (quais respostas HTTP são aceitas)
