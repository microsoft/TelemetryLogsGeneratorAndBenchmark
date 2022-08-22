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

    **Note** – You can also download the generated files directly from Blob Storage using URLs from the below snippet. If you plan to load the data to an instance of Azure Data Explorer, use the following snippet to do so directly.

    ```kql
    .execute database script with (ContinueOnErrors=true) <|
    //
    // Define table schema.
    .create-merge table Logs(Timestamp:datetime, Source:string, Node:string, Level:string, Component:string, ClientRequestId:string, Message:string, Properties:dynamic) 
    //
    // As we will be loading data from 2014, we need to ensure that retention policy is set to a period greater than "now() - 2014". 15 years should suffice.
    .alter-merge table Logs policy retention softdelete = 5475d recoverability = enabled
    //
    // Also, for better query performance, we want all data to land in hot cache and hence caching policy needs to be adjusted, too.
    // If running on free cluster, this command will fail but the rest of the script will succeed. In free cluster, all data is in hot cache.
    .alter table Logs policy caching hot = 5475d
    //
    // Load data. To support the same script on free and regular (paid) cluster we mixed asynchronous commands with several synchronous ones to introduce forced latency and prevent overloading free cluster.
    // On free cluster, it will take about 20 seconds; on regular paid cluster most likely just a few seconds.
    // Once the command completes, verify that your table has 3,834,012 rows by running "Logs | count".
    .ingest async into table Logs (h'https://logsbenchmark00.blob.core.windows.net/logsbenchmark-onegb/2014/03/08/00/data.csv.gz?sp=rl&st=2022-08-18T00:00:00Z&se=2030-01-01T00:00:00Z&spr=https&sv=2021-06-08&sr=c&sig=5pjOow5An3%2BTs5mZ%2FyosJBPtDvV7%2FXfDO8pLEeeylVc%3D') with (format='csv', creationTime='2014-03-08T00:00:00Z')
    .ingest async into table Logs (h'https://logsbenchmark00.blob.core.windows.net/logsbenchmark-onegb/2014/03/08/01/data.csv.gz?sp=rl&st=2022-08-18T00:00:00Z&se=2030-01-01T00:00:00Z&spr=https&sv=2021-06-08&sr=c&sig=5pjOow5An3%2BTs5mZ%2FyosJBPtDvV7%2FXfDO8pLEeeylVc%3D') with (format='csv', creationTime='2014-03-08T01:00:00Z')
    .ingest async into table Logs (h'https://logsbenchmark00.blob.core.windows.net/logsbenchmark-onegb/2014/03/08/02/data.csv.gz?sp=rl&st=2022-08-18T00:00:00Z&se=2030-01-01T00:00:00Z&spr=https&sv=2021-06-08&sr=c&sig=5pjOow5An3%2BTs5mZ%2FyosJBPtDvV7%2FXfDO8pLEeeylVc%3D') with (format='csv', creationTime='2014-03-08T02:00:00Z')
    .ingest async into table Logs (h'https://logsbenchmark00.blob.core.windows.net/logsbenchmark-onegb/2014/03/08/03/data.csv.gz?sp=rl&st=2022-08-18T00:00:00Z&se=2030-01-01T00:00:00Z&spr=https&sv=2021-06-08&sr=c&sig=5pjOow5An3%2BTs5mZ%2FyosJBPtDvV7%2FXfDO8pLEeeylVc%3D') with (format='csv', creationTime='2014-03-08T03:00:00Z')
    .ingest async into table Logs (h'https://logsbenchmark00.blob.core.windows.net/logsbenchmark-onegb/2014/03/08/04/data.csv.gz?sp=rl&st=2022-08-18T00:00:00Z&se=2030-01-01T00:00:00Z&spr=https&sv=2021-06-08&sr=c&sig=5pjOow5An3%2BTs5mZ%2FyosJBPtDvV7%2FXfDO8pLEeeylVc%3D') with (format='csv', creationTime='2014-03-08T04:00:00Z')
    .ingest       into table Logs (h'https://logsbenchmark00.blob.core.windows.net/logsbenchmark-onegb/2014/03/08/05/data.csv.gz?sp=rl&st=2022-08-18T00:00:00Z&se=2030-01-01T00:00:00Z&spr=https&sv=2021-06-08&sr=c&sig=5pjOow5An3%2BTs5mZ%2FyosJBPtDvV7%2FXfDO8pLEeeylVc%3D') with (format='csv', creationTime='2014-03-08T05:00:00Z')
    .ingest async into table Logs (h'https://logsbenchmark00.blob.core.windows.net/logsbenchmark-onegb/2014/03/08/06/data.csv.gz?sp=rl&st=2022-08-18T00:00:00Z&se=2030-01-01T00:00:00Z&spr=https&sv=2021-06-08&sr=c&sig=5pjOow5An3%2BTs5mZ%2FyosJBPtDvV7%2FXfDO8pLEeeylVc%3D') with (format='csv', creationTime='2014-03-08T06:00:00Z')
    .ingest async into table Logs (h'https://logsbenchmark00.blob.core.windows.net/logsbenchmark-onegb/2014/03/08/07/data.csv.gz?sp=rl&st=2022-08-18T00:00:00Z&se=2030-01-01T00:00:00Z&spr=https&sv=2021-06-08&sr=c&sig=5pjOow5An3%2BTs5mZ%2FyosJBPtDvV7%2FXfDO8pLEeeylVc%3D') with (format='csv', creationTime='2014-03-08T07:00:00Z')
    .ingest async into table Logs (h'https://logsbenchmark00.blob.core.windows.net/logsbenchmark-onegb/2014/03/08/08/data.csv.gz?sp=rl&st=2022-08-18T00:00:00Z&se=2030-01-01T00:00:00Z&spr=https&sv=2021-06-08&sr=c&sig=5pjOow5An3%2BTs5mZ%2FyosJBPtDvV7%2FXfDO8pLEeeylVc%3D') with (format='csv', creationTime='2014-03-08T08:00:00Z')
    .ingest async into table Logs (h'https://logsbenchmark00.blob.core.windows.net/logsbenchmark-onegb/2014/03/08/09/data.csv.gz?sp=rl&st=2022-08-18T00:00:00Z&se=2030-01-01T00:00:00Z&spr=https&sv=2021-06-08&sr=c&sig=5pjOow5An3%2BTs5mZ%2FyosJBPtDvV7%2FXfDO8pLEeeylVc%3D') with (format='csv', creationTime='2014-03-08T09:00:00Z')
    .ingest async into table Logs (h'https://logsbenchmark00.blob.core.windows.net/logsbenchmark-onegb/2014/03/08/10/data.csv.gz?sp=rl&st=2022-08-18T00:00:00Z&se=2030-01-01T00:00:00Z&spr=https&sv=2021-06-08&sr=c&sig=5pjOow5An3%2BTs5mZ%2FyosJBPtDvV7%2FXfDO8pLEeeylVc%3D') with (format='csv', creationTime='2014-03-08T10:00:00Z')
    .ingest       into table Logs (h'https://logsbenchmark00.blob.core.windows.net/logsbenchmark-onegb/2014/03/08/11/data.csv.gz?sp=rl&st=2022-08-18T00:00:00Z&se=2030-01-01T00:00:00Z&spr=https&sv=2021-06-08&sr=c&sig=5pjOow5An3%2BTs5mZ%2FyosJBPtDvV7%2FXfDO8pLEeeylVc%3D') with (format='csv', creationTime='2014-03-08T11:00:00Z')
    .ingest async into table Logs (h'https://logsbenchmark00.blob.core.windows.net/logsbenchmark-onegb/2014/03/08/12/data.csv.gz?sp=rl&st=2022-08-18T00:00:00Z&se=2030-01-01T00:00:00Z&spr=https&sv=2021-06-08&sr=c&sig=5pjOow5An3%2BTs5mZ%2FyosJBPtDvV7%2FXfDO8pLEeeylVc%3D') with (format='csv', creationTime='2014-03-08T12:00:00Z')
    .ingest async into table Logs (h'https://logsbenchmark00.blob.core.windows.net/logsbenchmark-onegb/2014/03/08/13/data.csv.gz?sp=rl&st=2022-08-18T00:00:00Z&se=2030-01-01T00:00:00Z&spr=https&sv=2021-06-08&sr=c&sig=5pjOow5An3%2BTs5mZ%2FyosJBPtDvV7%2FXfDO8pLEeeylVc%3D') with (format='csv', creationTime='2014-03-08T13:00:00Z')
    ``` 

