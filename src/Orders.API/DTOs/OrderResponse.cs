namespace Orders.API.DTOs
{
    public class OrderResponse
    {
        public Guid Id { get; set; }
        public Guid UsuarioId { get; set; }
        public List<OrderItemResponse> Items { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
