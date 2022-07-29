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
  icTable,
  sum(l.PropertiesStruct.rowCount) as TotalRows
from 
  Data l
inner join
  Data2 d
  on l.ClientRequestId = d.ClientRequestId
where
  starts_with(l.Message, 'IngestionCompletionEvent')
group by
  icTable
order by
  TotalRows desc
limit 10
;