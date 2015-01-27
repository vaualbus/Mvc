// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.ApplicationModels;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.ConfigurationModel;

namespace Microsoft.Framework.DependencyInjection
{
    public static class MvcServiceCollectionExtensions
    {
        public static IServiceCollection AddMvc(this IServiceCollection services, IConfiguration configuration = null)
        {
            ConfigureDefaultServices(services, configuration);
            services.TryAdd(MvcServices.GetDefaultServices(configuration));
            return services;
        }

        /// <summary>
        /// Adds services that allows controllers to be activated from the application's <see cref="System.IServiceProvider"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="configuration">The applications <see cref="IConfiguration"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection WithControllersFromServiceProvider(
            [NotNull] this IServiceCollection services,
            IConfiguration configuration = null)
        {
            var describer = new ServiceDescriber(configuration);
            services.Add(describer.Transient<IControllerActivator, ServiceBasedControllerActivator>());
            return services;
        }

        private static void ConfigureDefaultServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions(configuration);
            services.AddDataProtection(configuration);
            services.AddRouting(configuration);
            services.AddScopedInstance(configuration);
            services.AddAuthorization(configuration);
            services.Configure<RouteOptions>(routeOptions =>
                                                    routeOptions.ConstraintMap
                                                         .Add("exists",
                                                              typeof(KnownRouteValueConstraint)));

        }
    }
}
