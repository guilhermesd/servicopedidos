using Domain.Entities;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public interface IPedidoContext
    {
        IMongoCollection<Pedido> Pedidos { get; }
    }

    public class PedidoContext : IPedidoContext
    {
        private readonly IMongoDatabase _database;

        public PedidoContext(IConfiguration configuration)
        {
            var client = new MongoClient(configuration["MongoDB:ConnectionString"]);
            _database = client.GetDatabase(configuration["MongoDB:DatabaseName"]);
        }

        public IMongoCollection<Pedido> Pedidos => _database.GetCollection<Pedido>("Pedidos");
    }
}
