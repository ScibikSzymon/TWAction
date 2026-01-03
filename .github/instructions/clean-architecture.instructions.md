---
applyTo: '**/*.cs'
---

# Clean Architecture

When implementing backend services, follow these Clean Architecture principles to ensure maintainability, scalability, and separation of concerns. This rule is tailored for .NET solutions with a multi-project structure.

## 1. Solution Structure

- The solution **must** be organized into four main projects (one per layer):
  - `[project].Domain` (core business logic, entities, value objects, domain events)
  - `[project].Application` (use cases, commands, queries, interfaces for external services)
  - `[project].Infrastructure` (implementations for external services, database access, third-party integrations)
  - `[project].Api` (API endpoints, minimal API, request/response models)
- Each project must contain a marker/reference file (e.g., `DomainReference.cs`) for test discovery and architecture validation.
- Tests must be in separate projects:
  - `tests/[project].UnitTests` (for Domain and Application)
  - `tests/[project].IntegrationTests` (for Infrastructure, Api, and architecture validation)

## 2. Dependencies Between Layers

- **Domain**: has no dependencies.
- **Application**: depends only on **Domain**.
- **Infrastructure**: depends on **Application** and **Domain**.
- **Api**: depends only on **Infrastructure**.
- These dependencies **must** be enforced by automated architecture tests (e.g., NetArchTest in `ArchitectureTests.cs`).
- Forbidden dependencies (e.g., EntityFrameworkCore in Api/Domain) must be checked by tests.

## 3. Folder and File Structure

- Use a **feature-oriented** (domain-driven) folder structure in each layer (e.g., `Order/`, `Customer/`).
- Do **not** use technical root folders (Entities, ValueObjects, Services, etc.).
- Example minimal structure:

```
src/
  [project].Domain/
    DomainReference.cs
    Order/ 
      Order.cs
      OrderCreatedEvent.cs
    Customer/
      Customer.cs
  [project].Application/
    ApplicationReference.cs
    Order/
      ...
    Customer/
      ...
  [project].Infrastructure/
    InfrastructureReference.cs
    Order/
      ...
    Customer/
      ...
  [project].Api/
    Program.cs
    ...
tests/
  [project].UnitTests/
    ...
  [project].IntegrationTests/
    ArchitectureTests.cs
    ...
```

## 4. Coding Style and Conventions

- Use file-scoped namespaces.
- One type per file.
- Follow Microsoft .NET C# coding conventions.
- Organize files by feature/domain.

## 5. Implementation Guidelines

- **Domain Layer**: All business logic, entities, value objects, and domain events. No dependencies on other layers.
- **Application Layer**: Use cases, commands, queries, interfaces for repositories/services. No business logic.
- **Infrastructure Layer**: Implementations for interfaces, database access, external integrations. No business logic.
- **Api Layer**: Minimal API endpoints, request/response mapping. No business logic.
- Use dependency injection for all cross-layer dependencies.
- Avoid circular dependencies.
- Do not use a mediator library; call service methods directly from the Api layer.

## 6. Testing and Architecture Validation

- **Unit Tests**: In `tests/[project].UnitTests/`, for Domain and Application layers only. Use xUnit v3 and FakeItEasy for mocks.
- **Integration Tests**: In `tests/[project].IntegrationTests/`, for Infrastructure and Api layers. Use Testcontainers/Microcks for advanced scenarios.
- **Architecture Tests**: Must be present in `ArchitectureTests.cs` and:
  - Enforce allowed/forbidden dependencies between layers
  - Check for forbidden dependencies (e.g., EF Core in Api/Domain)
  - Optionally, check for immutability in Domain
- Always write tests before implementation (TDD).

## 7. Architecture Testing Example

To enforce and validate architecture rules, add automated tests in `tests/[project].IntegrationTests/ArchitectureTests.cs` using [NetArchTest](https://github.com/BenMorris/NetArchTest).
- Adapt namespaces, assemblies, and rules to your solution.
- Add tests to check for forbidden dependencies (e.g., EntityFrameworkCore in Api/Domain) and for immutability in Domain types if relevant.
- Run these tests with `dotnet test` to ensure architecture rules are enforced after every change.

# References
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/TheCleanArchitecture.html)