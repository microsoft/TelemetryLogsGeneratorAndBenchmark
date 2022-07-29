select 
  time_slice(Timestamp, 1, 'HOUR') as bin,
  count(distinct ClientRequestId) as dcount
from 
  logs_c
where
  Timestamp between 
    to_timestamp('2014-03-08 12:00:00') 
    and timestampadd(hour, 6, timestamp '2014-03-08 12:00:00') 
  and Level = 'Error'
group by
  bin
order by
  bin;
