
/* BigQuery Log Benchmark */

/* Schema Timestamp:TIMESTAMP,Source:STRING,Node:STRING,Level:STRING,Component:STRING,ClientRequestId:STRING,Message:STRING */


bq --project_id=<your-project-id> --location=US load --field_delimiter "," --source_format=CSV --ignore_unknown_values --max_bad_records 10000 --allow_quoted_newlines --time_partitioning_field Timestamp --time_partitioning_type HOUR --clustering_fields ClientRequestId,Source,Level,Component <your-dataset.your-table> gs://<your-gs-bucket/folder>/* logs.json; done

bq update logs100tb.logs_c ./logs-struct.json

update 
  logs100tb.logs_c
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

