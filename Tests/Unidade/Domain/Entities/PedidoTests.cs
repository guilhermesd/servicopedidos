using Domain.Entities;
using FluentAssertions;

namespace Tests.Unidade.Domain.Entities
{
    public class PedidoTests
    {
        [Fact]
        public void CriarPedido_DeveInicializarComStatusRecebido()
        {
            // Arrange
            var cpfCliente = "12345678900";

            // Act
            var pedido = new Pedido(cpfCliente);

            // Assert
            pedido.Should().NotBeNull();
            pedido.Status.Should().Be(StatusPedido.Recebido);
            pedido.CpfCliente.Should().Be(cpfCliente);
            pedido.Itens.Should().BeEmpty();
        }

        [Fact]
        public void AdicionarItem_DeveAdicionarItemCorretamente()
        {
            // Arrange
            var pedido = new Pedido("12345678900");
            var idProduto = 1;
            int quantidade = 2;
            string observacao = "Sem cebola";

            // Act
            pedido.AdicionarItem(idProduto, quantidade, 10, observacao);

            // Assert
            pedido.Itens.Should().HaveCount(1);
            var item = pedido.Itens[0];
            item.IdProduto.Should().Be(idProduto);
            item.Quantidade.Should().Be(quantidade);
            item.Observacao.Should().Be(observacao);
        }

        [Fact]
        public void AtualizarStatus_DeveAtualizarCorretamente()
        {
            // Arrange
            var pedido = new Pedido("12345678900");

            // Act
            pedido.AtualizarStatus(StatusPedido.EmPreparacao);

            // Assert
            pedido.Status.Should().Be(StatusPedido.EmPreparacao);
        }

        [Fact]
        public void AtualizarStatus_DeveLancarExcecaoSeFinalizado()
        {
            // Arrange
            var pedido = new Pedido("12345678900");
            pedido.AtualizarStatus(StatusPedido.Finalizado);

            // Act
            Action act = () => pedido.AtualizarStatus(StatusPedido.Pronto);

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Pedido já está finalizado.");
        }

        [Fact]
        public void AtualizarPagamento_DeveAtualizarStatusPagamento()
        {
            // Arrange
            var pedido = new Pedido("12345678900");

            // Act
            pedido.AtualizarPagamento("Aprovado");

            // Assert
            pedido.StatusPagamento.Should().Be("Aprovado");
            pedido.DataStatusPagamento.Should().NotBeNull();
        }

        [Fact]
        public void AdicionarItem_ComQuantidadeInvalida_DeveLancarExcecao()
        {
            // Arrange
            var pedido = new Pedido("12345678900");

            // Act
            Action act = () => pedido.AdicionarItem(1, 0, 10, "teste");

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Quantidade deve ser maior que zero.");
        }
    }
}
