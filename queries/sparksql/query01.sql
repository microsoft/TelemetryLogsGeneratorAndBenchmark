select
  count(*)
from
  logs_tpc
where Timestamp between '2014-03-08 00:00:00' and '2014-03-08 12:00:00'
  and Level = 'Warning'
  and Message like '%enabled%'