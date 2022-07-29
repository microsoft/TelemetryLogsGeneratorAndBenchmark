select
  *
from
  gigaom-microsoftadx-2022.logs100tb.logs 
where
  Timestamp between 
    timestamp '2014-03-08 03:00:00' 
    and timestamp_add(timestamp '2014-03-08 03:00:00', interval 1 hour)  
  and (
    regexp_contains(Source, r"(?i)(\b|_)Internal(\b|_)")
    or regexp_contains(Node, r"(?i)(\b|_)Internal(\b|_)")
    or regexp_contains(Level, r"(?i)(\b|_)Internal(\b|_)")
    or regexp_contains(Component, r"(?i)(\b|_)Internal(\b|_)")
    or regexp_contains(ClientRequestId, r"(?i)(\b|_)Internal(\b|_)")
    or regexp_contains(Message, r"(?i)(\b|_)Internal(\b|_)")
  )
order by
  Timestamp
limit 1000;
