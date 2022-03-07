select 
  regexp_extract(Message, 'Exception=(.*);') as ExceptionType,
  regexp_extract(Message, 'Message=(.*);') as ExceptionMessage,
  count(*) as ExceptionCount
from
  logs_tpc 
where 
  Day = '2014-03-08 00:00:00' and
  Timestamp between '2014-03-08 12:00:00' and '2014-03-08 13:00:00' and 
  Level = 'Error' and
  Message like 'Exception%' 
group by
  ExceptionType,
  ExceptionMessage
order by
  ExceptionCount desc
limit 10
 