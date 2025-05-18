using Application.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/pedidos")]
    public class PedidoController : ControllerBase
    {

        private readonly IGeracaoPedidosUseCase _geracaoPedidosUseCase;

        public PedidoController(IGeracaoPedidosUseCase geracaoPedidosUseCase)
        {
            _geracaoPedidosUseCase = geracaoPedidosUseCase;
        }

        /// <summary>
        /// Cria ou atualiza um produto.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> GerarPedido([FromBody] GerarPedidoDTO gerarPedidoDTO)
        {
            var idToken = string.Empty;
            var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
            if (authHeader?.StartsWith("Bearer ")??false)
                idToken = authHeader.Substring("Bearer ".Length).Trim();

            var pedidoGeradoDTO = await _geracaoPedidosUseCase.GerarAsync(gerarPedidoDTO, idToken);
            return Created(string.Empty, pedidoGeradoDTO);
        }
    }
}
