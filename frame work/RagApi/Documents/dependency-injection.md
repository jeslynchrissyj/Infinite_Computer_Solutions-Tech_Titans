# Dependency Injection in .NET

## What is Dependency Injection?

Dependency Injection (DI) is a software design pattern that implements Inversion of Control (IoC) for resolving dependencies. Instead of a class creating its own dependencies, they are provided (injected) from the outside, typically by a DI container.

## Why Use Dependency Injection?

Dependency injection provides several important benefits:

### Loose Coupling

Classes depend on abstractions (interfaces) rather than concrete implementations. This means you can swap implementations without changing the consuming code. For example, switching from a SQL Server repository to a PostgreSQL repository requires zero changes to your service layer.

### Testability

With DI, you can easily replace real dependencies with mocks or stubs in unit tests. This isolation makes tests more reliable and faster to execute. Without DI, testing often requires complex setup of database connections and external services.

### Maintainability

DI promotes the Single Responsibility Principle by encouraging smaller, focused classes. Each class declares its dependencies explicitly through its constructor, making the codebase easier to understand and maintain.

### Lifetime Management

The DI container manages the lifetime of objects, ensuring proper creation and disposal. This prevents common bugs like memory leaks from undisposed resources or thread-safety issues from shared mutable state.

## Service Lifetimes in ASP.NET Core

ASP.NET Core's built-in DI container supports three service lifetimes:

### Transient

- Created each time they are requested
- Best for lightweight, stateless services
- Registered with `AddTransient<TService, TImplementation>()`
- Example: A service that generates unique IDs or formats data

### Scoped

- Created once per HTTP request (or scope)
- Shared within a single request but not across requests
- Registered with `AddScoped<TService, TImplementation>()`
- Example: Entity Framework DbContext, unit-of-work patterns
- **Important**: Do not resolve scoped services from a singleton

### Singleton

- Created only once for the lifetime of the application
- Shared across all requests and threads
- Registered with `AddSingleton<TService, TImplementation>()`
- Example: Caching services, configuration providers, connection pools
- **Warning**: Must be thread-safe since multiple threads access them concurrently

## Registration Patterns

### Interface-Based Registration

The most common pattern registers an interface and its implementation:

```csharp
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddSingleton<ICacheService, RedisCacheService>();
```

### Factory Registration

Use a factory when you need more control over construction:

```csharp
builder.Services.AddScoped<IReportService>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var logger = provider.GetRequiredService<ILogger<ReportService>>();
    return new ReportService(config["ReportPath"], logger);
});
```

### Multiple Implementations

You can register multiple implementations of the same interface:

```csharp
builder.Services.AddScoped<INotificationService, EmailNotificationService>();
builder.Services.AddScoped<INotificationService, SmsNotificationService>();
```

Inject all implementations using `IEnumerable<INotificationService>`.

### Keyed Services (.NET 8+)

.NET 8 introduced keyed services for named registrations:

```csharp
builder.Services.AddKeyedScoped<IPaymentService, StripePaymentService>("stripe");
builder.Services.AddKeyedScoped<IPaymentService, PayPalPaymentService>("paypal");
```

## Constructor Injection

Constructor injection is the recommended approach in ASP.NET Core:

```csharp
public class OrderService
{
    private readonly IOrderRepository _repository;
    private readonly ILogger<OrderService> _logger;
    private readonly IEmailService _emailService;

    public OrderService(
        IOrderRepository repository,
        ILogger<OrderService> logger,
        IEmailService emailService)
    {
        _repository = repository;
        _logger = logger;
        _emailService = emailService;
    }
}
```

The DI container automatically resolves and injects all constructor parameters when creating an instance of `OrderService`.

## Common Pitfalls

### Captive Dependency

A captive dependency occurs when a longer-lived service captures a shorter-lived dependency. For example, a singleton service that depends on a scoped service will keep using the same scoped instance for the application's lifetime, which can cause data corruption or stale data.

### Service Locator Anti-Pattern

Avoid injecting `IServiceProvider` and resolving services manually. This hides dependencies and makes the code harder to test and understand. Instead, declare dependencies explicitly in the constructor.

### Circular Dependencies

If Service A depends on Service B, and Service B depends on Service A, the DI container will throw an exception. Break circular dependencies by introducing an intermediary service or using lazy resolution with `Lazy<T>`.
