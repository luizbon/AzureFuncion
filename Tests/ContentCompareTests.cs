using System.Diagnostics;
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
            _inputFile = new StringReader(@"[{
                url: 'https://www.jbhifi.com.au/computers-tablets/laptops/microsoft/microsoft-surface-book-i7-1tb-16gb-ram/977137/',
                content: '',
                selector: 'p.price span.amount',
                titleSelector: 'h1'
            },{
                url: 'https://www.jbhifi.com.au/computers-tablets/tablets/microsoft/microsoft-surface-book-with-performance-base-i7-512gb-16gb-ram/326449/',
                content: '',
                selector: 'p.price span.amount',
                titleSelector: 'h1'
            }]");

            var mockHttpClient = new Mock<IHttpClient>();

            mockHttpClient.Setup(x => x.PostStringAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((url, content) => _output.WriteLine(content));

            ContentCompare.HttpClient = new Mock<IHttpClient>().Object;
        }

        [Fact]
        public async Task ParseFile()
        {
            var sb = new StringBuilder();

            await ContentCompare.Run(_inputFile, new StringWriter(sb));

            _output.WriteLine(sb.ToString());
        }
    }
}