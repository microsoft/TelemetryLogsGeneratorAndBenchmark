--Q16;
with SourceMax as ( 
select 
  Source,
  count(*) as SourceCount   
from 
  logs_tpc 
where
  Timestamp between  '2014-03-08 00:00:00' and '2014-03-08 03:00:00' 
  and Level = 'Error'
  and Source in ('IMAGINEFIRST0', 'CLIMBSTEADY83', 'INTERNALFIRST79', 'WORKWITHIN77', 'ADOPTIONCUSTOMERS81', 'FIVENEARLY85', 'WHATABOUT98', 'PUBLICBRAINCHILD89', 'WATCHPREVIEW91', 'LATERYEARS87', 'GUTHRIESSCOTT93', 'THISSTORING16')
  and lower(Message) like '%arraytypemismatch%'
group by
  Source
)
, TopNestedNodes as ( 
select
  *
from (
select
  *,
  row_number() over (partition by Source order by NodeErrors desc) as rownum
from (
select 
  Source,
  Node,
  count(*) as NodeErrors   
from 
  logs_tpc 
where
  Timestamp between  '2014-03-08 00:00:00' and '2014-03-08 03:00:00' 
  and Level = 'Error'
  and lower(Message) like '%arraytypemismatch%'
group by
  Node,
  Source
order by
  Source,
  NodeErrors desc
)
)
where
  rownum <= 3
)
, TopNestedComponents as ( 
select
  Source,
  Node,
  case
-- added first due to aggregation error
    when first(rownum) <= 3 then Component
    else 'Other components'
  end as Component,
  sum(ComponentErrors) as Errors   
from (
select
  *,
  row_number() over (partition by Source,Node order by ComponentErrors desc) as rownum
from (
select 
  Source,
  Node,
  Component,
  count(*) as ComponentErrors   
from 
  logs_tpc 
where
  Timestamp between  '2014-03-08 00:00:00' and '2014-03-08 03:00:00' 
  and Level = 'Error'
  and lower (Message) like '%arraytypemismatch%'
group by
  Component,
  Node,
  Source
)
)
group by
  Source,
  Node,
  Component
order by
  Source,
  Node,
  Errors desc
)
select
  S.Source,
  N.Node,
  N.NodeErrors,
  C.Component,
  C.Errors as ComponentErrors   
from
  TopNestedComponents C
inner join
  SourceMax S
  on C.Source = S.Source
inner join 
  TopNestedNodes N
  on C.Node = N.Node
  and C.Source = N.Source
order by
  S.Source,
  N.Node,
  C.Component