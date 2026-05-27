using Products.API.Services;
using Products.API.DTOs;

namespace Products.API.Controllers;

public static class ProductsEndpoints
{
    public static void MapProductsEndpoints(this WebApplication app)
    {
        app.MapGet("/api/products", async (ProductService service, string? categoria, string? nombre) =>
        {
            var products = await service.GetAllAsync(categoria, nombre);
            return Results.Ok(products);
        });

        app.MapGet("/api/products/{id}", async (ProductService service, Guid id) =>
        {
            var product = await service.GetByIdAsync(id);
            return Results.Ok(product);
        });

        app.MapPost("/api/products", async (ProductService service, CreateProductRequest request) =>
        {
            var product = await service.CreateAsync(request);
            return Results.Created($"/api/products/{product.Id}", product);
        });

        app.MapPut("/api/products/{id}", async (ProductService service, Guid id, UpdateProductRequest request) =>
        {
            var product = await service.UpdateAsync(id, request);
            return Results.Ok(product);
        });

        app.MapDelete("/api/products/{id}", async (ProductService service, Guid id) =>
        {
            await service.DeleteAsync(id);
            return Results.NoContent();
        });
    }
}