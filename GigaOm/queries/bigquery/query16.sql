with ArgMax as (
  select 
    Source,
    min(Timestamp) as MinTS,
    max(Timestamp) as MaxTS,
    count(*) as Errors
  from 
    gigaom-microsoftadx-2022.logs100tb.logs
  where
    Timestamp between 
        timestamp '2014-03-08' 
        and timestamp_add(timestamp '2014-03-08', interval 3 hour) 
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
  gigaom-microsoftadx-2022.logs100tb.logs FirstComp 
    on FirstComp.Source = ArgMax.Source and FirstComp.Timestamp = ArgMax.MinTS
left join
  gigaom-microsoftadx-2022.logs100tb.logs LastComp 
    on LastComp.Source = ArgMax.Source and LastComp.Timestamp = ArgMax.MaxTS
order by
  Errors desc
limit 5
;
