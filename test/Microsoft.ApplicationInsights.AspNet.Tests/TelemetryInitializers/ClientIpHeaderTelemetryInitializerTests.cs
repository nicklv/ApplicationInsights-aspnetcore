﻿namespace Microsoft.ApplicationInsights.AspNet.Tests.TelemetryInitializers
{
    using System;
    using Microsoft.ApplicationInsights.AspNet.TelemetryInitializers;
    using Microsoft.ApplicationInsights.AspNet.Tests.Helpers;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.AspNet.Hosting;
    using Xunit;
    using Microsoft.AspNet.Http.Internal;

    public class ClientIpHeaderTelemetryInitializerTests
    {
        [Fact]
        public void InitializeThrowIfHttpContextAccessorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => { var initializer = new ClientIpHeaderTelemetryInitializer(null);  });
        }

        [Fact]
        public void InitializeDoesNotThrowIfHttpContextIsUnavailable()
        {
            var ac = new HttpContextAccessor() { HttpContext = null };
            
            var initializer = new ClientIpHeaderTelemetryInitializer(ac);

            initializer.Initialize(new RequestTelemetry());
        }

        [Fact]
        public void InitializeDoesNotThrowIfRequestTelemetryIsUnavailable()
        {
            var ac = new HttpContextAccessor() { HttpContext = new DefaultHttpContext() };
            
            var initializer = new ClientIpHeaderTelemetryInitializer(ac);

            initializer.Initialize(new RequestTelemetry());
        }

        [Fact]
        public void InitializeSetsIPFromStandardHeader()
        {
            var requestTelemetry = new RequestTelemetry();
            var contextAccessor = HttpContextAccessorHelper.CreateHttpContextAccessor(requestTelemetry);
            contextAccessor.HttpContext.Request.Headers.Add("X-Forwarded-For", new string[] { "127.0.0.3" });

            var initializer = new ClientIpHeaderTelemetryInitializer(contextAccessor);

            initializer.Initialize(requestTelemetry);

            Assert.Equal("127.0.0.3", requestTelemetry.Context.Location.Ip);
        }

        [Fact]
        public void InitializeSetsIPFromCustomHeader()
        {
            var requestTelemetry = new RequestTelemetry();
            var contextAccessor = HttpContextAccessorHelper.CreateHttpContextAccessor(requestTelemetry);
            contextAccessor.HttpContext.Request.Headers.Add("HEADER", new string[] { "127.0.0.3;127.0.0.4" });

            var initializer = new ClientIpHeaderTelemetryInitializer(contextAccessor);
            initializer.HeaderNames.Add("HEADER");
            initializer.HeaderValueSeparators = ",;";

            initializer.Initialize(requestTelemetry);

            Assert.Equal("127.0.0.3", requestTelemetry.Context.Location.Ip);
        }

        [Fact]
        public void InitializeDoesNotOverrideIPProvidedInline()
        {
            var requestTelemetry = new RequestTelemetry();
            requestTelemetry.Context.Location.Ip = "127.0.0.4";
            
            var contextAccessor = HttpContextAccessorHelper.CreateHttpContextAccessor(requestTelemetry);
            contextAccessor.HttpContext.Request.Headers.Add("X-Forwarded-For", new string[] { "127.0.0.3" });

            var initializer = new ClientIpHeaderTelemetryInitializer(contextAccessor);

            initializer.Initialize(requestTelemetry);

            Assert.Equal("127.0.0.4", requestTelemetry.Context.Location.Ip);
        }
    }
}