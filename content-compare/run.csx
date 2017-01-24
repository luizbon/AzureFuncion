#r "Implementation.dll"
using System.Net;
using Implementation;

public static async Task Run(TimerInfo timerTrigger, TextReader inputFile, TextWriter outputFile, TraceWriter log)
{
    return await ContentCompare.Run(inputFile, outputFile);
}