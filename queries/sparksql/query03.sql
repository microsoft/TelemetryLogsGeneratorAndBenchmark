select 
  Component, count(*) 
from 
  logs_tpc
where 
  Day = '2014-03-08 00:00:00' and
  Timestamp between '2014-03-08 03:00:00' and '2014-03-08 04:00:00'
  and Source like 'IN%' and Message ILIKE '%response%'
group by Component