using Microsoft.Extensions.DependencyInjection;
using RefactorHelper.Models.Config;

namespace RefactorHelper.App
{
    public static class ProgramExtensions
    {
        public static IServiceCollection AddRefactorHelper(this IServiceCollection services, RefactorHelperSettings settings)
        {
            services.AddSingleton<RefactorHelperApp>(new RefactorHelperApp(settings));

            return services;
        }
    }
}
