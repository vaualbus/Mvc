// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using FormatterWebSite;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Mvc.Xml;
using Microsoft.AspNet.TestHost;
using Xunit;

namespace Microsoft.AspNet.Mvc.FunctionalTests
{
    public class XmlSerializerOutputFormatterTest
    {
        private readonly IServiceProvider _services = TestHelper.CreateServices(nameof(FormatterWebSite));
        private readonly Action<IApplicationBuilder> _app = new FormatterWebSite.Startup().Configure;

        [Fact]
        public async Task FormatterIsCalled()
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/XmlSerializer/GetDummyClass?sampleInput=10");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/xml;charset=utf-8"));
            var expectedOutput = await WriteDataAsync(typeof(DummyClass), new DummyClass() { SampleInt = 10 });

            // Act
            var response = await client.SendAsync(request);

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(expectedOutput, await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task WhenDerivedClassIsReturned()
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(
                HttpMethod.Post, "http://localhost/XmlSerializer/GetDerivedDummyClass?sampleInput=10");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/xml;charset=utf-8"));
            var expectedOutput = await WriteDataAsync(typeof(DummyClass), new DerivedDummyClass
            {
                SampleInt = 10,
                SampleIntInDerived = 50
            });

            // Act
            var response = await client.SendAsync(request);

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(expectedOutput, await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task DoesNotWriteDictionaryObjects()
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(
                HttpMethod.Post, "http://localhost/XmlSerializer/GetDictionary");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/xml;charset=utf-8"));

            // Act
            var response = await client.SendAsync(request);

            //Assert
            Assert.Equal(HttpStatusCode.NotAcceptable, response.StatusCode);
        }

        private static async Task<string> WriteDataAsync(
            Type inputType,
            object input,
            XmlWriterSettings settings = null,
            Encoding encoding = null)
        {
            var xmlSerializer = new XmlSerializer(inputType);
            var stream = new MemoryStream();

            if (settings == null)
            {
                settings = FormattingUtilities.GetDefaultXmlWriterSettings();
            }

            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }

            var xmlWriter = XmlWriter.Create(stream, settings);
            xmlSerializer.Serialize(xmlWriter, input);
            stream.Position = 0;
            var streamReader = new StreamReader(stream, encoding);

            return await streamReader.ReadToEndAsync();
        }
    }
}