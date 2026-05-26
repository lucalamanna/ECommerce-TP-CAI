namespace Orders.API.Models
{
    public class Order
    {
        public Guid Id { get; set; }
        public Guid UsuarioId { get; set; }
        public List<OrderItem> Items { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
