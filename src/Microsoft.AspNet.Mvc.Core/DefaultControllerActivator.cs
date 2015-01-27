// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Mvc
{
    using CreateControllerThunk = Func<IServiceProvider, object[], object>;

    /// <summary>
    /// <see cref="IControllerActivator"/> that uses type activation to create controllers.
    /// </summary>
    public class DefaultControllerActivator : IControllerActivator
    {
        private static readonly Func<Type, CreateControllerThunk> _createFactoryThunk =
            type => ActivatorUtilities.CreateFactory(type, Type.EmptyTypes);
        private readonly ConcurrentDictionary<Type, CreateControllerThunk> _controllerThunks =
            new ConcurrentDictionary<Type, CreateControllerThunk>();

        /// <inheritdoc />
        public object Create([NotNull] ActionContext actionContext, [NotNull] Type controllerType)
        {
            var thunk = _controllerThunks.GetOrAdd(controllerType, _createFactoryThunk);
            return thunk(actionContext.HttpContext.RequestServices, null);
        }
    }
}
