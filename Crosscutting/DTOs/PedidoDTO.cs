using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crosscutting.DTOs
{
    public enum StatusPedidoDTO
    {
        Recebido,
        EmPreparacao,
        Pronto,
        Finalizado
    }
    public class PedidoItemDTO
    {
        public int IdProduto { get; set; }

        public string? Observacao { get; set; }

        public int Quantidade { get; set; }

        public decimal PrecoUnitario { get; set; }
    }

    public class PedidoDTO
    {
        public Guid IdPedido { get; set; }

        public DateTime Data { get; set; }

        public string? CpfCliente { get; set; }

        public StatusPedidoDTO Status { get; set; }

        public DateTime DataStatusPedido { get; set; }

        public string? StatusPagamento { get; set; }

        public DateTime? DataStatusPagamento { get; set; }

        public decimal ValorPedido { get; set; }

        public List<PedidoItemDTO> Itens { get; set; } = new();

    }
}
