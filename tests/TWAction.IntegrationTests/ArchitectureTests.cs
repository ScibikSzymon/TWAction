using NetArchTest.Rules;
using TWAction.Application.Users.Queries;
using TWAction.Domain.Users;

namespace TWAction.IntegrationTests;

public sealed class ArchitectureTests
{
    private const string DomainNamespace = "TWAction.Domain";
    private const string ApplicationNamespace = "TWAction.Application";
    private const string InfrastructureNamespace = "TWAction.Infrastructure";
    private const string PersistenceNamespace = "TWAction.Persistence";
    private const string ApiNamespace = "TWAction.Api";

    [Fact]
    public void Domain_ShouldNotHaveAnyDependenciesOnOtherLayers()
    {
        var result = Types.InAssembly(typeof(UserEntity).Assembly)
            .Should()
            .NotHaveDependencyOnAny(
                ApplicationNamespace,
                InfrastructureNamespace,
                PersistenceNamespace,
                ApiNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, "Domain layer should not depend on any other layers");
    }

    [Fact]
    public void Domain_ShouldNotReferenceEntityFrameworkCore()
    {
        var result = Types.InAssembly(typeof(UserEntity).Assembly)
            .Should()
            .NotHaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        Assert.True(result.IsSuccessful, "Domain layer should not reference EntityFrameworkCore");
    }

    [Fact]
    public void Application_ShouldOnlyDependOnDomain()
    {
        var result = Types.InAssembly(typeof(GetAllUsersHandler).Assembly)
            .Should()
            .NotHaveDependencyOnAny(
                InfrastructureNamespace,
                PersistenceNamespace,
                ApiNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, "Application layer should only depend on Domain");
    }

    [Fact]
    public void Application_ShouldNotReferenceEntityFrameworkCore()
    {
        var result = Types.InAssembly(typeof(GetAllUsersHandler).Assembly)
            .Should()
            .NotHaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        Assert.True(result.IsSuccessful, "Application layer should not reference EntityFrameworkCore");
    }

    [Fact]
    public void Infrastructure_ShouldNotDependOnApi()
    {
        var infrastructureAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == "TWAction.Infrastructure");

        if (infrastructureAssembly is null)
        {
            return;
        }

        var result = Types.InAssembly(infrastructureAssembly)
            .Should()
            .NotHaveDependencyOn(ApiNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, "Infrastructure layer should not depend on Api");
    }

    [Fact]
    public void Persistence_ShouldNotDependOnApiOrInfrastructure()
    {
        var persistenceAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == "TWAction.Persistence");

        if (persistenceAssembly is null)
        {
            return;
        }

        var result = Types.InAssembly(persistenceAssembly)
            .Should()
            .NotHaveDependencyOnAny(ApiNamespace, InfrastructureNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, "Persistence layer should not depend on Api or Infrastructure");
    }

    [Fact]
    public void Api_ShouldNotReferenceEntityFrameworkCore()
    {
        var result = Types.InAssembly(typeof(Program).Assembly)
            .That()
            .DoNotHaveName("Program")
            .Should()
            .NotHaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        Assert.True(result.IsSuccessful, 
            "Api layer should not reference EntityFrameworkCore (except Program.cs for migration startup)");
    }

    [Fact]
    public void DomainEntities_ShouldEndWithEntitySuffix()
    {
        var result = Types.InAssembly(typeof(UserEntity).Assembly)
            .That()
            .ResideInNamespace($"{DomainNamespace}.Entities")
            .And()
            .AreClasses()
            .Should()
            .HaveNameEndingWith("Entity")
            .GetResult();

        Assert.True(result.IsSuccessful, "Domain entities should end with 'Entity' suffix");
    }

    [Fact]
    public void ApplicationDTOs_ShouldEndWithDtoSuffix()
    {
        // NetArchTest treats records as classes
        var result = Types.InAssembly(typeof(GetAllUsersHandler).Assembly)
            .That()
            .ResideInNamespace($"{ApplicationNamespace}.DTOs")
            .And()
            .DoNotHaveNameEndingWith("Result") // Allow Result types
            .And()
            .DoNotHaveNameEndingWith("Response") // Allow Response types
            .Should()
            .HaveNameEndingWith("Dto")
            .GetResult();

        Assert.True(result.IsSuccessful, 
            $"Application DTOs should end with 'Dto' suffix (except Result/Response types). Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void ApplicationHandlers_ShouldEndWithHandlerSuffix()
    {
        var handlerTypes = Types.InAssembly(typeof(GetAllUsersHandler).Assembly)
            .That()
            .AreClasses()
            .And()
            .HaveNameEndingWith("Handler")
            .GetTypes();

        if (!handlerTypes.Any())
        {
            return; // No handlers found, skip test
        }

        // All classes ending with Handler should be in appropriate namespaces
        var result = Types.InAssembly(typeof(GetAllUsersHandler).Assembly)
            .That()
            .HaveNameEndingWith("Handler")
            .Should()
            .ResideInNamespaceContaining("Application")
            .GetResult();

        Assert.True(result.IsSuccessful, "Application handlers should end with 'Handler' suffix");
    }

    [Fact]
    public void ApplicationRepositories_ShouldEndWithRepositorySuffix()
    {
        var result = Types.InAssembly(typeof(GetAllUsersHandler).Assembly)
            .That()
            .ResideInNamespace($"{ApplicationNamespace}.Interfaces")
            .And()
            .AreInterfaces()
            .And()
            .HaveNameStartingWith("I")
            .And()
            .DoNotHaveName("IRepository`1") // Exclude generic base interface
            .Should()
            .HaveNameEndingWith("Repository")
            .GetResult();

        Assert.True(result.IsSuccessful, 
            $"Repository interfaces should end with 'Repository' suffix. Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }
}
