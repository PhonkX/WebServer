using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace WebServer.DataModel.Tests
{
    [TestFixture]
    public class RequestParserTests
    {
        private string request;
        private RequestParser requestParser;

        [OneTimeSetUp]
        public void Setup()
        {
            requestParser = new RequestParser();
            var sBuilder = new StringBuilder();
            sBuilder
                .Append("GET localhost:45645 HTTP 1.1\r\n")
                .Append("Connection: close\r\n\r\n")
                .Append("Hello, world");

            request = sBuilder.ToString();
        }

        [Test]
        public void ParseRequest_ShouldReturnRequest_WithTestQuery()
        {
            var parsedRequest = requestParser.ParseRequest(request);

            parsedRequest.Query.Should().Be("GET localhost:45645 HTTP 1.1");
        }

        [Test]
        public void ParseRequest_ShouldReturnRequest_WithTestHeaders()
        {
            var parsedRequest = requestParser.ParseRequest(request);

            parsedRequest.Headers.ShouldBeEquivalentTo(
                new WebHeaderCollection
                {
                    { "Connection", "close" }
                }
                );
        }

        [Test]
        public void ParseRequest_ShouldReturnRequest_WithTestBody()
        {
            var parsedRequest = requestParser.ParseRequest(request);

            parsedRequest.Body.Should().Be("Hello, world");
        }
    }
}
