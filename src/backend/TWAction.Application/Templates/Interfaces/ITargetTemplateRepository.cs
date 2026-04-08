using TWAction.Domain.Templates;

namespace TWAction.Application.Templates.Interfaces;

/// <summary>
/// Provides data access for <see cref="TargetTemplate"/> entities.
/// </summary>
public interface ITargetTemplateRepository
{
    /// <summary>
    /// Returns all default templates plus templates owned by the specified user.
    /// </summary>
    Task<IEnumerable<TargetTemplate>> GetAllAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Returns a single template by its identifier, or null when not found.</summary>
    Task<TargetTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Persists a new template and returns the saved entity.</summary>
    Task<TargetTemplate> CreateAsync(TargetTemplate template, CancellationToken ct = default);

    /// <summary>Persists changes to an existing template.</summary>
    Task UpdateAsync(TargetTemplate template, CancellationToken ct = default);

    /// <summary>Removes a template permanently.</summary>
    Task DeleteAsync(TargetTemplate template, CancellationToken ct = default);

    /// <summary>Returns true when any default template already exists (used by the seeder).</summary>
    Task<bool> DefaultTemplatesExistAsync(CancellationToken ct = default);
}
