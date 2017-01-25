﻿using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Implementation;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace Tests
{
    public class ContentCompareTests
    {
        private readonly ITestOutputHelper _output;
        private readonly TextReader _inputFile;

        public ContentCompareTests(ITestOutputHelper output)
        {
            _output = output;
            _inputFile = new StringReader(File.ReadAllText(@"C:\code\github\luizbon\AzureFuncion\Tests\Files\content-compare.json"));

            var mockHttpClient = new Mock<IHttpClient>();

            mockHttpClient.Setup(x => x.PostStringAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>((url, content) =>
                {
                    _output.WriteLine(content);
                    return null;
                });

            ContentCompare.HttpClient = new Mock<IHttpClient>().Object;
        }

        [Fact]
        public async Task ParseFile()
        {
            var sb = new StringBuilder();

            await ContentCompare.Run(_inputFile, new StringWriter(sb), null);

            _output.WriteLine(sb.ToString());
        }
    }
}