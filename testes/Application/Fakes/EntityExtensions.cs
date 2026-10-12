using System.Reflection;

namespace Prescription.Tests.Application.Fakes;

/// <summary>
/// Sets private properties (database generated ids and navigations), as EF Core does when loading.
/// </summary>
internal static class EntityExtensions
{
    public static T With<T>(this T entity, string property, object? value) where T : class
    {
        var info = entity.GetType().GetProperty(property, BindingFlags.Instance | BindingFlags.Public)
                   ?? throw new InvalidOperationException($"{entity.GetType().Name}.{property} not found.");
        info.SetValue(entity, value);
        return entity;
    }

    public static T WithId<T>(this T entity, uint id) where T : class => entity.With("Id", id);
}
