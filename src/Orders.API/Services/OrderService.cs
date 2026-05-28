using Orders.API.Data;
using Orders.API.DTOs;
using Orders.API.Exceptions;
using Orders.API.Models;
using System.Net.Http;

namespace Orders.API.Services
{
    public class OrderService
    {
        private readonly OrderRepository _repository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<OrderService> _logger;

        public OrderService (OrderRepository repository, IHttpClientFactory httpClientFactory, ILogger<OrderService> logger)
        {
            _repository = repository;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }
       
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

        
        public async Task<OrderResponse> CreateAsync(CreateOrderRequest request)
        {
            
            if (request.Items == null || !request.Items.Any() || request.Items.Any(i => i.Cantidad <= 0))
                throw new ValidationException("ORD-002", "Los datos de la orden son inválidos.");

            var client = _httpClientFactory.CreateClient("ProductsApi");
            var items = new List<OrderItem>();
            decimal total = 0;

          
            foreach (var itemRequest in request.Items)
            {
                var response = await client.GetAsync($"/api/products/{itemRequest.ProductoId}");

               
                if (!response.IsSuccessStatusCode)
                    throw new NotFoundException("ORD-004", $"Producto '{itemRequest.ProductoId}' no encontrado.");

                var product = await response.Content.ReadFromJsonAsync<ProductResponse>();

               
                if (product!.Stock < itemRequest.Cantidad)
                    throw new BusinessRuleException("ORD-005",
                        $"Stock insuficiente para '{product.Nombre}'. Disponible: {product.Stock}, solicitado: {itemRequest.Cantidad}.");

                items.Add(new OrderItem
                {
                    ProductoId = product.Id,
                    Cantidad = itemRequest.Cantidad,
                    PrecioUnitario = product.Precio
                });

                total += itemRequest.Cantidad * product.Precio;
            }
           
            var order = new Order
            {
                Id = Guid.NewGuid(),
                UsuarioId = request.UsuarioId,
                Items = items,
                Total = total,
                Estado = "Pendiente",
                FechaCreacion = DateTime.UtcNow
            };

            var created = await _repository.CreateAsync(order);

            return new OrderResponse
            {
                Id = created.Id,
                UsuarioId = created.UsuarioId,
                Items = created.Items.Select(i => new OrderItemResponse
                {
                    ProductoId = i.ProductoId,
                    Cantidad = i.Cantidad,
                    PrecioUnitario = i.PrecioUnitario
                }).ToList(),
                Total = created.Total,
                Estado = created.Estado,
                FechaCreacion = created.FechaCreacion
            };
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var order = await _repository.GetByIdAsync(id);

            if (order == null)
                throw new NotFoundException("ORD-001", "Orden no encontrada.");

            if (order.Estado != "Cancelada")
                throw new BusinessRuleException("ORD-008",
                    "Solo se pueden eliminar órdenes en estado 'Cancelada'.");

            return await _repository.DeleteAsync(id);
        }


    }
}
