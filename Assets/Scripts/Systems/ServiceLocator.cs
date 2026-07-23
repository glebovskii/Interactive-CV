using System;
using System.Collections.Generic;

public static class ServiceLocator
{
    private static readonly Dictionary<Type, object> Services = new();

    public static void Register<T>(T service)
        where T : class
    {
        if (service == null)
            throw new ArgumentNullException(nameof(service));

        Type type = typeof(T);

        if (!Services.TryAdd(type, service))
        {
            throw new InvalidOperationException($"Service {type.Name} is already registered.");
        }
    }

    public static void RegisterOrReplace<T>(T service)
        where T : class
    {
        if (service == null)
            throw new ArgumentNullException(nameof(service));

        Services[typeof(T)] = service;
    }

    public static T Get<T>()
        where T : class
    {
        if (Services.TryGetValue(typeof(T), out object service))
            return (T)service;

        throw new InvalidOperationException($"Service {typeof(T).Name} is not registered.");
    }

    public static bool TryGet<T>(out T service)
        where T : class
    {
        if (Services.TryGetValue(typeof(T), out object registeredService))
        {
            service = (T)registeredService;
            return true;
        }

        service = null;
        return false;
    }

    public static bool IsRegistered<T>()
        where T : class
    {
        return Services.ContainsKey(typeof(T));
    }

    public static void Unregister<T>()
        where T : class
    {
        Services.Remove(typeof(T));
    }

    public static void Clear()
    {
        Services.Clear();
    }
}