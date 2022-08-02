select 
  regexp_substr(Message, 'Exception=(.*);', 1, 1, 'e', 1) as ExceptionType,
  regexp_substr(Message, 'Message=(.*);', 1, 1, 'e', 1) as ExceptionMessage,
  count(*) as ExceptionCount
from
  logs_c
where 
  Timestamp between to_timestamp('2014-03-08 12:00:00') and timestampadd(hour, 1, to_timestamp('2014-03-08 12:00:00')) 
  and Level = 'Error'
  and startswith(Message, collate('exception', 'en-ci'))
group by
  ExceptionType,
  ExceptionMessage
order by
  ExceptionCount desc
limit 10;
