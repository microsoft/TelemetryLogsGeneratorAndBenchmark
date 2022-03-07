-- query 11
with top_nodes as (
select 
  Node
from 
  logs_tpc 
where
  Timestamp between '2014-03-08 12:00:00'  and '2014-03-08 18:00:00' and 
  Level = 'Error'
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
  logs_tpc
where
  Timestamp between '2014-03-08 12:00:00' and '2014-03-08 18:00:00' and 
  Node in (select Node from top_nodes)
group by 
  Level,
  Node