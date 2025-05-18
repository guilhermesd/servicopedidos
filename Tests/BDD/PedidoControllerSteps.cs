using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Controllers;
using Application.UseCases;
using Crosscutting.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TechTalk.SpecFlow;
using Xunit;

namespace Api.Tests.Steps
{
    [Binding]
    public class PedidoControllerSteps
    {
        private readonly Mock<IGeracaoPedidosUseCase> _mockGeracaoPedidosUseCase = new();
        private readonly Mock<IAlteraStatusPedidosUseCase> _mockAlteraStatusPedidosUseCase = new();
        private readonly Mock<IAlteraStatusPagamentoUseCase> _mockAlteraStatusPagamentoUseCase = new();
        private readonly Mock<IObterPedidosUseCase> _mockObterPedidosUseCase = new();

        private PedidoController _controller;
        private IActionResult _result;

        private GerarPedidoDTO _gerarPedidoDTO;
        private AlterarStatusPedidoDTO _alterarStatusPedidoDTO;
        private AlterarStatusPagamentoDTO _alterarStatusPagamentoDTO;
        private ObterPedidosDTO _filtroPedidos;
        private Guid _pedidoId;

        private PedidoGeradoDTO _pedidoGeradoDTO;
        private List<PedidoDTO> _listaPedidos;

        private DefaultHttpContext _httpContext;

        public PedidoControllerSteps()
        {
            _controller = new PedidoController(
                _mockGeracaoPedidosUseCase.Object,
                _mockAlteraStatusPedidosUseCase.Object,
                _mockAlteraStatusPagamentoUseCase.Object,
                _mockObterPedidosUseCase.Object);

            _httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = _httpContext
            };
        }

        [Given(@"que o DTO para gerar pedido está disponível")]
        public void GivenQueODTOParaGerarPedidoEstaDisponivel()
        {
            _gerarPedidoDTO = new GerarPedidoDTO();

            _pedidoGeradoDTO = new PedidoGeradoDTO();

            _mockGeracaoPedidosUseCase
                .Setup(x => x.GerarAsync(_gerarPedidoDTO, It.IsAny<string>()))
                .ReturnsAsync(_pedidoGeradoDTO);
        }

        [Given(@"o token de autorização ""(.*)"" está presente no header")]
        public void GivenOTokenDeAutorizacaoEstaPresenteNoHeader(string token)
        {
            _httpContext.Request.Headers["Authorization"] = token;
        }

        [When(@"eu enviar a requisição para gerar pedido")]
        public async Task WhenEuEnviarARequisicaoParaGerarPedido()
        {
            _result = await _controller.GerarPedido(_gerarPedidoDTO);
        }

        [Then(@"a resposta deve ser Created com o pedido gerado")]
        public void ThenARespostaDeveSerCreatedComOPedidoGerado()
        {
            var createdResult = Assert.IsType<CreatedResult>(_result);
            Assert.Equal(_pedidoGeradoDTO, createdResult.Value);
        }

        [Given(@"que o id do pedido e o DTO de alteração de status estão disponíveis")]
        public void GivenQueOIdDoPedidoEDTODeAlteracaoDeStatusEstaoDisponiveis()
        {
            _pedidoId = Guid.NewGuid();
            _alterarStatusPedidoDTO = new AlterarStatusPedidoDTO();

            _mockAlteraStatusPedidosUseCase
                .Setup(x => x.AlterarAsync(_pedidoId, _alterarStatusPedidoDTO))
                .ReturnsAsync(true);
        }

        [When(@"eu enviar a requisição para atualizar status do pedido")]
        public async Task WhenEuEnviarARequisicaoParaAtualizarStatusDoPedido()
        {
            _result = await _controller.AtualizarStatusPedido(_pedidoId, _alterarStatusPedidoDTO);
        }

        [Then(@"a resposta deve ser NoContent")]
        public void ThenARespostaDeveSerNoContent()
        {
            Assert.IsType<NoContentResult>(_result);
        }

        [Given(@"que o id do pedido e o DTO de alteração de status de pagamento estão disponíveis")]
        public void GivenQueOIdDoPedidoEDTODeAlteracaoDeStatusDePagamentoEstaoDisponiveis()
        {
            _pedidoId = Guid.NewGuid();
            _alterarStatusPagamentoDTO = new AlterarStatusPagamentoDTO();

            _mockAlteraStatusPagamentoUseCase
                .Setup(x => x.AlterarAsync(_pedidoId, _alterarStatusPagamentoDTO))
                .ReturnsAsync(true);
        }

        [When(@"eu enviar a requisição para atualizar status do pagamento")]
        public async Task WhenEuEnviarARequisicaoParaAtualizarStatusDoPagamento()
        {
            _result = await _controller.AtualizarStatusPagamento(_pedidoId, _alterarStatusPagamentoDTO);
        }

        [Given(@"que o filtro de pedidos está disponível")]
        public void GivenQueOFiltroDePedidosEstaDisponivel()
        {
            _filtroPedidos = new ObterPedidosDTO();

            _listaPedidos = new List<PedidoDTO>
            {
                new PedidoDTO(),
                new PedidoDTO()
            };

            _mockObterPedidosUseCase
                .Setup(x => x.ExecutarAsync(It.IsAny<ObterPedidosDTO>()))
                .ReturnsAsync(_listaPedidos);
        }

        [When(@"eu enviar a requisição para obter pedidos")]
        public async Task WhenEuEnviarARequisicaoParaObterPedidos()
        {
            _result = await _controller.ObterPedidos(_filtroPedidos);
        }

        [Then(@"a resposta deve ser Ok com a lista de pedidos")]
        public void ThenARespostaDeveSerOkComAListaDePedidos()
        {
            var okResult = Assert.IsType<OkObjectResult>(_result);
            Assert.Equal(_listaPedidos, okResult.Value);
        }
    }
}
