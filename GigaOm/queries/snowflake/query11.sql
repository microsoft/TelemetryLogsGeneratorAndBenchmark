with top_nodes as (
select 
  Node
from 
  logs_c 
where
  Timestamp between 
    to_timestamp('2014-03-08 12:00:00') 
    and timestampadd(hour, 6, timestamp '2014-03-08 12:00:00') 
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
  logs_c
where
  Timestamp between 
    to_timestamp('2014-03-08 12:00:00') 
    and timestampadd(hour, 6, timestamp '2014-03-08 12:00:00') 
  and Node in (select Node from top_nodes)
group by 
  Level,
  Node;
