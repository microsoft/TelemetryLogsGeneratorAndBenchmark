using Azure.Core;
using Azure.Storage.Blobs;
using BenchmarkLogGenerator.Utilities;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace BenchmarkLogGenerator
{
    class Program
    {
        static CommandLineArgs m_args = new CommandLineArgs();
        private static readonly string[] m_basicHelpHints = { "/?", "-?", "?", "/help", "-help", "help" };
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                if (m_basicHelpHints.SafeFastAny(h => string.Equals(h, args[0], StringComparison.OrdinalIgnoreCase)))
                {
                    PrintUsage();
                }
                else
                {
                    CommandLineArgsParser.Parse(args, m_args, null, true);
                }
            }
            else
            {
                PrintUsage();
            }
            if (m_args.outputType == WriterType.LocalDisk && m_args.localPath == null
                || m_args.outputType == WriterType.EventHub && m_args.eventHubConnectionString == null
                || m_args.outputType == WriterType.AzureStorage && m_args.blobConnectionString == null)
            {
                CommandLineArgsParser.WriteHelpStringToConsoleAndQuit(new string[] { }, new CommandLineArgs(), $"The output type of {m_args.outputType} was specified without the corrosponding connection/path");
            }

            Console.WriteLine("Starting...");
            string containerName = "";
            string partitionName = "";
            BlobContainerClient container = null;
            if(m_args.outputType == WriterType.AzureStorage)
            {
                try
                {
                    partitionName = $"-p{m_args.partition}";
                    var blobOptions = new BlobClientOptions();
                    blobOptions.Retry.MaxRetries = 3;
                    blobOptions.Retry.Mode = RetryMode.Exponential;

                    BlobServiceClient blobClient = new BlobServiceClient(m_args.blobConnectionString, blobOptions);
                    containerName = $"logsbenchmark-{m_args.size}{partitionName}".ToLower();
                    var response = blobClient.CreateBlobContainer(containerName);
                    container = response.Value;
                }                
                catch (Exception ex)
                {
                    var color = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error creating container {containerName}. Please verify that the container does not exist");
                    Console.WriteLine($"Exception Message: {ex.Message}");
                    Console.ForegroundColor = color;
                    Environment.Exit(1);
                }
            }

            if (m_args.outputType == WriterType.LocalDisk && m_args.size == BenchmarkDataSize.HundredTB)
            {
                Console.WriteLine("For 100TB data size, please use Azure storage outputType.");
                Environment.Exit(0);
            }

            if (m_args.outputType == WriterType.EventHub && m_args.size != BenchmarkDataSize.OneGB)
            {
                Console.WriteLine("For event hub, data size is restricted to 1 GB.");
                Environment.Exit(0);
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();
            int logsPerSessionFactor = GetLogsPerSessionFactor(m_args.size);
            int numSessionsFactor = GetNumSessionsFactor(m_args.size);
            int start = GetStart(m_args.partition, m_args.size);
            int end = GetEnd(m_args.partition, m_args.size);

            var res = Parallel.For(start, end, index =>
            {
                Generator.Run(index, GetWriter($"iteration count: {index} ", container), logsPerSessionFactor, numSessionsFactor);
            });

            sw.Stop();
            Console.WriteLine($"Total time {sw.ElapsedMilliseconds} ms");
        }

        //Factors:
        //OneTB 6X more sessions, 100X more sources
        //HundredTB 10X more sessions, 10X more logs per session, 10X more sources
        //Expected timespan period for logs:
        //~1 day for 1GB
        //~9 days for 1TB
        //~90 days for 100TB

        private static int GetNumSessionsFactor(BenchmarkDataSize size)
        {
            switch (size)
            {
                case BenchmarkDataSize.OneGB:
                    return 1;
                case BenchmarkDataSize.TenGB:
                    return 2;
                case BenchmarkDataSize.OneTB:
                    return 6;
                case BenchmarkDataSize.HundredTB:
                    return 60;
                default:
                    return 1;
            }
        }


        private static int GetLogsPerSessionFactor(BenchmarkDataSize size)
        {
            switch (size)
            {
                case BenchmarkDataSize.OneGB:
                    return 1;
                case BenchmarkDataSize.TenGB:
                    return 1;
                case BenchmarkDataSize.OneTB:
                    return 1;
                case BenchmarkDataSize.HundredTB:
                    return 1;
                default:
                    return 1;
            }
        }

        private static int GetStart(int partition, BenchmarkDataSize size)
        {
            if (size == BenchmarkDataSize.HundredTB && partition > -1)
            {
                return partition * 100;
            }
            return 0;
        }

        private static int GetEnd(int partition, BenchmarkDataSize size)
        {
            if (size == BenchmarkDataSize.HundredTB && partition > -1)
            {
                return (partition + 1) * 100 - 1;
            }
            return NumThreads(size) * NumIterations(size);
        }

        private static int NumThreads(BenchmarkDataSize size)
        {
            switch (size)
            {
                case BenchmarkDataSize.OneGB:
                    return 1;
                case BenchmarkDataSize.TenGB:
                    return 5;
                case BenchmarkDataSize.OneTB:
                    return 100;
                case BenchmarkDataSize.HundredTB:
                    return 100;
                default:
                    return 100;
            }
        }

        private static int NumIterations(BenchmarkDataSize size)
        {
            switch (size)
            {
                case BenchmarkDataSize.OneGB:
                    return 1;
                case BenchmarkDataSize.TenGB:
                    return 1;
                case BenchmarkDataSize.OneTB:
                    return 1;
                case BenchmarkDataSize.HundredTB:
                    return 10;
                default:
                    return 1;
            }
        }

        private static void PrintUsage()
        {
            var esb = new ExtendedStringBuilder();
            esb.AppendLine();
            esb.AppendLine("The BenchmarkLogGenerator is a tool to generate logs for benchmark testing");
            esb.AppendLine();
            esb.AppendLine("It is invoked with the following parameters:");
            esb.AppendLine("-output:Where the output should be written to. Possible values are: LocalDisk, AzureBlobStorage or EventHub");
            esb.AppendLine("-localPath: The root folder");
            esb.AppendLine("-azureStorageAccountConnections: A comma separated list of Azure storage account connections (can be single connection), containers will be created automaticly using the following template: logsBenchmark-{size}-p{partition}");
            esb.AppendLine("-eventHubConnection: The connection string for Azure EventHub");
            esb.AppendLine("-size: The output size, possible values are OneGB, TenGB, OneTB, HundredTB. Default is OneGB");
            esb.AppendLine("-partition: The applicable partition, between -1 to 9, where -1 means single partition. Only relevant for HundredTB size. Default is -1");
            esb.AppendLine();
            CommandLineArgsParser.PrintUsage(esb);

        }

        private static LogWriter GetWriter(string writerId, BlobContainerClient container)
        {
            switch (m_args.outputType)
            {
                case WriterType.LocalDisk:
                    return new FileLogWriter(m_args.localPath, false, writerId, null);
                case WriterType.AzureStorage:
                    return new FileLogWriter(m_args.blobConnectionString, true, writerId, container);
                case WriterType.EventHub:
                    return new EventHubWriter(m_args.eventHubConnectionString);
                default:
                    return null;
            }
        }
    }
}
