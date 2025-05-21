using Application.Middleares;
using Application.UseCases;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.ServicosExternos;
using Infrastructure.Repositories;
using Infrastructure.ServicosExternos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Injeção de dependências para Repositórios
builder.Services.AddScoped<IPedidoContext, PedidoContext>();
builder.Services.AddScoped<IPedidoRepository, PedidoRepository>();
builder.Services.AddScoped<IPagamentoService, PagamentoService>();
builder.Services.AddHttpClient<IPagamentoService, PagamentoService>();

// Injeção de dependências para Use Cases
builder.Services.AddScoped<IGeracaoPedidosUseCase, GeracaoPedidosUseCase>();
builder.Services.AddScoped<IAlteraStatusPedidosUseCase, AlteraStatusPedidosUseCase>();
builder.Services.AddScoped<IObterPedidosUseCase, ObterPedidosUseCase>();
builder.Services.AddScoped<IAlteraStatusPagamentoUseCase, AlteraStatusPagamentoUseCase>();

// ✅ Aqui está sua configuração de autenticação
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://cognito-idp.{regiao}.amazonaws.com/{poolId}";
        options.Audience = "{clientId}";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
        };
    });

// Swagger com suporte a Bearer
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Minha API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT com o prefixo 'Bearer '",
    });

    c.OperationFilter<OptionalAuthorizeCheckOperationFilter>();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionsMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }

public class OptionalAuthorizeCheckOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Security ??= new List<OpenApiSecurityRequirement>();

        var bearerScheme = new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
        };

        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [bearerScheme] = new string[] { }
        });
    }
}
