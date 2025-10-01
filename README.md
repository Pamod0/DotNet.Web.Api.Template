
# DotNet.Web.Api.Template 🚀

A clean, production-ready **.NET 8 Web API** architecture template. It utilizes a **Clean-ish Layered Architecture** for strong separation of concerns, high testability, and maintainability, making it ideal for small-to-medium-sized applications.


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


## 🏗 Architecture Overview (Clean-ish Layered)

```text

src/
 ├── Api/                  → ASP.NET Core Web API (controllers, minimal APIs, filters, DI)
 │    ├── Controllers/
 │    ├── Middlewares/
 │    ├── Extensions/
 │    └── Program.cs
 │
 ├── Application/          → Business logic layer (use cases)
 │    ├── Interfaces/      → Contracts (repositories, services)
 │    ├── DTOs/            → DTOs / requests / responses
 │    ├── Services/        → Business logic / orchestrations
 │    └── Validators/      → FluentValidation
 │
 ├── Domain/               → Core entities & domain logic
 │    ├── Entities/
 │    ├── ValueObjects/
 │    └── Enums/
 │
 ├── Infrastructure/       → External concerns (EF Core, Identity, Logging, 3rd party)
 │    ├── Data/
 │    │    ├── ApplicationDbContext.cs
 │    │    ├── Configurations/   → EF Fluent API configs
 │    ├── Repositories/
 │    └── Services/
 │
 ├── Tests/                → Unit + Integration Tests
 │    ├── Api.Tests/
 │    ├── Application.Tests/
 │    └── Infrastructure.Tests/
 │
 └── Shared/               → Cross-cutting (constants, exceptions, helpers, base classes)

```
# ⚙️ Project Responsibilities

The project is structured into distinct layers, each with specific responsibilities:

* **Domain**
    * Contains core *business entities* (e.g., `User`, `Order`, `Product`).
    * **No dependencies** on infrastructure frameworks like *EF Core* or *ASP.NET Core*.
    * Can include *domain rules*, *enums*, and *value objects*.



* **Application**
    * Houses the *use cases* and *business logic* (e.g., `OrderService`).
    * Defines and uses *interfaces* for repositories and external services (to be implemented in **Infrastructure**).
    * Utilizes *Data Transfer Objects (DTOs)* for communication between layers.
    * Integrates **FluentValidation** for request and command validation.


* **Infrastructure**
    * Implementation of persistence:
        * *EF Core* `DbContext` and *migrations*.
        * Concrete *repository implementations*.
    * Implementations for *external services*:
        * Email sender.
        * File storage.
        * Caching mechanisms.
    * *Identity* and *Authentication/Authorization* setup.


* **Api**
    * The entry point, typically using *ASP.NET Core*:
        * Controllers or Minimal APIs.
    * Configuration and setup:
        * *Dependency Injection* container.
        * *Middleware* pipeline.
    * API-specific features:
        * *Authentication/Authorization* filters.
        * *Exception handling* and *logging*.
        * *Swagger/OpenAPI* documentation.
## 📂 Example Folder Layout (Detailed)

```text

src/
 ├── Api/
 │    ├── Controllers/
 │    │     ├── ProductsController.cs
 │    │     └── AuthController.cs
 │    ├── Middlewares/
 │    │     └── ExceptionMiddleware.cs
 │    ├── Extensions/
 │    │     ├── ServiceCollectionExtensions.cs
 │    │     └── ApplicationBuilderExtensions.cs
 │    └── Program.cs
 │
 ├── Application/
 │    ├── DTOs/
 │    │     ├── ProductDto.cs
 │    │     └── AuthDto.cs
 │    ├── Interfaces/
 │    │     ├── IProductRepository.cs
 │    │     └── IAuthService.cs
 │    ├── Services/
 │    │     ├── ProductService.cs
 │    │     └── AuthService.cs
 │    └── Validators/
 │          └── ProductValidator.cs
 │
 ├── Domain/
 │    ├── Entities/
 │    │     ├── Product.cs
 │    │     └── User.cs
 │    ├── ValueObjects/
 │    │     └── Money.cs
 │    └── Enums/
 │          └── UserRole.cs
 │
 ├── Infrastructure/
 │    ├── Data/
 │    │     ├── ApplicationDbContext.cs
 │    │     └── Configurations/
 │    │          └── ProductConfiguration.cs
 │    ├── Repositories/
 │    │     └── ProductRepository.cs
 │    └── Services/
 │          └── AuthService.cs
 │
 ├── Shared/
 │    ├── Exceptions/
 │    │     └── NotFoundException.cs
 │    ├── Constants/
 │    │     └── AppConstants.cs
 │    └── Helpers/
 │          └── JwtHelper.cs
 │
 └── Tests/
      ├── Api.Tests/
      ├── Application.Tests/
      └── Infrastructure.Tests/

```

## ✅ Run EF Core Migration

Since your **DbContext** is in **Infrastructure** but the application startup configuration is in **Api**, you need to explicitly tell the EF Core tooling where to find both projects.

Use the following commands in the **Package Manager Console** or **CLI**:

```bash
# To create a new migration
Add-Migration InitialCreate -Project Infrastructure -StartupProject Api

# To apply pending migrations to the database
Update-Database -Project Infrastructure -StartupProject Api
```

