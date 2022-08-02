select 
  count(*) 
from 
  logs_c
where 
  Timestamp between 
    timestamp '2014-03-08' 
    and timestampadd(hour, 12, to_date('2014-03-08')) 
  and Collate(Level, '') = 'Warning'
  and regexp_like(Message, '.*enabled.*', 'is');
