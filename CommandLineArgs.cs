using BenchmarkLogGenerator.Utilities;
using System;
using System.Collections.Generic;

namespace BenchmarkLogGenerator
{
    internal class CommandLineArgs
    {
        [CommandLineArg(
            "output",
            "Where the output should be written to. Valid options are: LocalDisk, AzureStorage or EventHub",
            Mandatory = true)]
        public WriterType outputType= WriterType.LocalDisk;

        [CommandLineArg(
            "localPath",
            "The root folder for the output",
            Mandatory = false)]
        public string localPath = null;

        [CommandLineArg(
            "azureStorageAccountConnections",
            "A comma separated list of Azure storage account connections",
            ShortName = "cc", Mandatory = false)]
        public string blobConnectionString = null;

        [CommandLineArg(
            "eventHubConnection",
            "The connection string for Azure EventHub",
            ShortName = "ehc", Mandatory = false)]
        public string eventHubConnectionString = null;

        [CommandLineArg(
            "size",
            "The output size, possible values are OneGB, OneTB, HundredTB",
             Mandatory = false, DefaultValue = BenchmarkDataSize.OneGB)]
        public BenchmarkDataSize size = BenchmarkDataSize.OneGB;

        [CommandLineArg(
            "partition",
            "The partition id, possible values are -1 to 9, where -1 means single partition",
             Mandatory = false, ShortName = "p", DefaultValue = -1)]
        public int partition = -1;

    }
}
