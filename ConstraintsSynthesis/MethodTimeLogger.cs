using System;
using System.Diagnostics;
using System.Reflection;

namespace ConstraintsSynthesis
{
    public static class MethodTimeLogger
    {
        public static TimeLogLevel LogLevel = TimeLogLevel.General;
        public static Action<string> LogAction = Console.WriteLine; 

        static MethodTimeLogger()
        {
            LogAction("Execution times");
        }

        public static void Log(MethodBase methodBase, long milliseconds, string message)
        {
            var depth = new StackTrace().FrameCount;

            if (depth > (int) LogLevel)
                return;

            var paddingLength = depth - 7;
            var methodDescription = string.IsNullOrEmpty(message)
                ? methodBase.Name
                : message;

            LogAction($"{methodDescription.PadLeft(methodDescription.Length + paddingLength)} : {milliseconds} ms");
        }
    }
}
