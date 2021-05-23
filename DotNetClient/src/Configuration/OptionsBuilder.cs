using System;
using Microsoft.Extensions.DependencyInjection;

namespace StableCube.Backblaze.DotNetClient
{
    public class OptionsBuilder
    {
        private IServiceCollection _services;
        
        public OptionsBuilder(IServiceCollection services)
        {
            _services = services;
            
            _services.AddHttpClient<IB2ClientV2, B2ClientV2>(clientConfig => {
                clientConfig.DefaultRequestHeaders.Add("Accept", "application/json");
                clientConfig.DefaultRequestHeaders.Add("User-Agent", "StableCube.BackblazeClient");
            });

            _services.AddScoped<IBackblazeUploader, BackblazeUploader>();
        }
    }
}
