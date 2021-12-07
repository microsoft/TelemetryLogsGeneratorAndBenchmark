with CPUTime as (
select
  Source,
  json_tuple(Properties, 'rowCount', 'duration') as (rowCount, durationdt),
  cast ((to_timestamp(durationdt , 'HH:mm:ss.SSSSSSS')) as  double)  as duration
from 
  logs_tpc 
where 
  Timestamp between '2014-03-08 12:00:00'and '2014-03-08 18:00:00' 
  and lower(Message) like 'ingestioncompletionevent%'
  and Source in ('IMAGINEFIRST0', 'CLIMBSTEADY83', 'INTERNALFIRST79', 'WORKWITHIN77', 'ADOPTIONCUSTOMERS81', 'FIVENEARLY85', 'WHATABOUT98', 'PUBLICBRAINCHILD89', 'WATCHPREVIEW91', 'LATERYEARS87', 'GUTHRIESSCOTT93', 'THISSTORING16') 
  and lower(Properties) like '%parquet%')
select Source, max(rowCount) as MaxRowCount, percentile(duration, 0.50) as p50, percentile(duration, 0.90) as p90, percentile(duration, 0.95) as p95
from CPUTime
group by source
