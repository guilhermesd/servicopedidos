using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Testcontainers.MongoDb;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Domain.Interfaces.ServicosExternos;
using Infrastructure.ServicosExternos;
using Application.UseCases;
using Crosscutting.DTOs;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Text;
using Newtonsoft.Json;

namespace Tests.Integracao.Controller;

public class PedidoControllerTest : IAsyncLifetime
{
    private readonly MongoDbContainer _mongoContainer;
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    public PedidoControllerTest()
    {
        _mongoContainer = new MongoDbBuilder()
            .WithImage("mongo:7.0")
            .WithCleanUp(true)
            .WithName($"mongo-pedidos-test{Guid.NewGuid()}")
            .WithPortBinding(27017, true)
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _mongoContainer.StartAsync();

        var connectionString = _mongoContainer.GetConnectionString(); // exemplo: "mongodb://localhost:32776"

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var inMemorySettings = new Dictionary<string, string>
                    {
                        ["MongoSettings:ConnectionString"] = connectionString,
                        ["MongoSettings:Database"] = "TestDb"
                    };

                    configBuilder.AddInMemoryCollection(inMemorySettings);
                });

                builder.ConfigureServices(services =>
                {
                    // Se necessário, substitua repositórios ou services
                    services.RemoveAll<IPagamentoService>(); // Garante que o real não está registrado
                    services.AddScoped<IPagamentoService, PagamentoServiceMock>();

                });
            });

        _client = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        await _mongoContainer.DisposeAsync();
    }

    private async Task<PedidoGeradoDTO?> GerarPedido()
    {
        // Arrange
        var gerarPedidoDto = new GerarPedidoDTO
        {
            Itens = new List<PedidoItemDTO>
            {
                new PedidoItemDTO
                {
                    IdProduto = 1,
                    Observacao = "Obse",
                    PrecoUnitario = 1,
                    Quantidade = 1
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/pedidos", gerarPedidoDto);

        return await response.Content.ReadFromJsonAsync<PedidoGeradoDTO>();
    }

    [Fact]
    public async Task Deve_Gerar_Pedido_Com_Sucesso()
    {
        var retorno = await GerarPedido();
        retorno.Should().NotBeNull();
    }

    [Fact]
    public async Task Deve_Atualizar_Status_Pedido()
    {
        // Arrange
        var gerado = await GerarPedido();
        var pedidoId = gerado?.IdPedido; // Idealmente você insere um pedido real no Mongo antes.
        var dto = new AlterarStatusPedidoDTO
        {
            Status = StatusPedidoDTO.EmPreparacao
        };

        var json = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync($"/api/pedidos/{pedidoId}/alterar/status-pedido", json);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Deve_Atualizar_Status_Pagamento()
    {
        // Arrange
        var gerado = await GerarPedido();
        var pedidoId = gerado?.IdPedido; // Idealmente você insere um pedido real no Mongo antes.
        var dto = new AlterarStatusPagamentoDTO
        {
            Status = "PAGO"
        };

        var json = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync($"/api/pedidos/{pedidoId}/alterar/status-pagamento", json);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Deve_Obter_Pedidos_Com_Filtro()
    {
        // Arrange
        var retorno = await GerarPedido();
        var filtro = new ObterPedidosDTO
        {
            StatusPedidoDTO = StatusPedidoDTO.Recebido,
            SomentePedidosEmProducao = true
        };

        var query = $"?StatusPedidoDTO={filtro.StatusPedidoDTO}&SomentePedidosEmProducao={filtro.SomentePedidosEmProducao}";

        // Act
        var response = await _client.GetAsync($"/api/pedidos{query}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        var pedidos = JsonConvert.DeserializeObject<List<PedidoDTO>>(body);
        pedidos.Should().NotBeNull();
    }
}
