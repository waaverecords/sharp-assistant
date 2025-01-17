using System.Reflection;

public static class ActionHelper
{
    public static IEnumerable<Type> GetActionTypes()
    {
        return Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(Action).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);
    }
}