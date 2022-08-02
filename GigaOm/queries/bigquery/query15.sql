with TopNodesByCPU as (
  select
    Node,
    time_diff(PropertiesStruct.cpuTime, '00:00:00', microsecond) / 1000000 as cpuTime
  from 
    gigaom-microsoftadx-2022.logs100tb.logs_c
  where
    Source = 'IMAGINEFIRST0'
    and Timestamp between 
      timestamp '2014-03-08 12:00:00' 
      and timestamp_add(timestamp '2014-03-08 12:00:00', interval 5 day)
    and starts_with(lower(Message), 'ingestioncompletionevent')
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
  avg(time_diff(PropertiesStruct.cpuTime, '00:00:00', microsecond) / 1000000), 
  timestamp_seconds(5*60 * div(unix_seconds(Timestamp), 5*60)) as bin
from
  gigaom-microsoftadx-2022.logs100tb.logs_c
where
  Node in (select Node from TopNodesByCPU)
  and Source = 'IMAGINEFIRST0'
  and Timestamp between 
    timestamp '2014-03-08 12:00:00' 
    and timestamp_add(timestamp '2014-03-08 12:00:00', interval 5 day)
  and starts_with(lower(Message), 'ingestioncompletionevent')
group by
  Node,
  bin
order by
  bin
;