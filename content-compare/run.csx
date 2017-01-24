#r "Implementation.dll"
using System.Net;
using Implementation;

public static Task Run(TimerInfo timerTrigger, TextReader inputFile, TextWriter outputFile, TraceWriter log)
{
    return ContentCompare.Run(inputFile, outputFile);
}