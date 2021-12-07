--Q10
select 
  Component,
  Level,
  Node,
  ClientRequestId,
  count(*) as Count
from 
  logs_tpc 
where
   Timestamp between '2014-03-08 12:00:00' and '2014-03-11 00:00:00' and
  --   Source = 'IMAGINEFIRST0' and 
     lower(Message) like "%downloaded%"
group by 
  Component,
  Level,
  Node,
  ClientRequestId
order by
  Count desc
limit 10