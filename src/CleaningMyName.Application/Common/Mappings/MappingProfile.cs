using AutoMapper;
using System.Reflection;

namespace CleaningMyName.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        ApplyMappingsFromAssembly(Assembly.GetExecutingAssembly());
    }

    private void ApplyMappingsFromAssembly(Assembly assembly)
    {
        var types = assembly.GetExportedTypes()
            .Where(t => t.GetInterfaces().Any(i => 
                i.IsGenericType && (
                    i.GetGenericTypeDefinition() == typeof(IMapFrom<>) ||
                    i.GetGenericTypeDefinition() == typeof(IMapTo<>))))
            .ToList();

        foreach (var type in types)
        {
            var instance = Activator.CreateInstance(type);

            var methodInfoFrom = type.GetMethod("Mapping") 
                ?? type.GetInterface("IMapFrom`1")?.GetMethod("Mapping");
            
            var methodInfoTo = type.GetMethod("Mapping") 
                ?? type.GetInterface("IMapTo`1")?.GetMethod("Mapping");

            methodInfoFrom?.Invoke(instance, new object[] { this });
            methodInfoTo?.Invoke(instance, new object[] { this });
        }
    }
}
