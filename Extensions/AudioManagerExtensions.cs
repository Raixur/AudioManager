using AudioManager.Services;
using GCAudioManager.Services;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AudioManagerExtensions
    {
        public static IServiceCollection AddGcAudioService(this IServiceCollection services,
                                                            IConfiguration audioRepositoryConfig,
                                                            IConfiguration gcConfig)
        {
            services.AddTransient<IStorageService, GcStorageService>();
            services.Configure<GcStorageOptions>(gcConfig);

            services.AddTransient<AudioRepository>();
            services.Configure<AudioRepositoryOptions>(audioRepositoryConfig);

            return services;
        }
    }
}