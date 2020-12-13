using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// The service locator is responsible for handling all services in the project
/// </summary>
public static class ServiceLocator
{
    private static readonly Dictionary<Type, object> instantiatedServices = new Dictionary<Type, object>();

    public static T Locate<T>()
    {
        Type type = typeof(T);

        if (!Attribute.IsDefined(type, typeof(ServiceAttribute)))
        {
            throw new Exception("This class is not a service");
        }

        if (instantiatedServices.ContainsKey(type))
            return (T) instantiatedServices[type];

        if (typeof(ScriptableObject).IsAssignableFrom(type))
            return LocateScriptableObject<T>();
        else
            return LocateGenericScript<T>();
    }

    private static T LocateScriptableObject<T>()
    {
        Object[] scriptableObject = Resources.LoadAll("", typeof(T));
        if (scriptableObject.Length == 0)
            throw new Exception("Could not find scriptable object, is it in the Resources folder?");
        var obj = Convert.ChangeType(scriptableObject[0], typeof(T));
        instantiatedServices.Add(typeof(T), obj);
        return (T) obj;
    }

    private static T LocateGenericScript<T>()
    {
        if (typeof(T).IsInterface)
            throw new Exception("Interfaces are not supported yet.");
        var t = Activator.CreateInstance<T>();
        instantiatedServices.Add(typeof(T), t);
        return t;
    }
}