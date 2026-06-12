using Products.API.DTOs;
using Products.API.Models;
using Products.API.Services;
using Microsoft.OpenApi.Any;

namespace Products.API.Controllers;

public static class ProductsEndpoints
{
    private static readonly OpenApiObject EjemploProducto = new()
    {
        ["id"] = new OpenApiString("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
        ["nombre"] = new OpenApiString("Notebook Dell XPS 15"),
        ["descripcion"] = new OpenApiString("Laptop 15 pulgadas, 32GB RAM"),
        ["precio"] = new OpenApiDouble(1500.00),
        ["stock"] = new OpenApiInteger(10),
        ["categoria"] = new OpenApiString("Electrónica"),
        ["fechaCreacion"] = new OpenApiString("2024-01-15T10:30:00Z")
    };

    private static OpenApiObject ErrorPRD(string type, string title, int status, string detail, string instance, string code, string message) => new()
    {
        ["type"] = new OpenApiString(type),
        ["title"] = new OpenApiString(title),
        ["status"] = new OpenApiInteger(status),
        ["detail"] = new OpenApiString(detail),
        ["instance"] = new OpenApiString(instance),
        ["errorCode"] = new OpenApiString(code),
        ["errorMessage"] = new OpenApiString(message)
    };

    public static void MapProductsEndpoints(this WebApplication app)
    {
        // GET /api/products
        app.MapGet("/api/products", async (ProductService service, string? categoria, string? nombre) =>
        {
            var products = await service.GetAllAsync(categoria, nombre);
            return Results.Ok(products);
        })
        .WithTags("Products")
        .WithSummary("Listar productos")
        .WithDescription("Retorna todos los productos. Se puede filtrar por categoría y/o nombre.")
        .Produces<IEnumerable<Product>>(200)
        .Produces<ErrorResponse>(500)
        .WithOpenApi(op =>
        {
            op.Responses["200"].Description = "Lista de productos";
            op.Responses["200"].Content["application/json"].Example = new OpenApiArray
            {
                EjemploProducto
            };
            op.Responses["500"].Description = "Error interno (PRD-005)";
            op.Responses["500"].Content["application/json"].Example = ErrorPRD(
                "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                "Internal Server Error", 500,
                "Ocurrió un error interno.",
                "/api/products", "PRD-005", "Error interno al procesar el producto.");
            return op;
        });

        // GET /api/products/{id}
        app.MapGet("/api/products/{id}", async (ProductService service, Guid id) =>
        {
            var product = await service.GetByIdAsync(id);
            return Results.Ok(product);
        })
        .WithTags("Products")
        .WithSummary("Obtener producto por ID")
        .WithDescription("Retorna un producto específico según su identificador único.")
        .Produces<Product>(200)
        .Produces<ErrorResponse>(404)
        .Produces<ErrorResponse>(500)
        .WithOpenApi(op =>
        {
            op.Responses["200"].Description = "Producto encontrado";
            op.Responses["200"].Content["application/json"].Example = EjemploProducto;

            op.Responses["404"].Description = "Producto no encontrado (PRD-001)";
            op.Responses["404"].Content["application/json"].Example = ErrorPRD(
                "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                "Not Found", 404,
                "El recurso solicitado no fue encontrado.",
                "/api/products/99", "PRD-001", "Producto no encontrado.");

            op.Responses["500"].Description = "Error interno (PRD-005)";
            op.Responses["500"].Content["application/json"].Example = ErrorPRD(
                "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                "Internal Server Error", 500,
                "Ocurrió un error interno.",
                "/api/products/3fa85f64-5717-4562-b3fc-2c963f66afa6", "PRD-005", "Error interno al procesar el producto.");
            return op;
        });

        // POST /api/products
        app.MapPost("/api/products", async (ProductService service, CreateProductRequest request) =>
        {
            var product = await service.CreateAsync(request);
            return Results.Created($"/api/products/{product.Id}", product);
        })
        .WithTags("Products")
        .WithSummary("Crear producto")
        .WithDescription("Crea un nuevo producto en el catálogo.")
        .Produces<Product>(201)
        .Produces<ErrorResponse>(400)
        .Produces<ErrorResponse>(409)
        .Produces<ErrorResponse>(500)
        .WithOpenApi(op =>
        {
            op.Responses["201"].Description = "Producto creado exitosamente";
            op.Responses["201"].Content["application/json"].Example = EjemploProducto;

            op.Responses["400"].Description = "Datos inválidos (PRD-002)";
            op.Responses["400"].Content["application/json"].Example = ErrorPRD(
                "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                "Bad Request", 400,
                "Los datos enviados son inválidos.",
                "/api/products", "PRD-002", "Los datos del producto son inválidos.");

            op.Responses["409"].Description = "Producto duplicado en la categoría (PRD-003)";
            op.Responses["409"].Content["application/json"].Example = ErrorPRD(
                "https://tools.ietf.org/html/rfc7231#section-6.5.9",
                "Conflict", 409,
                "Ya existe un recurso con esos datos.",
                "/api/products", "PRD-003", "Ya existe un producto con ese nombre en la categoría 'Electrónica'.");

            op.Responses["500"].Description = "Error interno (PRD-005)";
            op.Responses["500"].Content["application/json"].Example = ErrorPRD(
                "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                "Internal Server Error", 500,
                "Ocurrió un error interno.",
                "/api/products", "PRD-005", "Error interno al procesar el producto.");
            return op;
        });

        // PUT /api/products/{id}
        app.MapPut("/api/products/{id}", async (ProductService service, Guid id, UpdateProductRequest request) =>
        {
            var product = await service.UpdateAsync(id, request);
            return Results.Ok(product);
        })
        .WithTags("Products")
        .WithSummary("Actualizar producto")
        .WithDescription("Actualiza los datos de un producto existente.")
        .Produces<Product>(200)
        .Produces<ErrorResponse>(400)
        .Produces<ErrorResponse>(404)
        .Produces<ErrorResponse>(500)
        .WithOpenApi(op =>
        {
            op.Responses["200"].Description = "Producto actualizado exitosamente";
            op.Responses["200"].Content["application/json"].Example = new OpenApiObject
            {
                ["id"] = new OpenApiString("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                ["nombre"] = new OpenApiString("Notebook Dell XPS 15"),
                ["descripcion"] = new OpenApiString("Laptop 15 pulgadas, 64GB RAM"),
                ["precio"] = new OpenApiDouble(1750.00),
                ["stock"] = new OpenApiInteger(8),
                ["categoria"] = new OpenApiString("Electrónica"),
                ["fechaCreacion"] = new OpenApiString("2024-01-15T10:30:00Z")
            };

            op.Responses["400"].Description = "Datos inválidos (PRD-002)";
            op.Responses["400"].Content["application/json"].Example = ErrorPRD(
                "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                "Bad Request", 400,
                "Los datos enviados son inválidos.",
                "/api/products/3fa85f64-5717-4562-b3fc-2c963f66afa6", "PRD-002", "Los datos del producto son inválidos.");

            op.Responses["404"].Description = "Producto no encontrado (PRD-001)";
            op.Responses["404"].Content["application/json"].Example = ErrorPRD(
                "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                "Not Found", 404,
                "El recurso solicitado no fue encontrado.",
                "/api/products/3fa85f64-5717-4562-b3fc-2c963f66afa6", "PRD-001", "Producto no encontrado.");

            op.Responses["500"].Description = "Error interno (PRD-005)";
            op.Responses["500"].Content["application/json"].Example = ErrorPRD(
                "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                "Internal Server Error", 500,
                "Ocurrió un error interno.",
                "/api/products/3fa85f64-5717-4562-b3fc-2c963f66afa6", "PRD-005", "Error interno al procesar el producto.");
            return op;
        });

        // DELETE /api/products/{id}
        app.MapDelete("/api/products/{id}", async (HttpContext http, ProductService service, Guid id) =>
        {
            var correlationId = http.Items["X-Correlation-Id"]?.ToString();
            await service.DeleteAsync(id, correlationId);
            return Results.NoContent();
        })
        .WithTags("Products")
        .WithSummary("Eliminar producto")
        .WithDescription("Elimina un producto del catálogo. No se puede eliminar si tiene órdenes activas.")
        .Produces(204)
        .Produces<ErrorResponse>(404)
        .Produces<ErrorResponse>(409)
        .Produces<ErrorResponse>(500)
        .WithOpenApi(op =>
        {
            op.Responses["204"].Description = "Producto eliminado exitosamente";

            op.Responses["404"].Description = "Producto no encontrado (PRD-001)";
            op.Responses["404"].Content["application/json"].Example = ErrorPRD(
                "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                "Not Found", 404,
                "El recurso solicitado no fue encontrado.",
                "/api/products/3fa85f64-5717-4562-b3fc-2c963f66afa6", "PRD-001", "Producto no encontrado.");

            op.Responses["409"].Description = "Producto con órdenes activas (PRD-004)";
            op.Responses["409"].Content["application/json"].Example = ErrorPRD(
                "https://tools.ietf.org/html/rfc7231#section-6.5.9",
                "Conflict", 409,
                "No se puede eliminar el recurso.",
                "/api/products/3fa85f64-5717-4562-b3fc-2c963f66afa6", "PRD-004", "El producto tiene órdenes activas y no puede eliminarse.");

            op.Responses["500"].Description = "Error interno (PRD-005)";
            op.Responses["500"].Content["application/json"].Example = ErrorPRD(
                "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                "Internal Server Error", 500,
                "Ocurrió un error interno.",
                "/api/products/3fa85f64-5717-4562-b3fc-2c963f66afa6", "PRD-005", "Error interno al procesar el producto.");
            return op;
        });
    }
}