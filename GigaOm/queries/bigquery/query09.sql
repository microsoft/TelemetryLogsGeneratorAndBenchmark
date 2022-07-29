select 
  Level,
  Component,
  count(*) as Count
from 
  gigaom-microsoftadx-2022.logs100tb.logs
where
  Timestamp between 
    timestamp '2014-03-08 12:00:00' 
    and timestamp_add(timestamp '2014-03-08 12:00:00', interval 6 hour) 
group by 
  Level,
  Component
order by
  Count desc
limit 50;
