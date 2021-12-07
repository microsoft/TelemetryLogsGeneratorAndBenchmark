with DownloadRates as (
select
  Source, 
  Properties,
  json_tuple(Properties, 'compressedSize', 'downloadDuration') as (compressedSize, downloadDuration ),
  cast ((to_timestamp(downloadDuration , 'HH:mm:ss.SSSSSSS')) as  double)  as duration
from 
  logs_tpc 
where
   timestamp between '2014-03-08 12:00:00' and '2014-03-08 13:00:00'
   and Component = 'DOWNLOADER'
)
select 
  Source, max(compressedSize/duration) as DownloadRate
from DownloadRates
group by 
  Source
order by DownloadRate desc
limit 10
