with Data as (
select 
  *   
from 
  gigaom-microsoftadx-2022.logs100tb.logs 
where
  Timestamp between 
    timestamp '2014-03-08 12:00:00' 
    and timestamp_add(timestamp '2014-03-08 12:00:00', interval 3 day) 
  and Source in ('IMAGINEFIRST0')
),
Downloading as (
select 
  regexp_extract(Message, 'path:(.*)') as Path,
  ClientRequestId as DownloadClientRequestId
from 
  Data
where
  starts_with(Message, 'Downloading file path:')
),
IngestionCompletion as (
select 
  regexp_extract(Message, 'path:(.*)') as Path,
  ClientRequestId as CompleteClientRequestId
from 
  Data
where
  starts_with(Message, 'IngestionCompletionEvent')
)
select
  count(*)
from
  Downloading d
inner join
  IngestionCompletion ic
  on d.Path = ic.path
where
  DownloadClientRequestId <> CompleteClientRequestId
;