with PercentilesTable as (
select 
  Source,
  max(PropertiesStruct.rowCount) as MaxRowCount,
  approx_quantiles(time_diff(PropertiesStruct.duration, '00:00:00', microsecond) / 1000000, 100) percentiles 
from
  gigaom-microsoftadx-2022.logs100tb.logs_c
where
  Timestamp between 
    timestamp '2014-03-08 12:00:00' 
    and timestamp_add(timestamp '2014-03-08 12:00:00', interval 6 hour) 
  and starts_with(Message, 'IngestionCompletionEvent')
  and Source in ('IMAGINEFIRST0', 'CLIMBSTEADY83', 'INTERNALFIRST79', 'WORKWITHIN77', 'ADOPTIONCUSTOMERS81', 'FIVENEARLY85', 'WHATABOUT98', 'PUBLICBRAINCHILD89', 'WATCHPREVIEW91', 'LATERYEARS87', 'GUTHRIESSCOTT93', 'THISSTORING16')
  and regexp_contains(Properties, r"(?i)(\b|_)parquet(\b|_)")
group by
  Source
)
select 
  Source,
  MaxRowCount,
  percentiles[offset(50)] as p50,
  percentiles[offset(90)] as p90,
  percentiles[offset(95)] as p95
from 
  PercentilesTable 
order by
  MaxRowCount desc;