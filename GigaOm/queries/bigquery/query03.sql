select 
  Component, 
  count(*) 
from
  gigaom-microsoftadx-2022.logs100tb.logs 
where 
  Timestamp between 
    timestamp '2014-03-08 03:00:00' 
    and timestamp_add(timestamp '2014-03-08 03:00:00', interval 1 hour) 
  and starts_with(lower(Source), 'im') 
  and lower(Message) like '%response%'
group by 
  Component
;
