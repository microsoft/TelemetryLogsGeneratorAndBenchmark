select
  Source,
  max(PropertiesStruct.compressedSize / time_diff(PropertiesStruct.downloadDuration, '00:00:00', microsecond) / 1000000) as DownloadRate
from 
  gigaom-microsoftadx-2022.logs100tb.logs_c
where
  Timestamp between 
    timestamp '2014-03-08 12:00:00' 
    and timestamp_add(timestamp '2014-03-08 12:00:00', interval 1 hour)
  and Component = 'DOWNLOADER'
group by
  Source
order by 
  DownloadRate desc
limit 10;