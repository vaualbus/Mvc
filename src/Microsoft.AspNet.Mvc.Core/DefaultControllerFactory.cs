// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Mvc
{
    public class DefaultControllerFactory : IControllerFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IControllerActivator _controllerActivator;
        private static ConcurrentDictionary<Type, Func<IServiceProvider, Type[], object>> _controllerCache = new ConcurrentDictionary<Type, Func<IServiceProvider, Type[], object>>();

        public DefaultControllerFactory(IServiceProvider serviceProvider,
                                        IControllerActivator controllerActivator)
        {
            _serviceProvider = serviceProvider;
            _controllerActivator = controllerActivator;
        }

        public object CreateController(ActionContext actionContext)
        {
            var actionDescriptor = actionContext.ActionDescriptor as ControllerActionDescriptor;
            if (actionDescriptor == null)
            {
                throw new ArgumentException(
                    Resources.FormatActionDescriptorMustBeBasedOnControllerAction(
                        typeof(ControllerActionDescriptor)),
                    nameof(actionContext));
            }

            var args = new Type[] { };
            var controllerType = actionDescriptor.ControllerTypeInfo.AsType();

            //var fact = _controllerCache.GetOrAdd(controllerType, ActivatorUtilities.CreateFactory(controllerType, args));
            //var controller = fact(_serviceProvider, args);

            var controller = ActivatorUtilities.CreateInstance(
                _serviceProvider,
                actionDescriptor.ControllerTypeInfo.AsType());

            _controllerActivator.Activate(controller, actionContext);

            return controller;
        }

        public void ReleaseController(object controller)
        {
            var disposableController = controller as IDisposable;

            if (disposableController != null)
            {
                disposableController.Dispose();
            }
        }
    }
}
