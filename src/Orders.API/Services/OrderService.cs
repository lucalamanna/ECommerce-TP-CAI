using Orders.API.Data;
using Orders.API.DTOs;
using Orders.API.Exceptions;

namespace Orders.API.Services
{
    public class OrderService
    {
        private readonly OrderRepository _repository;
        private readonly ILogger<OrderService> _logger;

        public OrderService (OrderRepository repository, ILogger<OrderService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        // --- GET ALL ---
        public async Task<IEnumerable<OrderResponse>> GetAllAsync(Guid? usuarioId)
        {
            _logger.LogDebug("Obteniendo órdenes. UsuarioId: {UsuarioId}", usuarioId);
            
            var orders = await _repository.GetAllAsync(usuarioId);

            _logger.LogDebug("Órdenes obtenidas. Cantidad: {Cantidad}", orders.Count());
            
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
            _logger.LogDebug("Obteniendo orden. Id: {Id}", id);

            var order = await _repository.GetByIdAsync(id);

            if (order == null)
            {
                _logger.LogWarning("Orden no encontrada. ErrorCode: ORD-001, Id: {Id}", id);

                throw new NotFoundException("ORD-001", "Orden no encontrada.");
            }

            _logger.LogDebug("Orden encontrada. Id: {Id}, Estado: {Estado}", order.Id, order.Estado);

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
            _logger.LogDebug("Actualizando estado. Id: {Id}, Estado: {Estado}", id, request.Estado);
            
            var order = await _repository.GetByIdAsync(id);

            if (order == null)
            {
                _logger.LogWarning("Orden no encontrada. ErrorCode: ORD-001, Id: {Id}", id);
                throw new NotFoundException("ORD-001", "Orden no encontrada.");
            }
               

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
            {
                _logger.LogWarning("Transición inválida. ErrorCode: ORD-006, De: {De} A: {A}", order.Estado, request.Estado);
                throw new BusinessRuleException("ORD-006",
                    $"Una orden en estado '{order.Estado}' no puede pasar a '{request.Estado}'.");
            }

            var updated = await _repository.UpdateStatusAsync(id, request.Estado);
            
            _logger.LogDebug("Estado actualizado. Id: {Id}, Estado: {Estado}", id, request.Estado);

            return new UpdateOrderStatusResponse
            {
                Id = updated!.Id,
                Estado = updated.Estado,
                FechaActualizacion = DateTime.UtcNow
            };
        }
    }
}
