using Microsoft.Extensions.DependencyInjection;
using Service.Ports;
using Service.UseCases;

namespace Service;

public static class DependencyInjection
{
    public static IServiceCollection AddService(this IServiceCollection services)
    {
        services.AddScoped<INoteService, NoteService>();

        return services;
    }
}
