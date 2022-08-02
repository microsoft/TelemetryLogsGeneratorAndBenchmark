select 
  timestamp_seconds(1*3600 * div(unix_seconds(Timestamp), 1*3600)) as bin,
  count(distinct ClientRequestId) as dcount
from 
  gigaom-microsoftadx-2022.logs100tb.logs 
where
  Timestamp between 
    timestamp '2014-03-08 12:00:00' 
    and timestamp_add(timestamp '2014-03-08 12:00:00', interval 6 hour) 
  and Level = 'Error'
group by
  bin
order by
  bin;
