# LogBenchmark

LogBenchmark is a benchmark for testing a log file workload against Microsoft Azure Data Explorer (ADX), Snowflake, and BigQuery.  The Driver has also been ported for ElasticSearch and AWS Athena.

No warranty is expressed or implied.  Use at your own discretion.

## Setup

### Snowflake

Create a database.

```bash
create database <your-database>;
use <your database>;
```

Create a data stage format.

```bash
create or replace file format <your-file-format>
	type = 'CSV'
	field_delimiter = ','
    timestamp_format = 'YYYY-MM-DD"T"HH24:MI:SS.FF"Z"'
    record_delimiter = '\\n'
    field_optionally_enclosed_by = '"'
    empty_field_as_null = true
    escape_unenclosed_field = none
    null_if = ('')
	;
```

Create an unclustered table.

```bash
create or replace table <your-unclustered-table>
     (
      timestamp timestamp,
      source varchar(50),
      node varchar(50),
      level varchar(50),
      component varchar(50),
      clientrequestid varchar(50),
      message varchar(20480),
      properties varchar(20480)
     );
```

Create a data stage and copy into your unclustered table.

```bash
create or replace stage <your-stage>
	file_format = <your-file-format>
    url='azure://<your-storage-blob>'
    credentials=(azure_sas_token='<your-sas-token>')
;
copy into <your-unclustered-table> from @<your-stage> pattern = '.*[.]gz';
```

Create a clustered table using CTAS from your unclustered table.

```bash
create or replace table <your-clustered-table>
cluster by (timestamp, level, source, node, component)
as select
  timestamp,
  source,
  node,
  level,
  component,
  clientrequestid,
  message,
  properties,
  to_object(parse_json('')) as propertiesstruct
from
  <your-unclustered-table>;
```

Update the Properties Struct in the clustered table.

```bash
update 
  <your-clustered-table>
set
  PropertiesStruct = to_object(parse_json(Properties))
where
  Timestamp between 
    to_timestamp('2014-03-08 00:00:00') 
    and timestampadd(day, 10, timestamp '2014-03-08 00:00:00') 
  and ( regexp_like(Properties,'.*cpuTime.*', 's')
    or
    regexp_like(Properties,'.*downloadDuration.*', 's') 
  );
```

Add Search Optimization to the clustered table.

```bash
grant add search optimization on schema public to role sysadmin;
alter table <your-clustered-table> add search optimization;
show tables like '%<your-clustered-table>%';
```

### BigQuery

Create a dataset.

```bash
bq --project_id=<your-project-id> --location=US mk <your-dataset>
```

Create and load a partitioned and clustered table using this JSON schema.

```bash
bq --project_id=<your-project-id> --location=US load --field_delimiter "," --source_format=CSV --ignore_unknown_values --max_bad_records 10000 --allow_quoted_newlines --time_partitioning_field Timestamp --time_partitioning_type HOUR --clustering_fields ClientRequestId,Source,Level,Component <your-dataset.your-table> gs://<your-gs-bucket/folder>/* logs.json; done
```
logs.json
```bash
[
	{
		"name": "Timestamp",
		"type": "TIMESTAMP"
	},
	{
		"name": "Source",
		"type": "STRING"
	},
	{
		"name": "Node",
		"type": "STRING"
	},
	{
		"name": "Level",
		"type": "STRING"
	},
	{
		"name": "Component",
		"type": "STRING"
	},
	{
		"name": "ClientRequestId",
		"type": "STRING"
	},
	{
		"name": "Message",
		"type": "STRING"
	},
	{
		"name": "Properties",
		"type": "STRING"
	}
]
```

Update the schema with this JSON

```bash
bq update logs100tb.logs_c ./logs-struct.json
```
logs-struct.json
```bash
[
	{
		"name": "Timestamp",
		"type": "TIMESTAMP"
	},
	{
		"name": "Source",
		"type": "STRING"
	},
	{
		"name": "Node",
		"type": "STRING"
	},
	{
		"name": "Level",
		"type": "STRING"
	},
	{
		"name": "Component",
		"type": "STRING"
	},
	{
		"name": "ClientRequestId",
		"type": "STRING"
	},
	{
		"name": "Message",
		"type": "STRING"
	},
	{
		"name": "Properties",
		"type": "STRING"
	},
	{
		"name": "PropertiesStruct",
		"type": "STRUCT",
		"fields": [
			{
				"name": "size",
				"type": "INTEGER"
			}, 
			{
				"name": "format",
				"type": "STRING"
			}, 
			{
				"name": "rowCount",
				"type": "INTEGER"
			}, 
			{
				"name":"cpuTime",
				"type": "TIME"
			},
			{
				"name": "duration",
				"type": "TIME"
			},
			{
				"name": "compressedSize",
				"type": "INTEGER"
			},
			{
				"name": "OriginalSize",
				"type": "INTEGER"
			},
			{
				"name": "downloadDuration",
				"type": "TIME"
			}
		]
	}
]
```

