﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Routing;
using Microsoft.TestCommon;

namespace System.Web.OData.Routing
{
    public class ODataRouteTest
    {
        [Fact]
        public void GenerateLinkDirectly_DoesNotReturnNull_IfHelperRequestHasNoConfiguration()
        {
            // Arrange
            ODataRoute odataRoute = new ODataRoute("prefix", pathConstraint: null);

            // Act
            var virtualPathData = odataRoute.GenerateLinkDirectly("odataPath");

            // Assert
            Assert.True(odataRoute.CanGenerateDirectLink);
            Assert.NotNull(virtualPathData);
            Assert.Equal("prefix/odataPath", virtualPathData.VirtualPath);
        }

        [Fact]
        public void CanGenerateDirectLink_IsFalse_IfRouteTemplateHasParameterInPrefix()
        {
            // Arrange && Act
            ODataRoute odataRoute = new ODataRoute("{prefix}", pathConstraint: null);

            // Assert
            Assert.False(odataRoute.CanGenerateDirectLink);
        }

        [Fact]
        public void GenerateLinkDirectly_DoesNotReturnNull_IfRoutePrefixIsNull()
        {
            // Arrange
            ODataRoute odataRoute = new ODataRoute(routePrefix: null, pathConstraint: null);

            // Act
            var virtualPathData = odataRoute.GenerateLinkDirectly("odataPath");

            // Assert
            Assert.True(odataRoute.CanGenerateDirectLink);
            Assert.NotNull(virtualPathData);
            Assert.Equal("odataPath", virtualPathData.VirtualPath);
        }

        [Fact]
        public void GetVirtualPath_CanGenerateDirectLinkIsTrue_IfRoutePrefixIsNull()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage(
                HttpMethod.Get,
                "http://localhost/vpath/prefix/Customers");
            HttpConfiguration config = new HttpConfiguration(new HttpRouteCollection("http://localhost/vpath"));
            request.SetConfiguration(config);
            ODataRoute odataRoute = new ODataRoute(routePrefix: null, pathConstraint: null);

            // Act
            var virtualPathData = odataRoute.GetVirtualPath(
                request,
                new HttpRouteValueDictionary { { "odataPath", "odataPath" }, { "httproute", true } });

            // Assert
            Assert.True(odataRoute.CanGenerateDirectLink);
            Assert.NotNull(virtualPathData);
            Assert.Equal("odataPath", virtualPathData.VirtualPath);
        }

        [Fact]
        public void HasRelaxedODataVersionConstraint_DefaultValueIsFalse()
        {
            ODataRoute odataRoute = new ODataRoute(routePrefix: null, pathConstraint: null);
            Assert.False(((ODataVersionConstraint)odataRoute.Constraints[ODataRouteConstants.VersionConstraintName]).IsRelaxedMatch);
        }

        [Fact]
        public void HasRelaxedODataVersionConstraint_SetValue()
        {
            ODataRoute odataRoute = new ODataRoute(routePrefix: null, pathConstraint: null).HasRelaxedODataVersionConstraint();
            Assert.True(((ODataVersionConstraint)odataRoute.Constraints[ODataRouteConstants.VersionConstraintName]).IsRelaxedMatch);
        }

        [Theory]
        [InlineData("")]
        [InlineData("odataPath")]
        [InlineData("Customers('$&+,/:;=?@ <>#%{}|\\^~[]` ')")]
        public void GetVirtualPath_MatchesHttpRoute(string odataPath)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/vpath/prefix/Customers");
            HttpConfiguration config = new HttpConfiguration(new HttpRouteCollection("http://localhost/vpath"));
            request.SetConfiguration(config);

            IHttpRoute httpRoute = config.Routes.CreateRoute("prefix/{*odataPath}", defaults: null, constraints: null);
            ODataRoute odataRoute = new ODataRoute("prefix", pathConstraint: null);

            // Test that the link generated by ODataRoute matches the one generated by HttpRoute
            Assert.Equal(
                httpRoute.GetVirtualPath(request, new HttpRouteValueDictionary { { "odataPath", odataPath }, { "httproute", true } }).VirtualPath,
                odataRoute.GetVirtualPath(request, new HttpRouteValueDictionary { { "odataPath", odataPath }, { "httproute", true } }).VirtualPath);
        }
    }
}