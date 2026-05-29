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
        .Produces<IEnumerable<Product>>(200)
        .Produces<ErrorResponse>(500);

        app.MapGet("/api/products/{id}", async (ProductService service, Guid id) =>
        {
            var product = await service.GetByIdAsync(id);
            return Results.Ok(product);
        })
        .WithTags("Products")
        .Produces<Product>(200)
        .Produces<ErrorResponse>(404)
        .Produces<ErrorResponse>(500);

        app.MapPost("/api/products", async (ProductService service, CreateProductRequest request) =>
        {
            var product = await service.CreateAsync(request);
            return Results.Created($"/api/products/{product.Id}", product);
        })
        .WithTags("Products")
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
        .Produces(204)
        .Produces<ErrorResponse>(404)
        .Produces<ErrorResponse>(409)
        .Produces<ErrorResponse>(500);
    }
}