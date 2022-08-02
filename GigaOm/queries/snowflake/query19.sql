with Data as (
select 
  *   
from 
  logs_c 
where
  Timestamp between 
    to_timestamp('2014-03-08 12:00:00')
    and timestampadd(day, 3, timestamp '2014-03-08 12:00:00')
  and Source in ('IMAGINEFIRST0')
),
Downloading as (
select 
  regexp_substr(Message, 'path:(.*)') as Path,
  ClientRequestId as DownloadClientRequestId
from 
  Data
where
  startswith(Message, 'Downloading file path:')
),
IngestionCompletion as (
select 
  regexp_substr(Message, 'path:(.*)') as Path,
  ClientRequestId as CompleteClientRequestId
from 
  Data
where
  startswith(Message, 'IngestionCompletionEvent')
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
