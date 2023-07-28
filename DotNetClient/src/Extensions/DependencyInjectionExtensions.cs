using System;
using StableCube.Backblaze.DotNetClient;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static OptionsBuilder AddBackblazeApiClient(
            this IServiceCollection services)
        {
            return new OptionsBuilder(services);
        }
    }
}