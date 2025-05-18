using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crosscutting.DTOs
{
    public class ObterPedidosDTO
    {
        /// <summary>
        /// Se TRUE somente em processo de produção ignora os FINALIZADOS
        /// </summary>
        public bool SomentePedidosEmProducao {  get; set; }

        /// <summary>
        /// Retorna por um status específico
        /// </summary>
        public StatusPedidoDTO? StatusPedidoDTO { get; set; }

    }
}