2. **TenGB size**

    `BenchmarkLogGenerator.exe -output:AzureStorage -size:TenGB -cc:BLOB STORAGE CONN STR`

    `BenchmarkLogGenerator -output:LocalDisk -size:TenGB -localPath:"C:\DATA"`

    **Note** – You can also download the generated files directly from Blob Storage using URLs from the below snippet. If you plan to load the data to an instance of Azure Data Explorer, use the following snippet to do so directly.

    ```kql
    .execute database script with (ContinueOnErrors=true) <|
    //
    // Define table schema.
    .create-merge table Logs(Timestamp:datetime, Source:string, Node:string, Level:string, Component:string, ClientRequestId:string, Message:string, Properties:dynamic) 
    //
    // As we will be loading data from 2014, we need to ensure that retention policy is set to a period greater than "now() - 2014". 15 years should suffice.
    .alter-merge table Logs policy retention softdelete = 5475d recoverability = enabled
    //
    // Also, for better query performance, we want all data to land in hot cache and hence caching policy needs to be adjusted, too.
    // If running on free cluster, this command will fail but the rest of the script will succeed. In free cluster, all data is in hot cache.
    .alter table Logs policy caching hot = 5475d
    //
    // Load data. To support the same script on free and regular (paid) cluster we mixed asynchronous commands with several synchronous ones to introduce forced latency and prevent overloading free cluster.
    // On free cluster, it will take about 3 minutes; on regular paid cluster most likely less than a minute.
    // Once the command completes, verify that your table has 45,286,250 rows by running "Logs | count".
    .ingest async into table Logs (h'https://logsbenchmark00.blob.core.windows.net/logsbenchmark-tengb/2014/03/08/00/data.csv.gz?sp=rl&st=2022-08-18T00:00:00Z&se=2030-01-01T00:00:00Z&spr=https&sv=2021-06-08&sr=c&sig=AcZvWrUj9EHWoV6%2BIKeo3dC12f06iq%2Fo42IRI6h4t8o%3D') with (format='csv', creationTime='2014-03-08T00:00:00Z')
    .ingest async into table Logs (h'https://logsbenchmark00.blob.core.windows.net/logsbenchmark-tengb/2014/03/08/01/data.csv.gz?sp=rl&st=2022-08-18T00:00:00Z&se=2030-01-01T00:00:00Z&spr=https&sv=2021-06-08&sr=c&sig=AcZvWrUj9EHWoV6%2BIKeo3dC12f06iq%2Fo42IRI6h4t8o%3D') with (format='csv', creationTime='2014-03-08T01:00:00Z')
    .ingest async into table Logs (h'https://logsbenchmark00.blob.core.windows.net/logsbenchmark-tengb/2014/03/08/02/data.csv.gz?sp=rl&st=2022-08-18T00:00:00Z&se=2030-01-01T00:00:00Z&spr=https&sv=2021-06-08&sr=c&sig=AcZvWrUj9EHWoV6%2BIKeo3dC12f06iq%2Fo42IRI6h4t8o%3D') with (format='csv', creationTime='2014-03-08T02:00:00Z')
    .ingest async into table Logs (h'https://logsbenchmark00.blob.core.windows.net/logsbenchmark-tengb/2014/03/08/03/data.csv.gz?sp=rl&st=2022-08-18T00:00:00Z&se=2030-01-01T00:00:00Z&spr=https&sv=2021-06-08&sr=c&sig=AcZvWrUj9EHWoV6%2BIKeo3dC12f06iq%2Fo42IRI6h4t8o%3D') with (format='csv', creationTime='2014-03-08T03:00:00Z')
    .ingest async into table Logs (h'https://logsbenchmark00.blob.core.windows.net/logsbenchmark-tengb/2014/03/08/04/data.csv.gz?sp=rl&st=2022-08-18T00:00:00Z&se=2030-01-01T00:00:00Z&spr=https&sv=2021-06-08&sr=c&sig=AcZvWrUj9EHWoV6%2BIKeo3dC12f06iq%2Fo42IRI6h4t8o%3D') with (format='csv', creationTime='2014-03-08T04:00:00Z')
    .ingest       into table Logs (h'https://logsbenchmark00.blob.core.windows.net/logsbenchmark-tengb/2014/03/08/05/data.csv.gz?sp=rl&st=2022-08-18T00:00:00Z&se=2030-01-01T00:00:00Z&spr=https&sv=2021-06-08&sr=c&sig=AcZvWrUj9EHWoV6%2BIKeo3dC12f06iq%2Fo42IRI6h4t8o%3D') with (format='csv', creationTime='2014-03-08T05:00:00Z')
    .ingest async into table Logs (h'https://logsbenchmark00.blob.core.windows.net/logsbenchmark-tengb/2014/03/08/06/data.csv.gz?sp=rl&st=2022-08-18T00:00:00Z&se=2030-01-01T00:00:00Z&spr=https&sv=2021-06-08&sr=c&sig=AcZvWrUj9EHWoV6%2BIKeo3dC12f06iq%2Fo42IRI6h4t8o%3D') with (format='csv', creationTime='2014-03-08T06:00:00Z')
    .ingest async into table Logs (h'https://logsbenchmark00.blob.core.windows.net/logsbenchmark-tengb/2014/03/08/07/data.csv.gz?sp=rl&st=2022-08-18T00:00:00Z&se=2030-01-01T00:00:00Z&spr=https&sv=2021-06-08&sr=c&sig=AcZvWrUj9EHWoV6%2BIKeo3dC12f06iq%2Fo42IRI6h4t8o%3D') with (format='csv', creationTime='2014-03-08T07:00:00Z')
    .ingest async into table Logs (h'https://logsbenchmark00.blob.core.windows.net/logsbenchmark-tengb/2014/03/08/08/data.csv.gz?sp=rl&st=2022-08-18T00:00:00Z&se=2030-01-01T00:00:00Z&spr=https&sv=2021-06-08&sr=c&sig=AcZvWrUj9EHWoV6%2BIKeo3dC12f06iq%2Fo42IRI6h4t8o%3D') with (format='csv', creationTime='2014-03-08T08:00:00Z')
    .ingest async into table Logs (h'https://logsbenchmark00.blob.core.windows.net/logsbenchmark-tengb/2014/03/08/09/data.csv.gz?sp=rl&st=2022-08-18T00:00:00Z&se=2030-01-01T00:00:00Z&spr=https&sv=2021-06-08&sr=c&sig=AcZvWrUj9EHWoV6%2BIKeo3dC12f06iq%2Fo42IRI6h4t8o%3D') with (format='csv', creationTime='2014-03-08T09:00:00Z')
    .ingest async into table Logs (h'https://logsbenchmark00.blob.core.windows.net/logsbenchmark-tengb/2014/03/08/10/data.csv.gz?sp=rl&st=2022-08-18T00:00:00Z&se=2030-01-01T00:00:00Z&spr=https&sv=2021-06-08&sr=c&sig=AcZvWrUj9EHWoV6%2BIKeo3dC12f06iq%2Fo42IRI6h4t8o%3D') with (format='csv', creationTime='2014-03-08T10:00:00Z')
    .ingest       into table Logs (h'https://logsbenchmark00.blob.core.windows.net/logsbenchmark-tengb/2014/03/08/11/data.csv.gz?sp=rl&st=2022-08-18T00:00:00Z&se=2030-01-01T00:00:00Z&spr=https&sv=2021-06-08&sr=c&sig=AcZvWrUj9EHWoV6%2BIKeo3dC12f06iq%2Fo42IRI6h4t8o%3D') with (format='csv', creationTime='2014-03-08T11:00:00Z')
    .ingest async into table Logs (h'https://logsbenchmark00.blob.core.windows.net/logsbenchmark-tengb/2014/03/08/12/data.csv.gz?sp=rl&st=2022-08-18T00:00:00Z&se=2030-01-01T00:00:00Z&spr=https&sv=2021-06-08&sr=c&sig=AcZvWrUj9EHWoV6%2BIKeo3dC12f06iq%2Fo42IRI6h4t8o%3D') with (format='csv', creationTime='2014-03-08T12:00:00Z')
    .ingest async into table Logs (h'https://logsbenchmark00.blob.core.windows.net/logsbenchmark-tengb/2014/03/08/13/data.csv.gz?sp=rl&st=2022-08-18T00:00:00Z&se=2030-01-01T00:00:00Z&spr=https&sv=2021-06-08&sr=c&sig=AcZvWrUj9EHWoV6%2BIKeo3dC12f06iq%2Fo42IRI6h4t8o%3D') with (format='csv', creationTime='2014-03-08T13:00:00Z')
    .ingest async into table Logs (h'https://logsbenchmark00.blob.core.windows.net/logsbenchmark-tengb/2014/03/08/14/data.csv.gz?sp=rl&st=2022-08-18T00:00:00Z&se=2030-01-01T00:00:00Z&spr=https&sv=2021-06-08&sr=c&sig=AcZvWrUj9EHWoV6%2BIKeo3dC12f06iq%2Fo42IRI6h4t8o%3D') with (format='csv', creationTime='2014-03-08T14:00:00Z')
    .ingest async into table Logs (h'https://logsbenchmark00.blob.core.windows.net/logsbenchmark-tengb/2014/03/08/15/data.csv.gz?sp=rl&st=2022-08-18T00:00:00Z&se=2030-01-01T00:00:00Z&spr=https&sv=2021-06-08&sr=c&sig=AcZvWrUj9EHWoV6%2BIKeo3dC12f06iq%2Fo42IRI6h4t8o%3D') with (format='csv', creationTime='2014-03-08T15:00:00Z')
    .ingest async into table Logs (h'https://logsbenchmark00.blob.core.windows.net/logsbenchmark-tengb/2014/03/08/16/data.csv.gz?sp=rl&st=2022-08-18T00:00:00Z&se=2030-01-01T00:00:00Z&spr=https&sv=2021-06-08&sr=c&sig=AcZvWrUj9EHWoV6%2BIKeo3dC12f06iq%2Fo42IRI6h4t8o%3D') with (format='csv', creationTime='2014-03-08T16:00:00Z')
    .ingest       into table Logs (h'https://logsbenchmark00.blob.core.windows.net/logsbenchmark-tengb/2014/03/08/17/data.csv.gz?sp=rl&st=2022-08-18T00:00:00Z&se=2030-01-01T00:00:00Z&spr=https&sv=2021-06-08&sr=c&sig=AcZvWrUj9EHWoV6%2BIKeo3dC12f06iq%2Fo42IRI6h4t8o%3D') with (format='csv', creationTime='2014-03-08T17:00:00Z')
    .ingest async into table Logs (h'https://logsbenchmark00.blob.core.windows.net/logsbenchmark-tengb/2014/03/08/18/data.csv.gz?sp=rl&st=2022-08-18T00:00:00Z&se=2030-01-01T00:00:00Z&spr=https&sv=2021-06-08&sr=c&sig=AcZvWrUj9EHWoV6%2BIKeo3dC12f06iq%2Fo42IRI6h4t8o%3D') with (format='csv', creationTime='2014-03-08T18:00:00Z')
    .ingest async into table Logs (h'https://logsbenchmark00.blob.core.windows.net/logsbenchmark-tengb/2014/03/08/19/data.csv.gz?sp=rl&st=2022-08-18T00:00:00Z&se=2030-01-01T00:00:00Z&spr=https&sv=2021-06-08&sr=c&sig=AcZvWrUj9EHWoV6%2BIKeo3dC12f06iq%2Fo42IRI6h4t8o%3D') with (format='csv', creationTime='2014-03-08T19:00:00Z')
    .ingest async into table Logs (h'https://logsbenchmark00.blob.core.windows.net/logsbenchmark-tengb/2014/03/08/20/data.csv.gz?sp=rl&st=2022-08-18T00:00:00Z&se=2030-01-01T00:00:00Z&spr=https&sv=2021-06-08&sr=c&sig=AcZvWrUj9EHWoV6%2BIKeo3dC12f06iq%2Fo42IRI6h4t8o%3D') with (format='csv', creationTime='2014-03-08T20:00:00Z')
    .ingest async into table Logs (h'https://logsbenchmark00.blob.core.windows.net/logsbenchmark-tengb/2014/03/08/21/data.csv.gz?sp=rl&st=2022-08-18T00:00:00Z&se=2030-01-01T00:00:00Z&spr=https&sv=2021-06-08&sr=c&sig=AcZvWrUj9EHWoV6%2BIKeo3dC12f06iq%2Fo42IRI6h4t8o%3D') with (format='csv', creationTime='2014-03-08T21:00:00Z')
    .ingest async into table Logs (h'https://logsbenchmark00.blob.core.windows.net/logsbenchmark-tengb/2014/03/08/22/data.csv.gz?sp=rl&st=2022-08-18T00:00:00Z&se=2030-01-01T00:00:00Z&spr=https&sv=2021-06-08&sr=c&sig=AcZvWrUj9EHWoV6%2BIKeo3dC12f06iq%2Fo42IRI6h4t8o%3D') with (format='csv', creationTime='2014-03-08T22:00:00Z')
    .ingest       into table Logs (h'https://logsbenchmark00.blob.core.windows.net/logsbenchmark-tengb/2014/03/08/23/data.csv.gz?sp=rl&st=2022-08-18T00:00:00Z&se=2030-01-01T00:00:00Z&spr=https&sv=2021-06-08&sr=c&sig=AcZvWrUj9EHWoV6%2BIKeo3dC12f06iq%2Fo42IRI6h4t8o%3D') with (format='csv', creationTime='2014-03-08T23:00:00Z')
    .ingest async into table Logs (h'https://logsbenchmark00.blob.core.windows.net/logsbenchmark-tengb/2014/03/09/00/data.csv.gz?sp=rl&st=2022-08-18T00:00:00Z&se=2030-01-01T00:00:00Z&spr=https&sv=2021-06-08&sr=c&sig=AcZvWrUj9EHWoV6%2BIKeo3dC12f06iq%2Fo42IRI6h4t8o%3D') with (format='csv', creationTime='2014-03-09T00:00:00Z')
    .ingest async into table Logs (h'https://logsbenchmark00.blob.core.windows.net/logsbenchmark-tengb/2014/03/09/01/data.csv.gz?sp=rl&st=2022-08-18T00:00:00Z&se=2030-01-01T00:00:00Z&spr=https&sv=2021-06-08&sr=c&sig=AcZvWrUj9EHWoV6%2BIKeo3dC12f06iq%2Fo42IRI6h4t8o%3D') with (format='csv', creationTime='2014-03-09T01:00:00Z')
    .ingest async into table Logs (h'https://logsbenchmark00.blob.core.windows.net/logsbenchmark-tengb/2014/03/09/02/data.csv.gz?sp=rl&st=2022-08-18T00:00:00Z&se=2030-01-01T00:00:00Z&spr=https&sv=2021-06-08&sr=c&sig=AcZvWrUj9EHWoV6%2BIKeo3dC12f06iq%2Fo42IRI6h4t8o%3D') with (format='csv', creationTime='2014-03-09T02:00:00Z')
    .ingest async into table Logs (h'https://logsbenchmark00.blob.core.windows.net/logsbenchmark-tengb/2014/03/09/03/data.csv.gz?sp=rl&st=2022-08-18T00:00:00Z&se=2030-01-01T00:00:00Z&spr=https&sv=2021-06-08&sr=c&sig=AcZvWrUj9EHWoV6%2BIKeo3dC12f06iq%2Fo42IRI6h4t8o%3D') with (format='csv', creationTime='2014-03-09T03:00:00Z')
    .ingest async into table Logs (h'https://logsbenchmark00.blob.core.windows.net/logsbenchmark-tengb/2014/03/09/04/data.csv.gz?sp=rl&st=2022-08-18T00:00:00Z&se=2030-01-01T00:00:00Z&spr=https&sv=2021-06-08&sr=c&sig=AcZvWrUj9EHWoV6%2BIKeo3dC12f06iq%2Fo42IRI6h4t8o%3D') with (format='csv', creationTime='2014-03-09T04:00:00Z')
    .ingest       into table Logs (h'https://logsbenchmark00.blob.core.windows.net/logsbenchmark-tengb/2014/03/09/05/data.csv.gz?sp=rl&st=2022-08-18T00:00:00Z&se=2030-01-01T00:00:00Z&spr=https&sv=2021-06-08&sr=c&sig=AcZvWrUj9EHWoV6%2BIKeo3dC12f06iq%2Fo42IRI6h4t8o%3D') with (format='csv', creationTime='2014-03-09T05:00:00Z')
    .ingest async into table Logs (h'https://logsbenchmark00.blob.core.windows.net/logsbenchmark-tengb/2014/03/09/06/data.csv.gz?sp=rl&st=2022-08-18T00:00:00Z&se=2030-01-01T00:00:00Z&spr=https&sv=2021-06-08&sr=c&sig=AcZvWrUj9EHWoV6%2BIKeo3dC12f06iq%2Fo42IRI6h4t8o%3D') with (format='csv', creationTime='2014-03-09T06:00:00Z')
    .ingest async into table Logs (h'https://logsbenchmark00.blob.core.windows.net/logsbenchmark-tengb/2014/03/09/07/data.csv.gz?sp=rl&st=2022-08-18T00:00:00Z&se=2030-01-01T00:00:00Z&spr=https&sv=2021-06-08&sr=c&sig=AcZvWrUj9EHWoV6%2BIKeo3dC12f06iq%2Fo42IRI6h4t8o%3D') with (format='csv', creationTime='2014-03-09T07:00:00Z')
    .ingest async into table Logs (h'https://logsbenchmark00.blob.core.windows.net/logsbenchmark-tengb/2014/03/09/08/data.csv.gz?sp=rl&st=2022-08-18T00:00:00Z&se=2030-01-01T00:00:00Z&spr=https&sv=2021-06-08&sr=c&sig=AcZvWrUj9EHWoV6%2BIKeo3dC12f06iq%2Fo42IRI6h4t8o%3D') with (format='csv', creationTime='2014-03-09T08:00:00Z')
    .ingest async into table Logs (h'https://logsbenchmark00.blob.core.windows.net/logsbenchmark-tengb/2014/03/09/09/data.csv.gz?sp=rl&st=2022-08-18T00:00:00Z&se=2030-01-01T00:00:00Z&spr=https&sv=2021-06-08&sr=c&sig=AcZvWrUj9EHWoV6%2BIKeo3dC12f06iq%2Fo42IRI6h4t8o%3D') with (format='csv', creationTime='2014-03-09T09:00:00Z')
    .ingest async into table Logs (h'https://logsbenchmark00.blob.core.windows.net/logsbenchmark-tengb/2014/03/09/10/data.csv.gz?sp=rl&st=2022-08-18T00:00:00Z&se=2030-01-01T00:00:00Z&spr=https&sv=2021-06-08&sr=c&sig=AcZvWrUj9EHWoV6%2BIKeo3dC12f06iq%2Fo42IRI6h4t8o%3D') with (format='csv', creationTime='2014-03-09T10:00:00Z')
    .ingest       into table Logs (h'https://logsbenchmark00.blob.core.windows.net/logsbenchmark-tengb/2014/03/09/11/data.csv.gz?sp=rl&st=2022-08-18T00:00:00Z&se=2030-01-01T00:00:00Z&spr=https&sv=2021-06-08&sr=c&sig=AcZvWrUj9EHWoV6%2BIKeo3dC12f06iq%2Fo42IRI6h4t8o%3D') with (format='csv', creationTime='2014-03-09T11:00:00Z')
    .ingest async into table Logs (h'https://logsbenchmark00.blob.core.windows.net/logsbenchmark-tengb/2014/03/09/12/data.csv.gz?sp=rl&st=2022-08-18T00:00:00Z&se=2030-01-01T00:00:00Z&spr=https&sv=2021-06-08&sr=c&sig=AcZvWrUj9EHWoV6%2BIKeo3dC12f06iq%2Fo42IRI6h4t8o%3D') with (format='csv', creationTime='2014-03-09T12:00:00Z')
    .ingest async into table Logs (h'https://logsbenchmark00.blob.core.windows.net/logsbenchmark-tengb/2014/03/09/13/data.csv.gz?sp=rl&st=2022-08-18T00:00:00Z&se=2030-01-01T00:00:00Z&spr=https&sv=2021-06-08&sr=c&sig=AcZvWrUj9EHWoV6%2BIKeo3dC12f06iq%2Fo42IRI6h4t8o%3D') with (format='csv', creationTime='2014-03-09T13:00:00Z')
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
