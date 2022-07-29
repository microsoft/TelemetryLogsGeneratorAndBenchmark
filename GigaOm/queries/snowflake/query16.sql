with ArgMax as (
  select 
    Source,
    min(Timestamp) as MinTS,
    max(Timestamp) as MaxTS,
    count(*) as Errors
  from 
    logs_c
  where
    Timestamp between 
        to_timestamp('2014-03-08 00:00:00')
        and timestampadd(hour, 3, timestamp '2014-03-08 00:00:00') 
    and Level = 'Error'
  group by
    Source
)
select 
  ArgMax.Source,
  MinTS as FirstErrorTime,
  FirstComp.Component as FirstErrorComponent,
  MaxTS as LastErrorTime,
  LastComp.Component as LastErrorComponent,
  Errors
from 
  ArgMax
left join
  logs_c FirstComp 
    on FirstComp.Source = ArgMax.Source and FirstComp.Timestamp = ArgMax.MinTS
left join
  logs_c LastComp 
    on LastComp.Source = ArgMax.Source and LastComp.Timestamp = ArgMax.MaxTS
order by
  Errors desc
limit 5
;