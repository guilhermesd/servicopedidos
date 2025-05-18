using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crosscutting.DTOs
{
    public class PagamentoRequest
    {
        public Guid IdPedido { get; set; }

        public decimal Valor { get; set; }
    }
}