using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class PedidoItem
    {
        public int IdProduto { get; private set; }
        public int Quantidade { get; private set; }
        public decimal PrecoUnitario { get; private set; } 
        public string? Observacao { get; private set; }

        // Construtor para MongoDB
        private PedidoItem() { }

        public PedidoItem(int idProduto, int quantidade, decimal precoUnitario, string? observacao)
        {
            IdProduto = idProduto;
            Quantidade = quantidade;
            Observacao = observacao;
            PrecoUnitario = precoUnitario;
        }
    }
}