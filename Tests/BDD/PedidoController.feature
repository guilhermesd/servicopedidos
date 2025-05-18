# language: pt-BR
Funcionalidade: Gerenciamento de Pedidos
  Como cliente da API de Pedidos
  Quero criar, atualizar status e consultar pedidos
  Para gerenciar pedidos corretamente

  Cenário: Gerar um pedido com sucesso
    Dado que o DTO para gerar pedido está disponível
    E o token de autorização "Bearer token123" está presente no header
    Quando eu enviar a requisição para gerar pedido
    Então a resposta deve ser Created com o pedido gerado

  Cenário: Atualizar status do pedido com sucesso
    Dado que o id do pedido e o DTO de alteração de status estão disponíveis
    Quando eu enviar a requisição para atualizar status do pedido
    Então a resposta deve ser NoContent

  Cenário: Atualizar status do pagamento com sucesso
    Dado que o id do pedido e o DTO de alteração de status de pagamento estão disponíveis
    Quando eu enviar a requisição para atualizar status do pagamento
    Então a resposta deve ser NoContent

  Cenário: Obter pedidos com filtro
    Dado que o filtro de pedidos está disponível
    Quando eu enviar a requisição para obter pedidos
    Então a resposta deve ser Ok com a lista de pedidos
