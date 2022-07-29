select
  *
from
  logs_c 
where
  Timestamp between 
    to_timestamp('2014-03-08 03:00:00') 
    and timestampadd(hour, 1,to_timestamp('2014-03-08 03:00:00'))  
  and (
    regexp_like(Source,'.*\\bInternal\\b.*', 'is')
    or regexp_like(Node,'.*\\bInternal\\b.*', 'is')
    or regexp_like(Level,'.*\\bInternal\\b.*', 'is')
    or regexp_like(Component,'.*\\bInternal\\b.*', 'is')
    or regexp_like(ClientRequestId,'.*\\bInternal\\b.*', 'is')
    or regexp_like(Message,'.*\\bInternal\\b.*', 'is')
  )
order by
  Timestamp
limit 1000;
