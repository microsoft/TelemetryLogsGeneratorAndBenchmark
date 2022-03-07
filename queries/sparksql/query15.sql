create or replace temporary view Data as (
select timestamp, node, properties
from 
  logs_tpc 
where isNotNull(Properties)
--    and Source = 'IMAGINEFIRST0'
    and Timestamp between  '2014-03-08 12:00:00' and '2014-03-13 12:00:00'
    and lower(Message) like 'ingestioncompletionevent%'
);
select 
  count(*) 
from 
  logs_tpc
where 
  Day = '2014-03-08 00:00:00' and
  Timestamp between '2014-03-08 00:00:00' and '2014-03-08 14:00:00'
  and Level = 'Warning' and Message like '%enabled%';
  create or replace temporary view TopNodesByCPU as
(with CPUTime as (
select
  Node,
  Properties,
  json_tuple(Properties, 'cpuTime') as (cpuTimeDate),
  cast ((to_timestamp(cpuTimeDate , 'HH:mm:ss.SSSSSSS')) as  double)  as cpuTime
from 
  data
)
select Node, max(cpuTime) as MaxCPU
from CPUTime
group by Node
order by Node desc, MaxCPU desc
limit 10);
with CPUTime as (select
  Node,
  cast (round (cast (timestamp as  double) / 300L)*300 as timestamp) as bin, -- 5 min bin
  json_tuple(Properties, 'cpuTime') as (cpuTimeDate),
  cast ((to_timestamp(cpuTimeDate , 'HH:mm:ss.SSSSSSS')) as  double)  as cpuTime
from 
  data 
where isNotNull(Properties) and Node in (select node from TopNodesByCPU))
select Node, 
bin, 
avg(cpuTime) as AvgCPU 
from CPUTime
group by Node, bin 