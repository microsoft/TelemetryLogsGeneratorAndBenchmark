select 
  Component,
  Level,
  Node,
  ClientRequestId,
  count(*) as Count
from 
  gigaom-microsoftadx-2022.logs100tb.logs 
where
  Timestamp between 
    timestamp '2014-03-08 00:00:00' 
    and timestamp_add(timestamp '2014-03-08 00:00:00', interval 3 day) 
  and Source = 'IMAGINEFIRST0'
  and regexp_contains(Message, r"(?i)(\b|_)downloaded(\b|_)")
group by 
  Component,
  Level,
  Node,
  ClientRequestId
order by
  Count desc
limit 10;
