using Crosscutting.DTOs;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces.Repositories
{
    public interface IPedidoRepository
    {
        Task AddAsync(Pedido pedido);
        Task UpdateAsync(Pedido pedido);
        Task<Pedido?> ObterPorIdAsync(Guid idPedido);
        Task<List<Pedido>> ObterPedidosAsync(ObterPedidosDTO dto);
    }

}