select 
  regexp_extract(Message, 'Exception=(.*);') as ExceptionType,
  regexp_extract(Message, 'Message=(.*);') as ExceptionMessage,
  count(*) as ExceptionCount
from
  gigaom-microsoftadx-2022.logs100tb.logs 
where 
  Timestamp between timestamp '2014-03-08 12:00:00' and timestamp_add(timestamp '2014-03-08 12:00:00', interval 1 hour) 
  and Level = 'Error'
  and starts_with(lower(Message), 'exception')
group by
  ExceptionType,
  ExceptionMessage
order by
  ExceptionCount desc
limit 10;
