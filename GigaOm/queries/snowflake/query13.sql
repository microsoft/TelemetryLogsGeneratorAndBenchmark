select 
  Source,
  max(cast(PropertiesStruct:rowCount as int)) as MaxRowCount,
  max(case when startswith(PropertiesStruct:duration, '1.') 
  then timediff(microsecond, to_time('00:00:00'), to_time(regexp_replace(PropertiesStruct:duration, '^1\\.(.*)', '\\1', 1, 1, 'm'))) / 1000000 + 24*3600
  else timediff(microsecond, to_time('00:00:00'), to_time(regexp_replace(PropertiesStruct:duration, '^1\\.(.*)', '\\1', 1, 1, 'm'))) / 1000000
  end),
  percentile_cont(0.50) within group (order by case when startswith(PropertiesStruct:duration, '1.') 
  then timediff(microsecond, to_time('00:00:00'), to_time(regexp_replace(PropertiesStruct:duration, '^1\\.(.*)', '\\1', 1, 1, 'm'))) / 1000000 + 24*3600
  else timediff(microsecond, to_time('00:00:00'), to_time(regexp_replace(PropertiesStruct:duration, '^1\\.(.*)', '\\1', 1, 1, 'm'))) / 1000000
  end) as percentiles50,
  percentile_cont(0.90) within group (order by case when startswith(PropertiesStruct:duration, '1.') 
  then timediff(microsecond, to_time('00:00:00'), to_time(regexp_replace(PropertiesStruct:duration, '^1\\.(.*)', '\\1', 1, 1, 'm'))) / 1000000 + 24*3600
  else timediff(microsecond, to_time('00:00:00'), to_time(regexp_replace(PropertiesStruct:duration, '^1\\.(.*)', '\\1', 1, 1, 'm'))) / 1000000
  end) as percentiles90,
  percentile_cont(0.95) within group (order by case when startswith(PropertiesStruct:duration, '1.') 
  then timediff(microsecond, to_time('00:00:00'), to_time(regexp_replace(PropertiesStruct:duration, '^1\\.(.*)', '\\1', 1, 1, 'm'))) / 1000000 + 24*3600
  else timediff(microsecond, to_time('00:00:00'), to_time(regexp_replace(PropertiesStruct:duration, '^1\\.(.*)', '\\1', 1, 1, 'm'))) / 1000000
  end) as percentiles95
from
  logs_c
where
  Timestamp between 
    to_timestamp('2014-03-08 12:00:00') 
    and timestampadd(hour, 6, timestamp '2014-03-08 12:00:00') 
  and startswith(Message, 'IngestionCompletionEvent')
  and Source in ('IMAGINEFIRST0', 'CLIMBSTEADY83', 'INTERNALFIRST79', 'WORKWITHIN77', 'ADOPTIONCUSTOMERS81', 'FIVENEARLY85', 'WHATABOUT98', 'PUBLICBRAINCHILD89', 'WATCHPREVIEW91', 'LATERYEARS87', 'GUTHRIESSCOTT93', 'THISSTORING16')
  and regexp_like(Properties,'.*\\bparquet\\b.*', 'is')
group by
  Source;