Update the Properties Struct in your table.

```bash
update 
  logs100tb.<your-table>
set 
  PropertiesStruct.size = cast(json_extract_scalar(Properties, '$.size') as int64),
  PropertiesStruct.format = json_extract_scalar(Properties, '$.format'),
  PropertiesStruct.rowCount = cast(json_extract_scalar(Properties, '$.rowCount') as int64),
  PropertiesStruct.cpuTime = 
    cast(case
      when starts_with(json_extract_scalar(Properties, '$.cpuTime'), '1.') then 
      time_diff(cast(substr(json_extract_scalar(Properties, '$.cpuTime'), 3, 15) as time), '00:00:00', microsecond) / 1000000 + 24*3600
      else time_diff(cast(substr(json_extract_scalar(Properties, '$.cpuTime'), 1, 15) as time), '00:00:00', microsecond) / 1000000
    end as time),
  PropertiesStruct.duration = 
    cast(case
      when starts_with(json_extract_scalar(Properties, '$.duration'), '1.') then 
      time_diff(cast(substr(json_extract_scalar(Properties, '$.duration'), 3, 15) as time), '00:00:00', microsecond) / 1000000 + 24*3600
      else time_diff(cast(substr(json_extract_scalar(Properties, '$.duration'), 1, 15) as time), '00:00:00', microsecond) / 1000000
    end as time),
  PropertiesStruct.compressedSize = cast(json_extract_scalar(Properties, '$.compressedSize') as int64),
  PropertiesStruct.OriginalSize = cast(json_extract_scalar(Properties, '$.OriginalSize') as int64),
  PropertiesStruct.downloadDuration = 
    cast(case
      when starts_with(json_extract_scalar(Properties, '$.downloadDuration'), '1.') then 
      time_diff(cast(substr(json_extract_scalar(Properties, '$.downloadDuration'), 3, 15) as time), '00:00:00', microsecond) / 1000000 + 24*3600
      else time_diff(cast(substr(json_extract_scalar(Properties, '$.downloadDuration'), 1, 15) as time), '00:00:00', microsecond) / 1000000
    end as time)
where
  (json_extract_scalar(Properties, '$.size') is not null
  or json_extract_scalar(Properties, '$.format') is not null
  or json_extract_scalar(Properties, '$.rowCount') is not null
  or json_extract_scalar(Properties, '$.cpuTime') is not null
  or json_extract_scalar(Properties, '$.duration') is not null
  or json_extract_scalar(Properties, '$.compressedSize') is not null
  or json_extract_scalar(Properties, '$.OriginalSize') is not null
  or json_extract_scalar(Properties, '$.downloadDuration') is not null)
;
```

### Azure Data Explorer
Create a cluster and a database by following [Azure Data Explorer Quickstart](https://docs.microsoft.com/en-us/azure/data-explorer/create-cluster-database-portal#create-a-cluster). Name your database `Benchmark`.

Create a table.
```bash
.create-merge table Logs (Timestamp:datetime, Source:string, Node:string, Level:string, Component:string, ClientRequestId:string, Message:string, Properties:dynamic)
```

Execute [LightIngest](https://docs.microsoft.com/en-us/azure/data-explorer/lightingest) to load data to Logs table.
```bash
LightIngest.exe "https://ingest-CLUSTERNAME.REGION.kusto.windows.net;Federated=true" -database:Benchmark -table:Logs -format:csv -pattern:"*.csv.gz" -dontWait:true -creationTimePattern:"'FOLDERNAME'/yyyy/MM/dd/HH'/'" -sourcePath:"https://BLOBSTORAGENAME.blob.core.windows.net/FOLDERNAME?SASKEY"
```

### Benchmark Driver

Install the required packages for [Snowflake](https://docs.snowflake.com/en/user-guide/python-connector-install.html), [BigQuery](https://pypi.org/project/google-cloud-bigquery/), and [ADX](https://docs.microsoft.com/en-us/azure/data-explorer/kusto/api/python/kusto-python-client-library).

Also any additional packages you need, such as, requests.

## Run the Benchmark

Get the options.

```bash
python logbenchmark.py --help
```




