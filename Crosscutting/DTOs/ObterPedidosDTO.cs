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
        public int Pagina { get; set; } = 0;
        public int TamanhoPagina { get; set; } = 10;
    }
}