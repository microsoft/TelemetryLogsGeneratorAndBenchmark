namespace BenchmarkLogGenerator.Data
{
    class Logs
    {
        public static readonly string[] IngestionLogs = new string[]
        {
            "AadValidator.ValidateAudienceImpl.{0}: {1} Audiences=https://{2}",
            @"Audience 'https://{2}' matches the valid {0} audience regex '^https://[\w\.\-]+\.{1}\.windows\.net/?$'",
            "AadValidator.ValidateIssuerImpl.{0}: Start {1} Issuer=https://{2}/",
            "Gateway {0} resolved primary {1} for service 'fabric://management.admin.svc/ - ': 'net.tcp://{2}/mgmt'",
            "ResponseStreamEncoder {0}: {1}: State='enabled' Reason='Accept-Encoding set to gzip' of https://{2}",
            "OwinResponseStreamCompressor: State='enabled' Reason='Accept-Encoding set to deflate'",
            "GetDataPullJobsOperation.OperationAsync.{0}: Downloaded '{1}' messages from https://{2} on different queues"
        };

        public const string IngestCommand = "$$IngestionCommand table={0} format={1}";
        public const string DownloadEvent = "\"Downloading file path: {0}\"";
        public const string IngestionCompletion = "\"IngestionCompletionEvent: finished ingestion file path: {0}\"";
        public const string CompletedMessage = "Completion Report (HttpPost.ExecuteAsync): Completed with HttpResponseMessage StatusCode={0}";

        public static readonly string[] FileFormats = new string[] { "csv", "json", "parquet", "avro" };

        public static readonly string[] StatusCodes = new string[] 
        { 
            "'OK (200)'", 
            "'Request Timeout (408)'", 
            "'Internal Server Error (500)'" 
        };

        //10
        public static readonly string[] StackTraces = new string[]
        {
            @" at BenchmarkLogGenerator.Flows.BootFlow.GetLevel(Int64 v) in C:\Src\Tools\BenchmarkLogGenerator\Flows\BootFlow.cs:line 85",
            @" at BenchmarkLogGenerator.Flows.BootFlow.<IngestionSession>d__1.MoveNext() in C:\Src\Tools\BenchmarkLogGenerator\Flows\BootFlow.cs:line 47",
            @" at BenchmarkLogGenerator.Scheduler.Flow.NextStep() in C:\Src\Tools\BenchmarkLogGenerator\Scheduler.cs:line 74",
            @" at BenchmarkLogGenerator.Scheduler.Step.EnqueueNextStep(Scheduler scheduler) in C:\Src\Tools\BenchmarkLogGenerator\Scheduler.cs:line 112",
            @" at BenchmarkLogGenerator.Scheduler.FlowDelayStep.Execute(Scheduler scheduler) in C:\Src\Tools\BenchmarkLogGenerator\Scheduler.cs:line 137",
            @" at BenchmarkLogGenerator.Scheduler.Run() in C:\Src\Tools\BenchmarkLogGenerator\Scheduler.cs:line 28",
            @" at BenchmarkLogGenerator.Generator.Run(Int32 sizeFactor) in C:\Src\Tools\BenchmarkLogGenerator\Generator.cs:line 84",
            @" at BenchmarkLogGenerator.Generator.<>c__DisplayClass26_0.<RunInBackground>b__0() in C:\Src\Tools\BenchmarkLogGenerator\Generator.cs:line 74",
            @" at System.Threading.ThreadHelper.ThreadStart_Context(Object state)",
            @" at System.Threading.ExecutionContext.RunInternal(ExecutionContext executionContext)"
        };

        //104
        public static readonly string[] ExceptionTypes = new string[]
        {
            "System.AccessViolation",
            "System.AppDomainUnloaded",
            "System.Argument",
            "System.Arithmetic",
            "System.ArrayTypeMismatch",
            "System.BadImageFormat",
            "System.CannotUnloadAppDomain",
            "System.ContextMarshal",
            "System.DataMisaligned",
            "System.ExecutionEngine",
            "System.Format",
            "System.IndexOutOfRange",
            "System.InsufficientExecutionStack",
            "System.InvalidCast",
            "System.InvalidOperation",
            "System.InvalidProgram",
            "System.MemberAccess",
            "System.MulticastNotSupported",
            "System.NotImplemented",
            "System.NotSupported",
            "System.NullReference",
            "System.OperationCanceled",
            "System.OutOfMemory",
            "System.Rank",
            "System.StackOverflow",
            "System.Timeout",
            "System.TypeInitialization",
            "System.TypeLoad",
            "System.TypeUnloaded",
            "System.UnauthorizedAccess",
            "System.UriTemplateMatch",
            "System.Activities.Validation",
            "System.Collections.Generic.KeyNotFound",
            "System.ComponentModel.License",
            "System.ComponentModel.Warning",
            "System.ComponentModel.Design.Serialization.CodeDomSerializer",
            "System.Configuration.Configuration",
            "System.Configuration.Install.Install",
            "System.Data.Data",
            "System.Data.DBConcurrency",
            "System.Data.OperationAborted",
            "System.Data.OracleClient.Oracle",
            "System.Data.SqlTypes.SqlType",
            "System.Deployment.Application.Deployment",
            "System.DirectoryServices.AccountManagement.Principal",
            "System.Drawing.Printing.InvalidPrinter",
            "System.EnterpriseServices.Registration",
            "System.EnterpriseServices.ServicedComponent",
            "System.IdentityModel.LimitExceeded",
            "System.IdentityModel.SecurityMessageSerialization",
            "System.IdentityModel.Tokens.SecurityToken",
            "System.IO.InternalBufferOverflow",
            "System.IO.InvalidData",
            "System.IO.IO",
            "System.Management.Management",
            "System.Printing.Print",
            "System.Reflection.AmbiguousMatch",
            "System.Reflection.ReflectionTypeLoad",
            "System.Resources.MissingManifestResource",
            "System.Resources.MissingSatelliteAssembly",
            "System.Runtime.InteropServices.External",
            "System.Runtime.InteropServices.InvalidComObject",
            "System.Runtime.InteropServices.InvalidOleVariantType",
            "System.Runtime.InteropServices.MarshalDirective",
            "System.Runtime.InteropServices.SafeArrayRankMismatch",
            "System.Runtime.InteropServices.SafeArrayTypeMismatch",
            "System.Runtime.Remoting.Remoting",
            "System.Runtime.Remoting.Server",
            "System.Runtime.Serialization.Serialization",
            "System.Security.HostProtection",
            "System.Security.Security",
            "System.Security.Verification",
            "System.Security.XmlSyntax",
            "System.Security.Authentication.Authentication",
            "System.Security.Cryptography.Cryptographic",
            "System.Security.Policy.Policy",
            "System.Security.Principal.IdentityNotMapped",
            "System.ServiceModel.Dispatcher.InvalidBodyAccess",
            "System.ServiceModel.Dispatcher.MultipleFilterMatches",
            "System.ServiceProcess.Timeout",
            "System.Threading.AbandonedMutex",
            "System.Threading.SemaphoreFull",
            "System.Threading.SynchronizationLock",
            "System.Threading.ThreadAbort",
            "System.Threading.ThreadInterrupted",
            "System.Threading.ThreadStart",
            "System.Threading.ThreadState",
            "System.Transactions.Transaction",
            "System.Web.Caching.DatabaseNotEnabledForNotification",
            "System.Web.Caching.TableNotEnabledForNotification",
            "System.Web.Management.SqlExecution",
            "System.Web.Services.Protocols.Soap",
            "System.Windows.Automation.ElementNotAvailable",
            "System.Windows.Data.ValueUnavailable",
            "System.Windows.Markup.XamlParse",
            "System.Windows.Media.InvalidWmpVersion",
            "System.Windows.Media.Animation.Animation",
            "System.Workflow.Activities.EventDeliveryFailed",
            "System.Workflow.Activities.WorkflowAuthorization",
            "System.Workflow.Runtime.Hosting.Persistence",
            "System.Workflow.Runtime.Tracking.TrackingProfileDeserialization",
            "System.Xml.Xml",
            "System.Xml.Schema.XmlSchema",
            "System.Xml.XPath.XPath",
            "System.Xml.Xsl.Xslt",
        };
        
        public readonly static string ExceptionHeader = @"Exception={0};
            HResult=0x{1};
            Message=exception happened;
            Source=BenchmarkLogGenerator;
            StackTrace:";

        public const string CriticalMessage = "\"$$ALERT[NativeCrash]: Unexpected string size: 'single string size={0}, offsets array size={1}, string idx={2}, offsets32[idx+1]={3}, offsets32[idx]={4}'\"";
    }
}
