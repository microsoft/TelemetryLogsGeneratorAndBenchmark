with Data as (
select 
  *   
from 
  gigaom-microsoftadx-2022.logs100tb.logs_c
where
  Timestamp between 
    timestamp '2014-03-08 12:00:00' 
    and timestamp_add(timestamp '2014-03-08 12:00:00', interval 3 day) 
  and Source = 'IMAGINEFIRST0'
),
Data2 as (
select 
  distinct regexp_extract(Message, 'table=(.*) ') as icTable,
  ClientRequestId
from 
  Data 
where
  starts_with(lower(Message), '$$ingestioncommand')
)
select
  Format,
  icTable,
  RowCount
from (
select
  *,
  row_number() over (partition by Format order by RowCount desc) as rownum
from (
select 
  l.PropertiesStruct.format as Format,
  icTable,
  sum(l.PropertiesStruct.rowCount) as RowCount   
from 
  Data l
inner join
  Data2 d
  on l.ClientRequestId = d.ClientRequestId
where
  starts_with(l.Message, 'IngestionCompletionEvent')
group by
  Format,
  icTable
)
)
where
  rownum <= 10
;