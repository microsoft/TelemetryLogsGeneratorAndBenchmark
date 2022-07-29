with Data as (
select 
  *   
from 
  logs_c
where
  Timestamp between 
    to_timestamp('2014-03-08 12:00:00')
    and timestampadd(day, 3, timestamp '2014-03-08 12:00:00') 
  and Source = 'IMAGINEFIRST0'
),
Data2 as (
select 
  distinct regexp_substr(Message, 'table=\\w+') as icTable,
  ClientRequestId
from 
  Data 
where
  startswith(lower(Message), '$$ingestioncommand')
)
select 
  icTable,
  sum(cast(l.PropertiesStruct:rowCount as int)) as TotalRows
from 
  Data l
inner join
  Data2 d
  on l.ClientRequestId = d.ClientRequestId
where
  startswith(l.Message, 'IngestionCompletionEvent')
group by
  icTable
order by
  TotalRows desc
limit 10
;