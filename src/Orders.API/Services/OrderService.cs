using Orders.API.Data;
using Orders.API.DTOs;
using Orders.API.Exceptions;

namespace Orders.API.Services
{
    public class OrderService
    {
        private readonly OrderRepository _repository;

        public OrderService(OrderRepository repository) => _repository = repository;

        // --- GET ALL ---
        public async Task<IEnumerable<OrderResponse>> GetAllAsync(Guid? usuarioId)
        {
            var orders = await _repository.GetAllAsync(usuarioId);

            return orders.Select(o => new OrderResponse
            {
                Id = o.Id,
                UsuarioId = o.UsuarioId,
                Items = o.Items.Select(i => new OrderItemResponse
                {
                    ProductoId = i.ProductoId,
                    Cantidad = i.Cantidad,
                    PrecioUnitario = i.PrecioUnitario
                }).ToList(),
                Total = o.Total,
                Estado = o.Estado,
                FechaCreacion = o.FechaCreacion
            });
        }

        // --- GET BY ID ---
        public async Task<OrderResponse> GetByIdAsync(Guid id)
        {
            var order = await _repository.GetByIdAsync(id);

            if (order == null)
                throw new NotFoundException("ORD-001", "Orden no encontrada.");

            return new OrderResponse
            {
                Id = order.Id,
                UsuarioId = order.UsuarioId,
                Items = order.Items.Select(i => new OrderItemResponse
                {
                    ProductoId = i.ProductoId,
                    Cantidad = i.Cantidad,
                    PrecioUnitario = i.PrecioUnitario
                }).ToList(),
                Total = order.Total,
                Estado = order.Estado,
                FechaCreacion = order.FechaCreacion
            };
        }

        // --- UPDATE STATUS ---
        public async Task<UpdateOrderStatusResponse> UpdateStatusAsync(Guid id, UpdateOrderStatusRequest request)
        {
            var order = await _repository.GetByIdAsync(id);

            if (order == null)
                throw new NotFoundException("ORD-001", "Orden no encontrada.");

            var esValida = (order.Estado, request.Estado) switch
            {
                ("Pendiente", "Confirmada") => true,
                ("Pendiente", "Cancelada") => true,
                ("Confirmada", "Enviada") => true,
                ("Confirmada", "Cancelada") => true,
                ("Enviada", "Entregada") => true,
                _ => false
            };

            if (!esValida)
                throw new BusinessRuleException("ORD-006",
                    $"Una orden en estado '{order.Estado}' no puede pasar a '{request.Estado}'.");

            var updated = await _repository.UpdateStatusAsync(id, request.Estado);

            return new UpdateOrderStatusResponse
            {
                Id = updated!.Id,
                Estado = updated.Estado,
                FechaActualizacion = DateTime.UtcNow
            };
        }
    }
}
