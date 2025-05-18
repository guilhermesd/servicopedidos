using Crosscutting.DTOs;

namespace Domain.Interfaces.ServicosExternos
{
    public interface IPagamentoService
    {
        Task<PagamentoResponse> GerarPagamento(PagamentoRequest pagamentoRequest);
    }
}