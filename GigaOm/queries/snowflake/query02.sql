select 
  count(*) 
from 
  logs_c
where 
  Level = 'Error' 
  and regexp_like(Message, '.*SafeArrayRankMismatch.*', 'is');
