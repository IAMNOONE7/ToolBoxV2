using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolBoxV2.Application.LocalMessages;

namespace ToolBoxV2.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddSingleton<ILocalMessageIncrementalBuilderFactory, LocalMessageIncrementalBuilderFactory>();
            return services;
        }
    }
}
