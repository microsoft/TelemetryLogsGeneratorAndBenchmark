# Sample Data Generator 
‘BenchmarkLogGenerator’ is a command line tool to generate sample trace and error logs data. This data can be used for proof of concepts or performance benchmarking scenarios.
This tool supports following configurable command line parameters –
1. **-output**: This is the location where output should be written to. 
Supported values are: LocalDisk, AzureStorage, EventHub
2. **-size**: The data size to be generated. 
Supported values are: OneGB, TenGB, OneTB, HundredTB 
Default value is OneGB
3. **-partition**: The value for data partition, it could be between -1 to 9, where -1 means single partition. Default value is -1. It’s only relevant for HundredTB data size.

**Examples of commands** – change CAPS values in following examples with your environment's values.
1. **OneGB size**

    `BenchmarkLogGenerator.exe -output:AzureStorage -size:OneGB -cc:BLOB STORAGE CONN STR`

    `BenchmarkLogGenerator -output:LocalDisk -size:OneGB -localPath:"C:\DATA"`

    `BenchmarkLogGenerator -output:EventHub -eventHubConnection:Endpoint=sb://EHNAMESPACE.servicebus.windows.net/;EntityPath=EHNAME;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=KEYVALUE -size:OneGB"`

    **Note** – Data size is restricted to 1 GB for event hub.

    **Note** – We host the generated files with default seed value at https://logsbenchmark00.blob.core.windows.net/logsbenchmark-onegb?sp=rl&st=2022-08-18T00:00:00Z&se=2030-01-01T00:00:00Z&spr=https&sv=2021-06-08&sr=c&sig=5pjOow5An3%2BTs5mZ%2FyosJBPtDvV7%2FXfDO8pLEeeylVc%3D. Please use Azure Storage Explorer and attach a blob container specifying this URL or leverage AzCopy to download it. If you plan to load the data to an instance of Azure Data Explorer, use the following snippet to do so directly.

    ```kql
    .execute database script <|
    // Define external table pointing to blob storage.
    .create-or-alter external table LogsExternal(Timestamp:datetime, Source:string, Node:string, Level:string, Component:string, ClientRequestId:string, Message:string, Properties:dynamic)  
    kind=storage 
    partition by (Date:datetime)
    pathformat = (datetime_pattern("yyyy/MM/dd/HH", Date))  
    dataformat=csv
    ( 
    h'https://logsbenchmark00.blob.core.windows.net/logsbenchmark-onegb?sp=rl&st=2022-08-18T00:00:00Z&se=2030-01-01T00:00:00Z&spr=https&sv=2021-06-08&sr=c&sig=5pjOow5An3%2BTs5mZ%2FyosJBPtDvV7%2FXfDO8pLEeeylVc%3D'
    )
    with (includeHeaders='None', compressed=true)
    //
    // Ingest data from external table to internal table named Logs. 
    // The command executes asynchronously in the background. Use ".show operations <<OperationId>>" to monitor its progress.
    .set-or-replace async Logs <| external_table('LogsExternal')
    ``` 

2. **TenGB size**

    `BenchmarkLogGenerator.exe -output:AzureStorage -size:TenGB -cc:BLOB STORAGE CONN STR`

    `BenchmarkLogGenerator -output:LocalDisk -size:TenGB -localPath:"C:\DATA"`

    **Note** – We host the generated files with default seed value at https://logsbenchmark00.blob.core.windows.net/logsbenchmark-tengb?sp=rl&st=2022-08-18T00:00:00Z&se=2030-01-01T00:00:00Z&spr=https&sv=2021-06-08&sr=c&sig=AcZvWrUj9EHWoV6%2BIKeo3dC12f06iq%2Fo42IRI6h4t8o%3D. Please use Azure Storage Explorer and attach a blob container specifying this URL or leverage AzCopy to download it. If you plan to load the data to an instance of Azure Data Explorer, use the following snippet to do so directly.

    ```kql
    .execute database script <|
    // Define external table pointing to blob storage.
    .create-or-alter external table LogsExternal(Timestamp:datetime, Source:string, Node:string, Level:string, Component:string, ClientRequestId:string, Message:string, Properties:dynamic)  
    kind=storage 
    partition by (Date:datetime)
    pathformat = (datetime_pattern("yyyy/MM/dd/HH", Date))  
    dataformat=csv
    ( 
    h'https://logsbenchmark00.blob.core.windows.net/logsbenchmark-tengb?sp=rl&st=2022-08-18T00:00:00Z&se=2030-01-01T00:00:00Z&spr=https&sv=2021-06-08&sr=c&sig=AcZvWrUj9EHWoV6%2BIKeo3dC12f06iq%2Fo42IRI6h4t8o%3D'
    )
    with (includeHeaders='None', compressed=true)
    //
    // Ingest data from external table to internal table named Logs. 
    // The command executes asynchronously in the background. Use ".show operations <<OperationId>>" to monitor its progress.
    .set-or-replace async Logs <| external_table('LogsExternal')
    ```

3. **OneTB size**

    `BenchmarkLogGenerator.exe -output:AzureStorage -size:OneTB -cc:BLOB STORAGE CONN STR`
    `BenchmarkLogGenerator -output:LocalDisk -size:OneTB -localPath:"C:\DATA"`

    You can also use below mentioned Azure batch templates to generate 1 TB data, 3 Standard_D32_v3 VMs would be enough to generate 1 TB .

4. **HundredTB size**

    Use Azure Batch compute to generate 100 TBs of data. Tool has Azure batch templates for generating required batch pools and jobs. 
    Follow these steps to generate 100TBs of data -

    a. Create required ten Azure storage accounts and Azure batch account using this script that calls relevant ARM templates-
    “TelemetryLogsGeneratorAndBenchmark\Infrastructure\ARM\deploy.ps1” -

    b. Create application package that has all required files and dependencies for running BenchmarkLogGenerator.exe. Application package will help to upload required files on
    all batch pool nodes.
    Publish self-contained .net app by using this command -

    `dotnet publish -r win-x64 -c Release --self-contained`

    This will create folder named ‘publish’ under bin/Release with all dependencies, zip ‘publish’ folder to be uploaded as an application package in next step.
    Use this command to create application package -

    `az batch application package create --application-name GENERATOR --name testbatchacc --package-file publish.zip --resource-group sample-rg --version-name 1.0`

    c. Create Azure batch pool using following template –
    10 Standard_D64_v3 VMs with 0 to 9 partitions takes approximately 12 hrs to generate 100 TBs of data. 
   

    `az batch pool create --template generator-pool.json`

    d. Create batch job using following template –
    Provide 10 storage account connection strings that were created in step 3a and create 10 tasks with 0 to 9 partitions respectively.
    
     **Note** – Number of VMs should match the number of tasks and partitions in generator-job.json

    `az batch job create --template generator-job.json`




# Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
