with top_nodes as (
select 
  Node
from 
  gigaom-microsoftadx-2022.logs100tb.logs 
where
  Timestamp between 
    timestamp '2014-03-08 12:00:00' 
    and timestamp_add(timestamp '2014-03-08 12:00:00', interval 6 hour) 
  and Level = 'Error'
group by 
  Node
order by
  count(*) desc
limit 10
)
select
  Level,
  Node,
  count(*) as Count
from
  gigaom-microsoftadx-2022.logs100tb.logs
where
  Timestamp between 
    timestamp '2014-03-08 12:00:00' 
    and timestamp_add(timestamp '2014-03-08 12:00:00', interval 6 hour) 
  and Node in (select Node from top_nodes)
group by 
  Level,
  Node;
