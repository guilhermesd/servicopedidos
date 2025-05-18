using Application.UseCases;
using Crosscutting.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/pedidos")]
    public class PedidoController : ControllerBase
    {

        private readonly IGeracaoPedidosUseCase _geracaoPedidosUseCase;
        private readonly IAlteraStatusPedidosUseCase _alteraStatusPedidosUseCase;
        private readonly IAlteraStatusPagamentoUseCase _alteraStatusPagamentoUseCase;
        private readonly IObterPedidosUseCase _obterPedidosUseCase;

        public PedidoController(IGeracaoPedidosUseCase geracaoPedidosUseCase, IAlteraStatusPedidosUseCase alteraStatusPedidosUseCase, IAlteraStatusPagamentoUseCase alteraStatusPagamentoUseCase, IObterPedidosUseCase obterPedidosUseCase)
        {
            _geracaoPedidosUseCase = geracaoPedidosUseCase;
            _alteraStatusPedidosUseCase = alteraStatusPedidosUseCase;
            _alteraStatusPagamentoUseCase = alteraStatusPagamentoUseCase;
            _obterPedidosUseCase = obterPedidosUseCase;
        }

        /// <summary>
        /// Cria ou atualiza um produto.
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> GerarPedido([FromBody] GerarPedidoDTO gerarPedidoDTO)
        {
            var idToken = string.Empty;
            var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
            if (authHeader?.StartsWith("Bearer ")??false)
                idToken = authHeader.Substring("Bearer ".Length).Trim();

            var pedidoGeradoDTO = await _geracaoPedidosUseCase.GerarAsync(gerarPedidoDTO, idToken);
            return Created(string.Empty, pedidoGeradoDTO);
        }

        /// <summary>
        /// Atualiza um status pedido.
        /// </summary>
        [HttpPut("{idPedido}/alterar/status-pedido")]
        [AllowAnonymous]
        public async Task<IActionResult> AtualizarStatusPedido(Guid idPedido, [FromBody] AlterarStatusPedidoDTO alterarStatusPedidoDTO)
        {
            await _alteraStatusPedidosUseCase.AlterarAsync(idPedido, alterarStatusPedidoDTO);
            return NoContent();
        }

        /// <summary>
        /// Atualiza um status pagamento.
        /// </summary>
        [HttpPut("{idPedido}/alterar/status-pagamento")]
        [AllowAnonymous]
        public async Task<IActionResult> AtualizarStatusPagamento(Guid idPedido, [FromBody] AlterarStatusPagamentoDTO alterarStatusPagamentoDTO)
        {
            await _alteraStatusPagamentoUseCase.AlterarAsync(idPedido, alterarStatusPagamentoDTO);
            return NoContent();
        }

        /// <summary>
        /// Obtém pedidos com filtros opcionais.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ObterPedidos([FromQuery] ObterPedidosDTO filtro)
        {
            var pedidos = await _obterPedidosUseCase.ExecutarAsync(filtro);
            return Ok(pedidos);
        }
    }
}
