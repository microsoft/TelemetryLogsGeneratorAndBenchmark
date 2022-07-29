/* Snowflake DDL and Load */

--stage format
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

create or replace stage <your-stage>
	file_format = <your-file-format>
    url='azure://<your-storage-blob>'
    credentials=(azure_sas_token='<your-sas-token>')
;
copy into <your-unclustered-table> from @<your-stage> pattern = '.*[.]gz';


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

grant add search optimization on schema public to role sysadmin;
alter table <your-clustered-table> add search optimization;
show tables like '%<your-clustered-table>%';

