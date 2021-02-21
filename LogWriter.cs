using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using Azure.Storage.Blobs;
using BenchmarkLogGenerator.Utilities;
using Microsoft.Azure.EventHubs;

namespace BenchmarkLogGenerator
{

    public abstract class LogWriter : IDisposable
    {
        public abstract void Write(DateTime timestamp, string source, string node, string level, string component, string cid, string message, string properties);
        public abstract void Close();
        public string Source { get; set; }
        #region IDisposable Support

        public abstract void Dispose(bool disposing);
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }

    public class FileLogWriter : LogWriter
    {
        private const int c_maxFileSize = 100000;
        private const string c_compressedSuffix = "csv.gz";
        private static readonly byte[] s_newLine = Encoding.UTF8.GetBytes(Environment.NewLine);

        private bool writeToStorage;
        private int year, month, day, hour;
        string localPath = string.Empty;
        string fileName = string.Empty;
        private MemoryStream activeFile;
        private DirectoryInfo currentDirectory;
        private string rootDirectory;
        private int rowCount;
        private BlobContainerClient blobContainerClient;
        private string blobConnectionString;
        private int fileCounter;
        private string id;
        private Stopwatch sw = new Stopwatch();
        private bool disposedValue = false; // To detect redundant calls

        public FileLogWriter(string targetPath, bool isAzureStorage, string writerId, BlobContainerClient client)
        {
            writeToStorage = isAzureStorage;
            id = writerId;
            if (writeToStorage)
            {
                rootDirectory = Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\temp").FullName;
                blobConnectionString = targetPath;
                blobContainerClient = client;
            }
            else
            {
                rootDirectory = targetPath;
            }
            sw.Start();
        }

        public override void Write(DateTime timestamp, string source, string node, string level, string component, string cid, string message, string properties)
        {
            try
            {
                if (year != timestamp.Year || month != timestamp.Month || day != timestamp.Day || hour != timestamp.Hour)
                {
                    if (activeFile != null)
                    {
                        WriteFile();
                        activeFile.Close();
                        fileCounter++;
                    }
                    //update existing values
                    year = timestamp.Year;
                    month = timestamp.Month;
                    day = timestamp.Day;
                    hour = timestamp.Hour;
                    localPath = string.Join('\\', year, month.ToString("D2"), day.ToString("D2"), hour.ToString("D2"));
                    //Need new file
                    currentDirectory = Directory.CreateDirectory(string.Join('\\', rootDirectory, localPath));
                    activeFile = new MemoryStream();
                }
                else if (rowCount >= c_maxFileSize)
                {
                    //compress file
                    WriteFile();
                    activeFile.Close();

                    fileCounter++;
                    activeFile = new MemoryStream();
                    rowCount = 0;
                }
                var log = string.Join(",", timestamp.FastToString(), source, node, level, component, cid, message, properties);
                activeFile.Write(Encoding.UTF8.GetBytes(log));
                activeFile.Write(s_newLine);

                rowCount++;
            }
            catch (Exception ex)
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Unexpected Error");
                Console.WriteLine(ex.Message);
                Console.ForegroundColor = color;
            }
        }

        private void WriteFile()
        {
            var compressedFileName = GetFileName(currentDirectory.FullName, c_compressedSuffix);
            if (File.Exists(compressedFileName))
            {
                File.Delete(compressedFileName);
            }

            using (FileStream compressedFileStream = File.Create(compressedFileName))
            {
                using (GZipStream compressionStream = new GZipStream(compressedFileStream, CompressionMode.Compress))
                {
                    activeFile.Position = 0;
                    activeFile.CopyTo(compressionStream);
                }
            }

            if (writeToStorage)
            {
                using (FileStream uploadFileStream = File.OpenRead(compressedFileName))
                {
                    var fileName = GetFileName(localPath, c_compressedSuffix);
                    var blobClient = blobContainerClient.GetBlobClient(fileName);
                    //retry blob ingestion
                    bool ingestionDone = false;
                    while (!ingestionDone)
                    {
                        try
                        {
                            blobClient.Upload(uploadFileStream, true, new CancellationToken());
                            ingestionDone = true;
                        }
                        catch (Exception ex)
                        {
                            var color = Console.ForegroundColor;
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Blob upload error");
                            Console.WriteLine(ex.Message);
                            Console.ForegroundColor = color;
                            Thread.Sleep(100);
                        }
                    }
                }
                File.Delete(compressedFileName);
            }
        }

        private string GetFileName(string root, string suffix)
        {
            return $"{root}\\{this.Source}_{fileCounter}.{suffix}";
        }

        public override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    WriteFile();
                    activeFile.Dispose();
                    this.sw.Stop();
                    Console.WriteLine($"Last file closed: {this.id} time elapsed: {Math.Round(sw.Elapsed.TotalMinutes, 2)} minutes");
                }
                disposedValue = true;
            }
        }

        public override void Close()
        {
            this.Dispose();
        }
    }

    public class EventHubWriter : LogWriter
    {
        List<string> logs = new List<string>();
        private bool disposedValue = false; // To detect redundant calls

        int ehSendCounter;
        // local root directory to write to
        public string RootDirectory { get; set; }
        //EventHub connection string
        public string EventHubConnectionString { get; set; }
        //Azure storage container connection string
        public string AzureStorageContainerConnectionString { get; set; }

        EventHubClient eventHubClient;
        public EventHubWriter(string connectionString)
        {
            var builder = new EventHubsConnectionStringBuilder(connectionString)
            {
                TransportType = TransportType.Amqp,
                OperationTimeout = TimeSpan.FromSeconds(120)
            };

            eventHubClient = EventHubClient.Create(builder);

        }

        public override void Write(DateTime timestamp, string source, string node, string level, string component, string cid, string message, string properties)
        {
            var log = string.Join(",", timestamp.FastToString(), source, node, level, component, cid, message, properties);
            logs.Add(log);
            if (logs.Count >= 2000)
            {
                ehSendCounter++;
                SendToEventHub();
                Console.WriteLine($"Message {ehSendCounter} sent");
                logs = new List<string>();
            }
        }

        private void SendToEventHub()
        {
            string recordString = string.Join(Environment.NewLine, logs);
            EventData eventData = new EventData(Encoding.UTF8.GetBytes(recordString));
            eventHubClient.SendAsync(eventData).Wait();
        }

        public override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    SendToEventHub();
                    Console.WriteLine($"Final message to EH");
                }
                disposedValue = true;
            }
        }

        public override void Close()
        {
            this.Dispose();
        }
    }
}
