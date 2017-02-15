using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Implementation;
using Microsoft.Azure.WebJobs.Host;
using Moq;
using Newtonsoft.Json.Serialization;
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
        }

        [Fact]
        public async Task ParseFile()
        {
            var sb = new StringBuilder();

            await ContentCompare.Run(_inputFile, new StringWriter(sb), new NullTraceWriter(TraceLevel.Off));

            _output.WriteLine(sb.ToString());
        }

        public class NullTraceWriter : TraceWriter
        {
            public NullTraceWriter(TraceLevel level) : base(level)
            {
            }

            public override void Trace(TraceEvent traceEvent)
            {
            }
        }
    }
}