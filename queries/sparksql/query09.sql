-- q9 
select 
  Level,
  Component,
  count(*) as Count
from 
  logs_tpc 
where
  Timestamp between '2014-03-08 12:00:00' and '2014-03-08 18:00:00'
group by 
  Level,
  Component
order by
  Count desc
limit 50