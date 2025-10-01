# DotNet.Web.Api.Template 🚀

A clean, production-ready **.NET 8 Web API** architecture template. It utilizes a **Clean-ish Layered Architecture** for strong separation of concerns, high testability, and maintainability, making it ideal for small-to-medium-sized applications.

---

## ✨ Features & Tech Stack

| Component | Technology / Concept | Details |
| :--- | :--- | :--- |
| **Framework** | **.NET 8 Web API** | Supports both **Controllers** and **Minimal APIs**. |
| **Architecture** | **Clean-ish Layered** | Focuses on separation of concerns (Domain, Application, Infrastructure). |
| **ORM** | **EF Core 8** | Used for data persistence with migrations and Fluent API configurations. |
| **Validation** | **FluentValidation** | Request/DTO validation in the Application layer. |
| **Authentication** | **ASP.NET Core Identity** | Used for user management, configurable with **JWT Bearer**. |
| **Documentation** | **Swashbuckle** | Provides interactive API documentation via **Swagger UI**. |
| **Logging** | **Serilog** | Configured for structured logging. |
| **Testing** | **xUnit + Moq** | Comprehensive Unit and Integration testing setup. |
| **Mapping** | AutoMapper (Optional) | Easily integrated for mapping between DTOs and Entities. |

---

## 🏗 Architecture Overview

The solution is structured into distinct projects (`.csproj`) representing the architectural layers, promoting clear dependency flow (Domain $\rightarrow$ Application $\rightarrow$ Infrastructure $\rightarrow$ Api).

### 📂 Solution Layout

```text
MyApp.sln
├── src/
│   ├── Api/                → ASP.NET Core Web API entry point
│   ├── Application/        → Business logic, use cases, DTOs, interfaces
│   ├── Domain/             → Core entities, value objects, domain logic
│   ├── Infrastructure/     → Data access (EF Core), concrete repositories, external services
│   └── Shared/             → Cross-cutting concerns (exceptions, constants, helpers)
└── tests/
    ├── Api.Tests/
    ├── Application.Tests/
    └── Infrastructure.Tests/
