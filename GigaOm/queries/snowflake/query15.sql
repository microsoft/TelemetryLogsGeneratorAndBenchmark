with TopNodesByCPU as (
  select
    Node,
    case when startswith(PropertiesStruct:cpuTime, '1.') 
      then timediff(microsecond, to_time('00:00:00'), to_time(regexp_replace(PropertiesStruct:cpuTime, '^1\\.(.*)', '\\1', 1, 1, 'm'))) / 1000000 + 24*3600
      else timediff(microsecond, to_time('00:00:00'), to_time(regexp_replace(PropertiesStruct:cpuTime, '^1\\.(.*)', '\\1', 1, 1, 'm'))) / 1000000
    end as cpuTime
  from 
    logs_c
  where
    Source = 'IMAGINEFIRST0'
    and Timestamp between 
      to_timestamp('2014-03-08 12:00:00')
      and timestampadd(day, 5, timestamp '2014-03-08 12:00:00')
    and startswith(lower(Message), 'ingestioncompletionevent')
  group by
    Node,
    cpuTime
  order by
    cpuTime desc,
    Node desc
  limit 10
  )
select
  Node,
  avg(case when startswith(PropertiesStruct:cpuTime, '1.') 
    then timediff(microsecond, to_time('00:00:00'), to_time(regexp_replace(PropertiesStruct:cpuTime, '^1\\.(.*)', '\\1', 1, 1, 'm'))) / 1000000 + 24*3600
    else timediff(microsecond, to_time('00:00:00'), to_time(regexp_replace(PropertiesStruct:cpuTime, '^1\\.(.*)', '\\1', 1, 1, 'm'))) / 1000000
  end), 
  time_slice(Timestamp, 5, 'MINUTE', 'START') as bin
from
  logs_c
where
  Node in (select Node from TopNodesByCPU)
  and Source = 'IMAGINEFIRST0'
  and Timestamp between 
    to_timestamp('2014-03-08 12:00:00')
    and timestampadd(day, 5, timestamp '2014-03-08 12:00:00')
  and startswith(lower(Message), 'ingestioncompletionevent')
group by
  Node,
  bin
order by
  bin
;
