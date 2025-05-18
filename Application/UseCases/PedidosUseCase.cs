using Crosscutting.DTOs;
using Crosscutting.Exceptions;
using Domain.Entities;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.ServicosExternos;
using System.IdentityModel.Tokens.Jwt;

namespace Application.UseCases
{
    public static class JwtHelper
    {
        public static string? ObterUsernameDoIdToken(string idToken)
        {
            var handler = new JwtSecurityTokenHandler();

            // Lê o token sem validar (assumindo que já foi autenticado)
            var jwt = handler.ReadJwtToken(idToken);

            // Tenta pegar o claim "username"
            var usernameClaim = jwt.Claims.FirstOrDefault(c => c.Type == "username");

            if (usernameClaim != null)
                return usernameClaim.Value;

            // Se não encontrar, tenta pegar o claim "cognito:username"
            return jwt.Claims.FirstOrDefault(c => c.Type == "cognito:username")?.Value;
        }
    }

    public class PedidoGeradoDTO
    {
        public Guid IdPedido { get; set; }

        public string? QrCode { get; set; }
    }

    public class GerarPedidoDTO
    {
        public List<PedidoItemDTO> Itens { get; set; } = new(); // Se aplicável
    }

    public class AlterarStatusPedidoDTO
    {
        public StatusPedidoDTO Status { get; set; }
    }

    public class AlterarStatusPagamentoDTO
    {
        public string? Status { get; set; }
    }

    public interface IGeracaoPedidosUseCase
    {
        Task<PedidoGeradoDTO> GerarAsync(GerarPedidoDTO gerarPedidoDTO, string idToken);
    }

    public class GeracaoPedidosUseCase : IGeracaoPedidosUseCase
    {
        private readonly IPedidoRepository _pedidoRepository;
        private readonly IPagamentoService _pagamentoService;

        public GeracaoPedidosUseCase(IPedidoRepository pedidoRepository, IPagamentoService pagamentoService)
        {
            _pedidoRepository = pedidoRepository;
            _pagamentoService = pagamentoService;
        }

        public async Task<PedidoGeradoDTO> GerarAsync(GerarPedidoDTO gerarPedidoDTO, string idToken)
        {
            var cpfCliente = string.Empty;
            if (!string.IsNullOrEmpty(idToken))
                cpfCliente = JwtHelper.ObterUsernameDoIdToken(idToken);

            var pedido = new Pedido(cpfCliente);
            foreach (var item in gerarPedidoDTO.Itens)
                pedido.AdicionarItem(item.IdProduto, item.Quantidade, item.PrecoUnitario, item.Observacao);

            await _pedidoRepository.AddAsync(pedido);

            var pagamentoResponse = await _pagamentoService.GerarPagamento(new PagamentoRequest
            {
                IdPedido = pedido.IdPedido,
                Valor = pedido.GetValorPedido()
            });

            return new PedidoGeradoDTO
            {
                IdPedido = pedido.IdPedido,
                QrCode = pagamentoResponse.QrCode,
            };
        }
    }

    public interface IAlteraStatusPedidosUseCase
    {
        Task<bool> AlterarAsync(Guid idPedido, AlterarStatusPedidoDTO alterarStatusPedidoDTO);
    }

    public class AlteraStatusPedidosUseCase : IAlteraStatusPedidosUseCase
    {
        private readonly IPedidoRepository _pedidoRepository;

        public AlteraStatusPedidosUseCase(IPedidoRepository pedidoRepository)
        {
            _pedidoRepository = pedidoRepository;
        }

        public async Task<bool> AlterarAsync(Guid idPedido, AlterarStatusPedidoDTO alterarStatusPedidoDTO)
        {

            var pedido = await _pedidoRepository.ObterPorIdAsync(idPedido);

            if (pedido == null)
                throw new NotFoundException("Pedido não encontrado");

            pedido.AtualizarStatus((StatusPedido)alterarStatusPedidoDTO.Status);

            await _pedidoRepository.UpdateAsync(pedido);

            return true;
        }
    }

    public interface IAlteraStatusPagamentoUseCase
    {
        Task<bool> AlterarAsync(Guid idPedido, AlterarStatusPagamentoDTO alterarStatusPagamentoDTO);
    }

    public class AlteraStatusPagamentoUseCase : IAlteraStatusPagamentoUseCase
    {
        private readonly IPedidoRepository _pedidoRepository;

        public AlteraStatusPagamentoUseCase(IPedidoRepository pedidoRepository)
        {
            _pedidoRepository = pedidoRepository;
        }

        public async Task<bool> AlterarAsync(Guid idPedido, AlterarStatusPagamentoDTO alterarStatusPagamentoDTO)
        {
            var pedido = await _pedidoRepository.ObterPorIdAsync(idPedido);

            if (pedido == null)
                throw new NotFoundException("Pedido não encontrado");

            pedido.AtualizarPagamento(alterarStatusPagamentoDTO.Status ?? string.Empty);

            await _pedidoRepository.UpdateAsync(pedido);

            return true;
        }
    }

    public interface IObterPedidosUseCase
    {
        Task<List<PedidoDTO>> ExecutarAsync(ObterPedidosDTO obterPedidosDTO);
    }

    public class ObterPedidosUseCase : IObterPedidosUseCase
    {
        private readonly IPedidoRepository _pedidoRepository;

        public ObterPedidosUseCase(IPedidoRepository pedidoRepository)
        {
            _pedidoRepository = pedidoRepository;
        }

        public async Task<List<PedidoDTO>> ExecutarAsync(ObterPedidosDTO obterPedidosDTO)
        {
            try
            {
                var pedidos = await _pedidoRepository.ObterPedidosAsync(obterPedidosDTO);
                var retorno = new List<PedidoDTO>();
                foreach (var pedido in pedidos)
                {
                    var pedidoDto = new PedidoDTO
                    {
                        IdPedido = pedido.IdPedido,
                        CpfCliente = pedido.CpfCliente,
                        Data = pedido.Data,
                        DataStatusPagamento = pedido.DataStatusPagamento,
                        DataStatusPedido = pedido.DataStatusPedido,
                        StatusPagamento = pedido.StatusPagamento,
                        Status = (StatusPedidoDTO)pedido.Status,
                        ValorPedido = pedido.GetValorPedido()
                    };

                    foreach (var itemPedido in pedido.Itens)
                    {
                        pedidoDto.Itens.Add(new PedidoItemDTO
                        {
                            IdProduto = itemPedido.IdProduto,
                            Observacao = itemPedido.Observacao,
                            PrecoUnitario = itemPedido.PrecoUnitario,
                            Quantidade = itemPedido.Quantidade,
                        });
                    }

                    retorno.Add(pedidoDto);
                }

                return retorno;
            }
            catch (Exception ex) 
            {
                var e = ex;
                throw;
            }
        }
    }
}
