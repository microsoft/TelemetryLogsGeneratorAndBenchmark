create or replace temporary view Data as (
select 
  *   
from 
  logs_tpc 
where
  Timestamp between '2014-03-08 12:00:00' and '2014-03-08 15:00:00'
    and Source = 'IMAGINEFIRST0' or Source ='PAPERWHITE113'
);
create or replace temporary view Data2 as (
select 
  distinct regexp_extract(Message, 'table=(.*) ') as icTable,
  ClientRequestId
from 
  Data l
where
  lower(substring(l.Message, 3)) like  'ingestioncommand%'
);
create or replace temporary view Data3 as (
select
  json_tuple(l.Properties, 'format', 'rowCount') as (Format, rowCount),
  icTable   
from 
  Data l
inner join
  Data2 d
  on l.ClientRequestId = d.ClientRequestId
where
  lower(l.Message) like 'ingestioncompletionevent%'
);
select Format, icTable, sum(rowCount) as rowCount
from
  Data3
group by
  Format,
  icTable
