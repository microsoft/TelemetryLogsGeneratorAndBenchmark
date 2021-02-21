namespace BenchmarkLogGenerator.Flows
{
    using BenchmarkLogGenerator.Data;
    using Microsoft.Azure.Amqp;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Enumeration;
    using System.Reflection.Metadata.Ecma335;
    using System.Security.Claims;
    using Step = Scheduler.Step;

    public static class IngestionFlow
    {
        public static IEnumerable<Step> Generate(int index, int logsPerSessionFactor, int numSessionsFactor, string source)
        {
            var rng = Generator.Current.Rng;
            
            //increase number of sessions by sizeFactor
            int numSessions = numSessionsFactor*(4 * (80 + (rng.Next() % 1000)));
            for (int i = 0; i < numSessions; ++i)
            {
                var cid = ToGuid(index, i);

                yield return Gen.Sleep(TimeSpan.FromSeconds(rng.Next() % 64));
                //traces 
               
                int rnd = rng.Next();
                Gen.Spawn(IngestionSession(i, cid, rnd, rng, source, logsPerSessionFactor));
            }
        }

        public static IEnumerable<Step> IngestionSession(int index, string cid, long rnd, Random rng, string source, int logsPerSessionFactor)
        {
            long stepLength = 6400000;
            var format = Logs.FileFormats[rnd % 4];
            //increase the number of logs per session by factor
            int numFiles = logsPerSessionFactor * (5 + (int)rnd % 100);
            List<string> files = new List<string>();

            for (int i = 0; i < numFiles; i++)
            {
                var fileName = $"\"\"https://benchmarklogs3.blob.core.windows.net/benchmark/2014/{source}_{index}_{i}.{format}.gz\"\"";
                files.Add(fileName);
            }

            yield return Gen.Sleep(TimeSpan.FromTicks(rnd % stepLength));
            var rand1000 = rnd % 1000;
            var node = $"Engine{(rand1000).ToString().PadLeft(12, '0')}";
            var message = string.Format(Logs.IngestCommand, Names.Tables[rnd % 607], Logs.FileFormats[rnd%4]);
            var filesArray = $"\"[{string.Join(",", files)}]\"";
            bool isCritical = rng.Next() % 10000 <= 25;
            bool isError = rng.Next() % 10000 <= 250;
            int numTracesForSession = 50 + rng.Next() % 5000;

            // first step send ingestion command
            Gen.TraceInfo(node, Level.Information.ToString(), Names.ingestionComponents[0], cid, message, filesArray);

            foreach (var file in files)
            {
                yield return Gen.Sleep(TimeSpan.FromTicks(rnd % stepLength));
                Gen.TraceInfo(node, Level.Information.ToString(), Names.ingestionComponents[1], cid, string.Format(Logs.DownloadEvent, file), GetDownloadProperties(rnd));
                //noise loop
                int numIterations = (int)(numTracesForSession/files.Count);
                for(int i=0; i <= numIterations; i++)
                {
                    yield return Gen.Sleep(TimeSpan.FromTicks(rnd % stepLength));
                    rand1000 = rng.Next() % 1000;
                    node = $"Engine{(rand1000).ToString().PadLeft(12, '0')}";
                    var level = GetLevel(rand1000);
                    var component = Names.Components[rng.Next() % 128];
                    int messageIndex = (int)rng.Next() % 7;
                    message = GetMessage(messageIndex, new object[] { node, rand1000, node + "." + component + ".com" });
                    Gen.TraceInfo(node, level, component, cid, message, "");
                }
                yield return Gen.Sleep(TimeSpan.FromTicks(rnd % stepLength));
                if (isError || isCritical)
                {
                    //no ingestion - emit error trace
                    Gen.TraceInfo(node, Level.Error.ToString(), Names.Components[rng.Next() % 128], cid, GetException(rng.Next()), "");
                }
                else
                {
                    //ingest
                    Gen.TraceInfo(node, Level.Information.ToString(), Names.ingestionComponents[2], cid, string.Format(Logs.IngestionCompletion, file), GetIngestionProperties(rnd, format));
                }
            }
            yield return Gen.Sleep(TimeSpan.FromTicks(rnd % stepLength));
            if (isCritical)
            {
                //emit critical trace
                Gen.TraceInfo(node, Level.Critical.ToString(), Names.Components[rng.Next() % 128], cid, string.Format(Logs.CriticalMessage, rnd, rnd % 100000, rnd % 60000, rnd%40000, rnd%250000),"");
                Gen.TraceInfo(node, Level.Information.ToString(), Names.ingestionComponents[3], cid, string.Format(Logs.CompletedMessage, Logs.StatusCodes[2]), "");
            }
            else if (isError)
            {
                Gen.TraceInfo(node, Level.Information.ToString(), Names.ingestionComponents[3], cid, string.Format(Logs.CompletedMessage, Logs.StatusCodes[1]), "");
            }
            else
            {
                Gen.TraceInfo(node, Level.Information.ToString(), Names.ingestionComponents[3], cid, string.Format(Logs.CompletedMessage, Logs.StatusCodes[0]), "");
            }
        }

        private static string GetDownloadProperties(long rnd)
        {
            long size = (long)rnd % 10_000_000_000L;
            long rowCount = (long)size / ((rnd % 1000) + 999);
            double durationInSeconds = (size * 1.0) / (15 * 1024 * 1024) + (rnd % 17);
            var duration = TimeSpan.FromSeconds(durationInSeconds).ToString();
            return $"\"{{\"\"compressedSize\"\": {size},\"\"OriginalSize\"\": {size*8},\"\"downloadDuration\"\": \"\"{duration}\"\" }}\"";
        }

        private static string GetIngestionProperties(long rnd, string format)
        {
            long size = (long)rnd % 10_000_000_000L;
            long rowCount = (long)size / ((rnd % 1000) + 999);
            double durationInSeconds = (size * 1.0) / (15 * 1024 * 1024) + (rnd % 17);
            double cpuTimeInSeconds = (size * 1.0) / (10 * 1024 * 1024) + (rnd % 110737);
            var duration = TimeSpan.FromSeconds(durationInSeconds).ToString();
            var cpuTime = TimeSpan.FromSeconds(cpuTimeInSeconds).ToString();
            return $"\"{{\"\"size\"\": {size}, \"\"format\"\":\"\"{format}\"\", \"\"rowCount\"\":{rowCount}, \"\"cpuTime\"\":\"\"{cpuTime}\"\",\"\"duration\"\": \"\"{duration}\"\" }}\"";
        }

        private static string GetLevel(long v)
        {
            if (v <= 100)
                return Level.Warning.ToString();
            return Level.Information.ToString();
        }

        private static string GetMessage(int v, object[] replacments)
        {
            var m = Logs.IngestionLogs[v];
            return string.Format(m, replacments);
        }

        private static string GetException(long v)
        {
            var startPosition = v % 10;
            var numberOfStackSteps = 10 * startPosition;
            List<string> steps = new List<string>() { string.Format(Logs.ExceptionHeader, Logs.ExceptionTypes[GetExceptionIndex(v)], (v % 10000).ToString().PadLeft(8, '0'))};
            for (long i = startPosition; i<= numberOfStackSteps; i++ )
            {
                steps.Add(Logs.StackTraces[i % 10]);
            }
            return $"\"{string.Join(Environment.NewLine, steps.ToArray())}\"";
        }

        private static int GetExceptionIndex(long v)
        {
            if (v % 10 <= 4)
                return (int)v % 5;
            if (v % 10 >= 5 && v % 10 <= 6)
                return (int)v % 40;
            if (v % 10 == 7)
                return (int)v % 80;
            return (int)v % 104;
        }

        public static IEnumerable<Step> PeriodicTrace()
        {
            for (int i = 0; i < 100; i++)
            {
                yield return Gen.Sleep(TimeSpan.FromMinutes(1));
                //Gen.TraceInfo("Generator: periodic trace");
            }
        }

        public static string ToGuid(int index, int iterator)
        {
            ulong a;
            ulong b;
            unchecked
            {
                var x = unchecked((ulong)index);
                var y = unchecked((ulong)iterator);
                a = Scramble(Scramble(0x165667B19E3779F9UL, x), y);
                b = Scramble(Scramble(a, y), x);
            }

            Span<byte> bytes = stackalloc byte[16];
            BitConverter.TryWriteBytes(bytes.Slice(0, 8), a);
            BitConverter.TryWriteBytes(bytes.Slice(8, 8), b);
            return (new Guid(bytes)).ToString();
        }

        private static ulong Scramble(ulong h, ulong n)
        {
            unchecked
            {
                h += n * 0xC2B2AE3D27D4EB4FUL;
                h = (h << 31) | (h >> 33);
                h *= 0x9E3779B185EBCA87UL;
                return h;
            }
        }
    }
}
