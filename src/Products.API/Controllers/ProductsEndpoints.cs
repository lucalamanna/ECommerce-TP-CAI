using Products.API.DTOs;
using Products.API.Models;
using Products.API.Services;

namespace Products.API.Controllers;

public static class ProductsEndpoints
{
    public static void MapProductsEndpoints(this WebApplication app)
    {
        app.MapGet("/api/products", async (ProductService service, string? categoria, string? nombre) =>
        {
            var products = await service.GetAllAsync(categoria, nombre);
            return Results.Ok(products);
        })
.WithTags("Products")
.WithSummary("Listar productos")
.WithDescription("Retorna todos los productos. Se puede filtrar por categoría y/o nombre.")
.Produces<IEnumerable<Product>>(200)
.Produces<ErrorResponse>(500);

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
        .Produces<ErrorResponse>(500);

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
        .Produces<ErrorResponse>(500);

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
        .Produces<ErrorResponse>(500);

        app.MapDelete("/api/products/{id}", async (ProductService service, Guid id) =>
        {
            await service.DeleteAsync(id);
            return Results.NoContent();
        })
        .WithTags("Products")
        .WithSummary("Eliminar producto")
        .WithDescription("Elimina un producto del catálogo. No se puede eliminar si tiene órdenes activas.")
        .Produces(204)
        .Produces<ErrorResponse>(404)
        .Produces<ErrorResponse>(409)
        .Produces<ErrorResponse>(500);
    }
}