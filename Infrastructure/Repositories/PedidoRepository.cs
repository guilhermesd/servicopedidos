using Crosscutting.DTOs;
using Domain.Entities;
using Domain.Interfaces.Repositories;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Infrastructure.Repositories
{
    public class PedidoRepository : IPedidoRepository
    {
        private readonly IPedidoContext _context;

        public PedidoRepository(IPedidoContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Pedido pedido)
        {
            await _context.Pedidos.InsertOneAsync(pedido);
        }

        public async Task UpdateAsync(Pedido pedido)
        {
            var filter = Builders<Pedido>.Filter.Eq(p => p.IdPedido, pedido.IdPedido);

            var resultado = await _context.Pedidos.ReplaceOneAsync(
                filter: filter,
                replacement: pedido,
                options: new ReplaceOptions { IsUpsert = false } // Não insere, apenas atualiza
            );

            if (resultado.MatchedCount == 0)
                throw new InvalidOperationException($"Pedido com Id {pedido.IdPedido} não encontrado para atualização.");
        }

        public async Task<Pedido?> ObterPorIdAsync(Guid idPedido)
        {
            return await _context.Pedidos
                .Find(p => p.IdPedido == idPedido)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Pedido>> ObterPedidosAsync(ObterPedidosDTO dto)
        {
            var pedidoQuery = _context.Pedidos.AsQueryable();

            if (dto.SomentePedidosEmProducao)
            {
                pedidoQuery.Where(c => c.Status != StatusPedido.Finalizado);
            }

            // Filtrar por status específico (opcional)
            if (dto.StatusPedidoDTO.HasValue)
            {
                pedidoQuery.Where(c => c.Status == (StatusPedido) dto.StatusPedidoDTO);
            }

            // Obtem os pedidos filtrados
            var pedidos = await pedidoQuery.ToListAsync();

            // Ordenação por prioridade de status e data
            var statusOrdem = new Dictionary<StatusPedido, int>
    {
        { StatusPedido.Pronto, 0 },
        { StatusPedido.EmPreparacao, 1 },
        { StatusPedido.Recebido, 2 },
        { StatusPedido.Finalizado, 3 },
    };

            var pedidosOrdenados = pedidos
                .Where(p => statusOrdem.ContainsKey(p.Status)) // Remove "Finalizado" da ordenação, se aparecer
                .OrderBy(p => statusOrdem[p.Status])
                .ThenBy(p => p.Data)
                .ToList();

            return pedidosOrdenados;
        }

    }
}
