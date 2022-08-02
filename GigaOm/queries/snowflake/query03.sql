select 
  Component, 
  count(*) 
from
  logs_c 
where 
  Timestamp between 
    to_timestamp('2014-03-08 03:00:00') 
    and timestampadd(hour, 1, to_timestamp('2014-03-08 03:00:00')) 
  and startswith(Source, collate('im', 'en-ci')) 
  and contains(lower(Message), 'response')
group by 
  Component
order by 
  Component;
