select 
  count(*) 
from 
  logs_tpc
where 
 Level = 'Error' and lower (Message) like '%safearrayrankmismatch%'