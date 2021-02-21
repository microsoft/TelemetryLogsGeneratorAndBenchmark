using BenchmarkLogGenerator.Data;
using BenchmarkLogGenerator.Flows;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BenchmarkLogGenerator
{
    using Step = Scheduler.Step;
    using Event = Scheduler.Event;

    public static class Gen
    {
        public static void TraceInfo(string node, string level, string component, string cid, string message, string properties)
        {
            /* - TODO check if we need to generate the strings as invariant format */
            var gen = Generator.Current;
            var now = gen.Now;
            Generator.Current.LogWriter.Write(
                now,
                Generator.Current.Source,
                node, 
                level,
                component,
                cid,
                message,
                properties);
        }

        public static void CloseTracer()
        {
            Generator.Current.LogWriter.Close();
        }

        public static Step Sleep(TimeSpan duration)
        {
            return Generator.Current.Scheduler.DelayFlow(duration);
        }

        public static Event Spawn(IEnumerable<Step> steps)
        {
            return Generator.Current.Scheduler.ScheduleNewFlow(steps);
        }
    }

    public sealed class Generator
    {
        public Scheduler Scheduler { get; private set; }

        public int Seed { get; set; }

        public int SessionCount { get; set; }
        
        public Random Rng { get; private set; }

        public LogWriter LogWriter { get; private set; }

        public string Source;

        public DateTime Now
        {
            get { return Scheduler.Now; }
        }

        [ThreadStatic] static Generator s_current;

        public static Generator Current => s_current;

        public static void Run(int index, LogWriter writer, int logsPerSessionFactor, int numSessionsFactor)
        {
            var gen = new Generator(index, writer);
            gen.SetOnThread();
            Gen.Spawn(IngestionFlow.Generate(index, logsPerSessionFactor, numSessionsFactor, gen.Source));
            gen.Scheduler.Run();
            Gen.CloseTracer();
        }

        public Generator(int seed, LogWriter writer)
        {
            Scheduler = new Scheduler(new DateTime(2014, 3, 8, 0, 0, 0, DateTimeKind.Utc));
            Rng = new Random(5546548 + seed % 100);
            Seed = seed;
            Source = Names.Sources[seed % 1610] + seed;
            LogWriter = writer;
            LogWriter.Source = Source;
        }

        private void SetOnThread()
        {
            s_current = this;
        }
    }
}
