using Application.UseCases;
using Crosscutting.DTOs;
using Domain.Entities;
using Domain.Interfaces.Repositories;
using FluentAssertions;
using Moq;

namespace Tests.Unidade.Application.UseCases
{
    public class ObterPedidosUseCaseTests
    {
        [Fact]
        public async Task ExecutarAsync_DeveRetornarListaDePedidoDTO_MapeadoCorretamente()
        {
            // Arrange
            var mockRepo = new Mock<IPedidoRepository>();
            var useCase = new ObterPedidosUseCase(mockRepo.Object);

            var dto = new ObterPedidosDTO
            {
                SomentePedidosEmProducao = false,
                StatusPedidoDTO = null
            };

            var pedido = new Pedido("12345678901");
            pedido.AdicionarItem(1, 2, 10.5m, "Sem cebola");
            pedido.AtualizarStatus(StatusPedido.EmPreparacao);
            pedido.AtualizarPagamento("Pago");

            var pedidosFake = new List<Pedido> { pedido };

            mockRepo.Setup(r => r.ObterPedidosAsync(dto)).ReturnsAsync(pedidosFake);

            // Act
            var resultado = await useCase.ExecutarAsync(dto);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().HaveCount(1);

            var dtoResultado = resultado[0];

            dtoResultado.IdPedido.Should().Be(pedido.IdPedido);
            dtoResultado.CpfCliente.Should().Be(pedido.CpfCliente);
            dtoResultado.Data.Should().BeCloseTo(pedido.Data, TimeSpan.FromSeconds(1));
            dtoResultado.DataStatusPedido.Should().BeCloseTo(pedido.DataStatusPedido, TimeSpan.FromSeconds(1));
            dtoResultado.StatusPagamento.Should().Be(pedido.StatusPagamento);
            dtoResultado.DataStatusPagamento.Should().BeCloseTo(pedido.DataStatusPagamento.Value, TimeSpan.FromSeconds(1));
            dtoResultado.Status.Should().Be((StatusPedidoDTO)pedido.Status);
            dtoResultado.ValorPedido.Should().Be(pedido.GetValorPedido());

            dtoResultado.Itens.Should().HaveCount(1);
            var itemDto = dtoResultado.Itens[0];
            var itemOriginal = pedido.Itens[0];

            itemDto.IdProduto.Should().Be(itemOriginal.IdProduto);
            itemDto.Observacao.Should().Be(itemOriginal.Observacao);
            itemDto.PrecoUnitario.Should().Be(itemOriginal.PrecoUnitario);
            itemDto.Quantidade.Should().Be(itemOriginal.Quantidade);

            mockRepo.Verify(r => r.ObterPedidosAsync(dto), Times.Once);
        }
    }
}
