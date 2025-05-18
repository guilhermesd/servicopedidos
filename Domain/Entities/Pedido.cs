using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities
{
    public enum StatusPedido
    {
        Recebido,
        EmPreparacao,
        Pronto,
        Finalizado
    }

    public class Pedido
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid IdPedido { get; private set; }

        public DateTime Data { get; private set; }

        public string? CpfCliente { get; private set; }

        [BsonRepresentation(BsonType.String)]
        public StatusPedido Status { get; private set; }

        public DateTime DataStatusPedido { get; private set; }

        public string? StatusPagamento { get; private set; }

        public DateTime? DataStatusPagamento { get; private set; }

        public List<PedidoItem> Itens { get; private set; } = new();

        // Construtor vazio exigido pelo MongoDB
        private Pedido() { }

        // Fábrica ou construtor de domínio
        public Pedido(string? cpfCliente)
        {
            IdPedido = Guid.NewGuid();
            Data = DateTime.UtcNow;
            CpfCliente = cpfCliente;
            DataStatusPedido = DateTime.UtcNow;
            Status = StatusPedido.Recebido;
        }

        // Comportamentos de domínio
        public void AdicionarItem(int idProduto, int quantidade, decimal precoUnitario, string? observacao)
        {
            if (quantidade <= 0)
                throw new ArgumentException("Quantidade deve ser maior que zero.");

            var item = new PedidoItem(idProduto, quantidade, precoUnitario, observacao);
            Itens.Add(item);
        }

        public void AtualizarStatus(StatusPedido novoStatus)
        {
            if (Status == StatusPedido.Finalizado)
                throw new InvalidOperationException("Pedido já está finalizado.");

            Status = novoStatus;
            DataStatusPedido = DateTime.UtcNow;
        }

        public void AtualizarPagamento(string statusPagamento)
        {
            StatusPagamento = statusPagamento;
            DataStatusPagamento = DateTime.UtcNow;
        }

        public decimal GetValorPedido()
        {
            return Itens.Sum(item => item.Quantidade * item.PrecoUnitario);
        }
    }
}