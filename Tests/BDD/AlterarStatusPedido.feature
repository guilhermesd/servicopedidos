Feature: Alteração de Status de Pedido

  Scenario: Alterar o status de um pedido existente para EmPreparacao
    Given que existe um pedido com status "Recebido"
    When eu altero o status do pedido para "EmPreparacao"
    Then o pedido deve ser atualizado com sucesso
