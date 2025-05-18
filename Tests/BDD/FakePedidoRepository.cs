using Domain.Entities;
using Crosscutting.DTOs; // ou o namespace onde está ObterPedidosDTO
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Interfaces.Repositories;

public class FakePedidoRepository : IPedidoRepository
{
    private readonly Dictionary<Guid, Pedido> _store = new();

    public Task AddAsync(Pedido pedido)
    {
        _store[pedido.IdPedido] = pedido;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Pedido pedido)
    {
        if (_store.ContainsKey(pedido.IdPedido))
        {
            _store[pedido.IdPedido] = pedido;
        }
        return Task.CompletedTask;
    }

    public Task<Pedido?> ObterPorIdAsync(Guid idPedido)
    {
        _store.TryGetValue(idPedido, out var pedido);
        return Task.FromResult(pedido);
    }

    public Task<List<Pedido>> ObterPedidosAsync(ObterPedidosDTO dto)
    {
        // Obter todos os pedidos do repositório
        var pedidos = _store.Values.ToList();

        // Filtra para ignorar pedidos finalizados, se solicitado
        if (dto.SomentePedidosEmProducao)
        {
            pedidos = pedidos.Where(p => p.Status != StatusPedido.Finalizado).ToList();
        }

        // Filtra pelo status específico, se informado
        if (dto.StatusPedidoDTO.HasValue)
        {
            // Aqui, convertemos o StatusPedidoDTO para StatusPedido
            // Se os enums forem equivalentes, você pode fazer cast direto
            var status = (StatusPedido)dto.StatusPedidoDTO.Value;
            pedidos = pedidos.Where(p => p.Status == status).ToList();
        }

        return Task.FromResult(pedidos);
    }
}
