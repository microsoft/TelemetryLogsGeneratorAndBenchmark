using System;
using System.Collections.Generic;
using System.Text;

namespace BenchmarkLogGenerator
{
    public enum WriterType
    {
        EventHub,
        LocalDisk,
        AzureStorage
    }

    public enum BenchmarkDataSize
    {
        OneGB,
        OneTB,
        HundredTB
    }

    public enum Level
    {
        Information,
        Warning,
        Error,
        Critical
    }
}
