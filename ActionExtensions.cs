using Microsoft.Extensions.DependencyInjection;

public static class ActionExtensions
{
    public static void AddActionServices(this IServiceCollection serviceCollection)
    {
        foreach (var actionType in ActionHelper.GetActionTypes())
            serviceCollection.AddScoped(actionType);
    }

    public static void AddActionStore(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton(new ActionVectorStore());
    }
}