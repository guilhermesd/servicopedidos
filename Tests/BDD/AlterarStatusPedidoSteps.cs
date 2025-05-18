using Application.UseCases;
using Crosscutting.DTOs;
using Domain.Interfaces.ServicosExternos;
using FluentAssertions;
using Infrastructure.ServicosExternos;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using TechTalk.SpecFlow;
using Testcontainers.MongoDb;

[Binding]
public class AlterarStatusPedidoSteps : IAsyncDisposable
{
    private readonly ScenarioContext _scenarioContext;
    private readonly MongoDbContainer _mongoContainer;
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;
    private Guid _pedidoId;

    public AlterarStatusPedidoSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;

        _mongoContainer = new MongoDbBuilder()
            .WithImage("mongo:7.0")
            .WithCleanUp(true)
            .WithName($"mongo-pedido-specflow-{Guid.NewGuid()}")
            .WithPortBinding(27017, true)
            .Build();
    }

    [BeforeScenario]
    public async Task SetupAsync()
    {
        await _mongoContainer.StartAsync();

        var connectionString = _mongoContainer.GetConnectionString();

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var settings = new Dictionary<string, string>
                    {
                        ["MongoSettings:ConnectionString"] = connectionString,
                        ["MongoSettings:Database"] = "TestDb"
                    };
                    configBuilder.AddInMemoryCollection(settings);
                });

                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IPagamentoService>();
                    services.AddScoped<IPagamentoService, PagamentoServiceMock>();
                });
            });

        _client = _factory.CreateClient();
    }

    [Given(@"que existe um pedido com status ""(.*)""")]
    public async Task GivenQueExisteUmPedidoComStatus(string status)
    {
        var gerarPedidoDto = new GerarPedidoDTO
        {
            Itens = new List<PedidoItemDTO>
            {
                new PedidoItemDTO
                {
                    IdProduto = 1,
                    Observacao = "Teste",
                    PrecoUnitario = 10,
                    Quantidade = 1
                }
            }
        };

        var response = await _client.PostAsJsonAsync("/api/pedidos", gerarPedidoDto);
        var pedidoGerado = await response.Content.ReadFromJsonAsync<PedidoGeradoDTO>();
        _pedidoId = pedidoGerado!.IdPedido;

        if (status != "Recebido")
        {
            var alterar = new AlterarStatusPedidoDTO
            {
                Status = Enum.Parse<StatusPedidoDTO>(status)
            };

            var content = new StringContent(JsonConvert.SerializeObject(alterar), Encoding.UTF8, "application/json");
            await _client.PutAsync($"/api/pedidos/{_pedidoId}/alterar/status-pedido", content);
        }
    }

    [When(@"eu altero o status do pedido para ""(.*)""")]
    public async Task WhenEuAlteroOStatusDoPedidoPara(string novoStatus)
    {
        var dto = new AlterarStatusPedidoDTO
        {
            Status = Enum.Parse<StatusPedidoDTO>(novoStatus)
        };

        var json = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");
        var response = await _client.PutAsync($"/api/pedidos/{_pedidoId}/alterar/status-pedido", json);

        _scenarioContext["Response"] = response;
    }

    [Then(@"o pedido deve ser atualizado com sucesso")]
    public void ThenOPedidoDeveSerAtualizadoComSucesso()
    {
        var response = (HttpResponseMessage)_scenarioContext["Response"];
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    public async ValueTask DisposeAsync()
    {
        await _mongoContainer.DisposeAsync();
    }
}
