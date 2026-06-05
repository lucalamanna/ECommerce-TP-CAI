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
       
        public async Task<IEnumerable<OrderResponse>> GetAllAsync(Guid? usuarioId, Guid? productoId = null)
        {
            _logger.LogInformation("Obteniendo órdenes. UsuarioId: {UsuarioId}, ProductoId: {ProductoId}", usuarioId, productoId);
            
            var orders = await _repository.GetAllAsync(usuarioId, productoId);

            _logger.LogInformation("Órdenes obtenidas. Cantidad: {Cantidad}", orders.Count());

            return orders.Select(MapToResponse);
        }
       
        public async Task<OrderResponse> GetByIdAsync(Guid id)
        {
            _logger.LogInformation("Obteniendo orden. Id: {Id}", id);

            var order = await _repository.GetByIdAsync(id);

            if (order == null)
            {
                _logger.LogWarning("Orden no encontrada. ErrorCode: ORD-001, Id: {Id}", id);

                throw new NotFoundException("ORD-001", "Orden no encontrada.");
            }

            _logger.LogInformation("Orden encontrada. Id: {Id}, Estado: {Estado}", order.Id, order.Estado);

            return MapToResponse(order);
        }
      
        public async Task<UpdateOrderStatusResponse> UpdateStatusAsync(Guid id, UpdateOrderStatusRequest request)
        {
            _logger.LogInformation("Actualizando estado. Id: {Id}, Estado: {Estado}", id, request.Estado);
            
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
            
            _logger.LogInformation("Estado actualizado. Id: {Id}, Estado: {Estado}", id, request.Estado);

            return new UpdateOrderStatusResponse
            {
                Id = updated!.Id,
                Estado = updated.Estado,
                FechaActualizacion = DateTime.UtcNow
            };
        }

        
        public async Task<OrderResponse> CreateAsync(CreateOrderRequest request, string? correlationId)
        {

            _logger.LogInformation("Creando orden. UsuarioId: {UsuarioId}, Items: {Cantidad}",
            request.UsuarioId, request.Items?.Count ?? 0);

            ValidarRequest(request);

            var usersClient = _httpClientFactory.CreateClient("UsersApi");
            var userResponse = await usersClient.GetAsync($"/api/users/{request.UsuarioId}");

            if (!userResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning("Usuario no encontrado. ErrorCode: ORD-003, UsuarioId: {UsuarioId}",
                    request.UsuarioId);
                throw new NotFoundException("ORD-003", "Usuario no encontrado.");
            }

            var client = _httpClientFactory.CreateClient("ProductsApi");
            
            if (!string.IsNullOrWhiteSpace(correlationId))
                client.DefaultRequestHeaders.Add("X-Correlation-Id", correlationId);
            
            var items = new List<OrderItem>();
            decimal total = 0;

          
            foreach (var itemRequest in request.Items)
            {
                var response = await client.GetAsync($"/api/products/{itemRequest.ProductoId}");

               
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Producto no encontrado. ErrorCode: ORD-004, ProductoId: {ProductoId}", itemRequest.ProductoId);
                    throw new NotFoundException("ORD-004", $"Producto '{itemRequest.ProductoId}' no encontrado.");
                }
                   

                var product = await response.Content.ReadFromJsonAsync<ProductResponse>();

               
                if (product!.Stock < itemRequest.Cantidad)
                {
                    _logger.LogWarning("Stock insuficiente. ErrorCode: ORD-005, ProductoId: {ProductoId}, Disponible: {Stock}, Solicitado: {Cant}",
                        product.Id, product.Stock, itemRequest.Cantidad);
                    throw new BusinessRuleException("ORD-005",
                        $"Stock insuficiente para '{product.Nombre}'. Disponible: {product.Stock}, solicitado: {itemRequest.Cantidad}.");

                }

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
            _logger.LogInformation("Orden creada. Id: {Id}, Total: {Total}", created.Id, created.Total);

            return MapToResponse(created);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            _logger.LogInformation("Eliminando orden. Id: {Id}", id);
            var order = await _repository.GetByIdAsync(id);

            if (order == null)
            {
                _logger.LogWarning("Orden no encontrada. ErrorCode: ORD-001, Id: {Id}", id);
                throw new NotFoundException("ORD-001", "Orden no encontrada.");
            }
               

            if (order.Estado != "Cancelada")
            {
                _logger.LogWarning("Eliminación no permitida. ErrorCode: ORD-008, Estado: {Estado}, Id: {Id}", order.Estado, id);
                throw new BusinessRuleException("ORD-008", "Solo se pueden eliminar órdenes en estado 'Cancelada'.");
            }

            _logger.LogInformation("Orden eliminada. Id: {Id}", id);
            return await _repository.DeleteAsync(id);
        }

        private static OrderResponse MapToResponse(Order order)
        {
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

        private static void ValidarRequest(CreateOrderRequest request)
        {
            var errores = new List<string>();

            if (request.UsuarioId == Guid.Empty)
                errores.Add("El campo 'UsuarioId' es requerido.");

            if (request.Items == null || !request.Items.Any())
                errores.Add("La orden debe contener al menos un item.");
            else if (request.Items.Any(i => i.Cantidad <= 0))
                errores.Add("La cantidad de cada item debe ser mayor a 0.");

            if (errores.Count > 0)
                throw new ValidationException("ORD-002", string.Join("; ", errores));
        }


    }
}
