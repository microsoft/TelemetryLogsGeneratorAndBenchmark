select 
  Component,
  Level,
  Node,
  ClientRequestId,
  count(*) as Count
from 
  logs_c
where
  Timestamp between 
        to_timestamp('2014-03-08 00:00:00') 
    and timestampadd(day, 3, timestamp '2014-03-08 12:00:00')  
  and Source = 'IMAGINEFIRST0'
  and regexp_like(Message,'.*\\bDownloaded\\b.*', 'is')
group by 
  Component,
  Level,
  Node,
  ClientRequestId
order by
  Count desc
limit 10; 
