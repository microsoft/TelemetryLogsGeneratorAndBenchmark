select 
  count(*) 
from 
  gigaom-microsoftadx-2022.logs100tb.logs 
where 
  Level = 'Error'
  and regexp_contains(Message, r"(?i)(\b|_)safeArrayRankMismatch(\b|_)");
