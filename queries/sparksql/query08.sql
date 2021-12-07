select 
  Level,
  count(*) as Count
from 
  logs_tpc 
where
  Day = '2014-03-08 00:00:00' and
  Timestamp between '2014-03-08 12:00:00' and '2014-03-08 18:00:00' 
group by 
  Level