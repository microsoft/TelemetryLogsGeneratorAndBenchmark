select
  Source,
  max(
    cast(PropertiesStruct:compressedSize as int) / 
    ( case when startswith(PropertiesStruct:downloadDuration, '1.') 
  then timediff(microsecond, to_time('00:00:00'), to_time(regexp_replace(PropertiesStruct:downloadDuration, '^1\\.(.*)', '\\1', 1, 1, 'm'))) / 1000000 + 24*3600
  else timediff(microsecond, to_time('00:00:00'), to_time(regexp_replace(PropertiesStruct:downloadDuration, '^1\\.(.*)', '\\1', 1, 1, 'm'))) / 1000000
  end
  )) as DownloadRate
from 
  logs_c
where
  Timestamp between 
    to_timestamp('2014-03-08 12:00:00')
    and timestampadd(hour, 1, timestamp '2014-03-08 12:00:00')
  and Component = 'DOWNLOADER'
group by
  Source
order by 
  DownloadRate desc
limit 10;