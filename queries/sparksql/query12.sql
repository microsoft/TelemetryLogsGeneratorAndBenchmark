--Q12;
select 
  date_trunc("Hour", Timestamp) as bin,
  count(distinct ClientRequestId) as dcount
from 
  logs_tpc 
where
  Timestamp between '2014-03-08 12:00:00' and '2014-03-08 18:00:00' 
  and Level = 'Error'
group by
  bin
order by
  bin
