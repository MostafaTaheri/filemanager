using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Service.Interfaces;
using Service.Repositories;

namespace Service.Extensions;

public static class ServiceInjectExtension
{

    public static List<Type> GetTypesAssignableFrom<T>(this Assembly assembly)
    {
        var data = assembly.GetTypesAssignableFrom(typeof(T));
        return data;
    }
    public static List<Type> GetTypesAssignableFrom(this Assembly assembly, Type compareType)
    {
        List<Type> ret = new List<Type>();
        foreach (var type in assembly.DefinedTypes)
        {
            if (compareType.IsAssignableFrom(type) && compareType != type)
            {
                ret.Add(type);
            }
        }
        return ret;
    }
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IFileManagerService, FileManagerService>();

        return services;
    }

}