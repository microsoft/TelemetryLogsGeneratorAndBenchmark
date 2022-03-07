--Q19;
with Data as (
select 
  *   
from 
  logs_tpc 
  where
    Timestamp between '2014-03-08 12:00:00'  and '2014-03-08 15:00:00' 
    and Source in ('IMAGINEFIRST0')
),
Downloading as (
select 
  regexp_extract(Message, 'path:(.*)') as Path,
  ClientRequestId as DownloadClientRequestId
from 
  Data
where
  lower (Message) like 'downloading file path:%'
),
IngestionCompletion as (
select 
  regexp_extract(Message, 'path:(.*)') as Path,
  ClientRequestId as CompleteClientRequestId
from 
  Data
where
  lower (Message) like 'ingestioncompletionevent%'
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
