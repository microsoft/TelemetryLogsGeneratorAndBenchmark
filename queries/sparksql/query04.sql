select 
  *
from 
  logs_tpc
where 
  Timestamp between '2014-03-08 03:00:00' and '2014-03-08 04:00:00'
  and (lower (message) like "%internal%")
  order by Timestamp
  limit 1000
 