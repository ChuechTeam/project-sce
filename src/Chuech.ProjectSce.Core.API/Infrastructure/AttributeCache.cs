using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Reflection;

namespace Chuech.ProjectSce.Core.API.Infrastructure;

public static class AttributeCache<T> where T : Attribute
{
    private static readonly ConcurrentDictionary<Type, ImmutableArray<T>> s_attributes = new();

    public static ImmutableArray<T> Get(Type type)
    {
        return s_attributes.GetOrAdd(type, x => x.GetCustomAttributes<T>().ToImmutableArray());
    }
}