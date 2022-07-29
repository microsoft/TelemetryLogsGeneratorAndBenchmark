select 
  count(*) 
from 
  gigaom-microsoftadx-2022.logs100tb.logs
where 
  Timestamp between 
    timestamp '2014-03-08' 
    and timestamp_add(timestamp '2014-03-08', interval 12 hour) 
  and Level = 'Warning'
  and regexp_contains(Message, r"(?i)(\b|_)enabled(\b|_)")
