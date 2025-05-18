using Crosscutting.DTOs;
using Domain.Interfaces.ServicosExternos;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace Infrastructure.ServicosExternos
{
    public class PagamentoService: IPagamentoService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _configuration;

        public PagamentoService(HttpClient http, IConfiguration configuration)
        {
            _http = http;
            _configuration = configuration;
        }

        public async Task<PagamentoResponse> GerarPagamento(PagamentoRequest pagamentoRequest)
        {
            var response = await _http.PostAsJsonAsync($"{_configuration["UrlPagamento"]}/api/pagamentos", pagamentoRequest);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadFromJsonAsync<PagamentoResponse>();

            if (content is null)
                throw new Exception("Resposta do pagamento veio nula.");

            return content;
        }
    }


    public class PagamentoServiceMock : IPagamentoService
    {
        public async Task<PagamentoResponse> GerarPagamento(PagamentoRequest pagamentoRequest)
        {
            return new PagamentoResponse
            {
                IdPagamento = Guid.NewGuid(),
                QrCode = "rtrtrt"
            };
        }
    }
}
