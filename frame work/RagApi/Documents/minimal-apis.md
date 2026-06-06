# ASP.NET Core Minimal APIs

## What are Minimal APIs?

Minimal APIs were introduced in ASP.NET Core 6 as a simplified approach to building HTTP APIs. They reduce the ceremony and boilerplate code required compared to traditional Controller-based APIs, making it faster to create lightweight services and microservices.

## Why Choose Minimal APIs?

### Reduced Boilerplate

A complete API can be created in just a few lines of code. There's no need for controller classes, attributes, or separate startup configuration. The entire application can live in a single `Program.cs` file for simple scenarios.

### Faster Startup

Minimal APIs have lower overhead than controller-based APIs because they skip the MVC middleware pipeline. This results in faster application startup times and slightly better request throughput for simple endpoints.

### Perfect for Microservices

When building microservices that expose only a handful of endpoints, Minimal APIs provide the right level of abstraction without unnecessary complexity. They work well with containers and serverless deployments.

## Basic Endpoint Definitions

### GET Endpoint

```csharp
app.MapGet("/products", async (ProductDbContext db) =>
    await db.Products.ToListAsync());

app.MapGet("/products/{id}", async (int id, ProductDbContext db) =>
    await db.Products.FindAsync(id) is Product product
        ? Results.Ok(product)
        : Results.NotFound());
```

### POST Endpoint

```csharp
app.MapPost("/products", async (Product product, ProductDbContext db) =>
{
    db.Products.Add(product);
    await db.SaveChangesAsync();
    return Results.Created($"/products/{product.Id}", product);
});
```

### PUT Endpoint

```csharp
app.MapPut("/products/{id}", async (int id, Product input, ProductDbContext db) =>
{
    var product = await db.Products.FindAsync(id);
    if (product is null) return Results.NotFound();

    product.Name = input.Name;
    product.Price = input.Price;
    await db.SaveChangesAsync();

    return Results.NoContent();
});
```

### DELETE Endpoint

```csharp
app.MapDelete("/products/{id}", async (int id, ProductDbContext db) =>
{
    if (await db.Products.FindAsync(id) is Product product)
    {
        db.Products.Remove(product);
        await db.SaveChangesAsync();
        return Results.Ok(product);
    }
    return Results.NotFound();
});
```

## Parameter Binding

Minimal APIs automatically bind parameters from various sources:

### Route Parameters

```csharp
app.MapGet("/users/{id}", (int id) => $"User {id}");
```

### Query String Parameters

```csharp
app.MapGet("/search", (string query, int page = 1) => $"Searching for {query}, page {page}");
```

### Request Body (JSON)

```csharp
app.MapPost("/orders", (OrderRequest order) => Results.Ok(order));
```

### Dependency Injection

Services registered in the DI container are automatically injected:

```csharp
app.MapGet("/weather", (IWeatherService weatherService) =>
    weatherService.GetForecast());
```

### Special Types

- `HttpContext` — the full HTTP context
- `HttpRequest` / `HttpResponse` — request and response objects
- `CancellationToken` — for async cancellation
- `ClaimsPrincipal` — the authenticated user

## Route Groups

Route groups allow you to organize endpoints with shared prefixes and filters:

```csharp
var api = app.MapGroup("/api/v1");
var products = api.MapGroup("/products").RequireAuthorization();

products.MapGet("/", GetAllProducts);
products.MapGet("/{id}", GetProduct);
products.MapPost("/", CreateProduct);
```

## Filters and Middleware

### Endpoint Filters

Endpoint filters run before and after the endpoint handler, similar to action filters in MVC:

```csharp
app.MapPost("/products", CreateProduct)
    .AddEndpointFilter(async (context, next) =>
    {
        var product = context.GetArgument<Product>(0);
        if (string.IsNullOrEmpty(product.Name))
            return Results.BadRequest("Product name is required");
        return await next(context);
    });
```

### Rate Limiting

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 100;
    });
});

app.MapGet("/api/data", () => "Hello").RequireRateLimiting("fixed");
```

## OpenAPI / Swagger Integration

Minimal APIs integrate with Swagger/OpenAPI through extension methods:

```csharp
app.MapPost("/chat", HandleChat)
    .WithName("Chat")
    .WithTags("AI")
    .WithOpenApi()
    .Produces<ChatResponse>(200)
    .Produces(400);
```

Adding `builder.Services.AddEndpointsApiExplorer()` and `builder.Services.AddSwaggerGen()` enables automatic API documentation generation.

## Validation

For input validation, you can use endpoint filters or libraries like FluentValidation:

```csharp
app.MapPost("/users", async (CreateUserRequest request, IValidator<CreateUserRequest> validator) =>
{
    var result = await validator.ValidateAsync(request);
    if (!result.IsValid)
        return Results.ValidationProblem(result.ToDictionary());

    // Process valid request
    return Results.Created();
});
```

## Best Practices

1. **Use TypedResults** for better OpenAPI metadata generation
2. **Organize endpoints** using route groups and extension methods
3. **Separate concerns** by moving handler logic into dedicated service classes
4. **Add validation** at the endpoint level using filters
5. **Document endpoints** with `.WithOpenApi()`, `.WithName()`, and `.WithTags()`
6. **Use async/await** consistently for all I/O operations
7. **Return appropriate status codes** using the `Results` or `TypedResults` helper classes
