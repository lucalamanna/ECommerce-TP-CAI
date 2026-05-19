namespace Orders.API.DTOs
{
    public class UpdateOrderStatusResponse
    {
        public Guid Id { get; set; }
        public string Estado { get; set; }
        public DateTime FechaActualizacion { get; set; }
    }
}
