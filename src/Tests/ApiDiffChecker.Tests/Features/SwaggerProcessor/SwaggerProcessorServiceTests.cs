using FluentAssertions;
using Xunit;

namespace ApiDiffChecker.Tests.Features.SwaggerProcessor
{
    public class SwaggerProcessorServiceTests
    {
        public SwaggerProcessorServiceTests()
        {

        }

        [Fact]
        public void Test1()
        {
            "hello".Should().Be("hello!!");
        }
    }
}
