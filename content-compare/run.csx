#r "Implementation.dll"
using System;
using System.Net;
using Implementation;

public static async Task Run(TimerInfo timerTrigger, TextReader inputFile, TextWriter outputFile, TraceWriter log)
{
    log.Info($"Content Compare executed at {DateTime.UtcNow}");
    await ContentCompare.Run(inputFile, outputFile, log);
}