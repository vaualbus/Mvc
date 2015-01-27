// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using ControllersFromServicesWebSite;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.TestHost;
using Xunit;

namespace Microsoft.AspNet.Mvc.FunctionalTests
{
    public class ControllerFromServicesTest
    {
        private readonly IServiceProvider _provider = TestHelper.CreateServices(
            nameof(ControllersFromServicesWebSite));
        private readonly Action<IApplicationBuilder> _app = new Startup().Configure;

#if ASPNET50
        // Depends on Autofac behavior that is not available in ASPNETCORE50
        [Fact]
        public async Task ControllersAreInitializedFromServiceProvider()
        {
            // Arrange
            var expected = "Value from service: 10";
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetStringAsync("http://localhost/autowireup/service?value=10");

            // Assert
            Assert.Equal(expected, response);
        }

        [Fact]
        public async Task ControllersInitializedFromServicesAreActivated()
        {
            // Arrange
            var expected = "<view-data>some-value</view-data>";
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetStringAsync("http://localhost/autowireup/activated?value=some-value");

            // Assert
            Assert.Equal(expected, response);
        }
#endif

        [Fact]
        public async Task ControllersWithConstructorInjectionAreCreatedAndActivated()
        {
            // Arrange
            var expected = "/constructorinjection 14 test-header-value";
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();
            client.DefaultRequestHeaders.TryAddWithoutValidation("Test-Header", "test-header-value");

            // Act
            var response = await client.GetStringAsync("http://localhost/constructorinjection?value=14");

            // Assert
            Assert.Equal(expected, response);
        }

    }
}
