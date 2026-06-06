# ASP.NET Core Overview

## What is ASP.NET Core?

ASP.NET Core is a cross-platform, high-performance, open-source framework for building modern, cloud-enabled, Internet-connected applications. It was redesigned from the ground up to be modular, lightweight, and fast.

## Key Features

### Cross-Platform Support

ASP.NET Core runs on Windows, macOS, and Linux. You can develop and deploy ASP.NET Core applications on any of these operating systems. This makes it an excellent choice for organizations that use diverse server environments.

### High Performance

ASP.NET Core is one of the fastest web frameworks available. It consistently ranks among the top performers in the TechEmpower benchmarks. The framework achieves this through:

- Asynchronous programming patterns built into the framework
- Efficient memory management with low allocation rates
- Kestrel, a high-performance cross-platform web server
- Support for HTTP/2 and gRPC

### Modular Architecture

Unlike its predecessor ASP.NET, the Core version uses a modular architecture where you only include the packages you need. This results in smaller application footprints and better performance. The middleware pipeline allows you to compose your request-handling logic from discrete, reusable components.

### Built-in Dependency Injection

ASP.NET Core includes a built-in dependency injection (DI) container that supports constructor injection out of the box. This promotes loose coupling, testability, and clean architecture patterns. Services can be registered with different lifetimes: Singleton, Scoped, and Transient.

## Application Types

You can build several types of applications with ASP.NET Core:

- **Web APIs**: RESTful services using Controllers or Minimal APIs
- **Web Applications**: Server-rendered pages using Razor Pages or MVC
- **Real-time Applications**: Using SignalR for WebSocket communication
- **gRPC Services**: High-performance RPC using Protocol Buffers
- **Blazor Applications**: Interactive web UIs using C# instead of JavaScript

## Configuration System

ASP.NET Core uses a flexible configuration system that can read settings from multiple sources:

- `appsettings.json` files
- Environment variables
- Command-line arguments
- Azure Key Vault
- User secrets (for development)

The configuration system supports environment-specific overrides. For example, `appsettings.Development.json` will override values from `appsettings.json` when running in the Development environment.

## Middleware Pipeline

The ASP.NET Core request pipeline consists of a sequence of middleware components. Each component can:

1. Choose whether to pass the request to the next component
2. Perform work before and after the next component
3. Short-circuit the pipeline

Common middleware includes authentication, authorization, CORS, static files, routing, and exception handling. The order in which middleware is added matters and affects how requests are processed.

## Hosting Models

ASP.NET Core applications can be hosted in several ways:

- **Kestrel**: A cross-platform web server included with ASP.NET Core
- **IIS**: Internet Information Services on Windows
- **Nginx**: As a reverse proxy on Linux
- **Docker**: Containerized deployment
- **Azure App Service**: Cloud hosting with automatic scaling

## Security Features

ASP.NET Core provides comprehensive security features:

- Authentication with support for JWT, cookies, OAuth, and OpenID Connect
- Authorization with policy-based and role-based access control
- Data protection APIs for encryption
- HTTPS enforcement and HSTS headers
- Anti-forgery token support for CSRF protection
- CORS (Cross-Origin Resource Sharing) configuration
