select 
  Level,
  Component,
  count(*) as Count
from 
  logs_c 
where
  Timestamp between 
    to_timestamp('2014-03-08 12:00:00') 
    and timestampadd(hour, 6, timestamp '2014-03-08 12:00:00') 
group by 
  Level,
  Component
order by
  Count desc
limit 50; 
