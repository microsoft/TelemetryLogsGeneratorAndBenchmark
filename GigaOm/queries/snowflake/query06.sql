select 
  case
    when Component in ('CLOUDREPORTSERVER', 'COMMON1', 'FABRICINTEGRATOR', 'REQUESTPROTECTION', 'DIRECTORYSERVICE', 'REPORTSERVERSERVICETRACE', 'ACONFIGURATION', 'EXPLORESERVICEWATCHDOG', 'COMMUNICATIONRUNTIME') then 'Security'
    when Component in ('REPORTNETWORKING', 'PUSHDATASERVICETRACE', 'HEALTHSERVICE', 'UTILS', 'PROVIDERSCOMMON') then 'Performance'
    when Component in ('WORKERSERVICECONTENT', 'XMLACOMMON', 'INTEGRATIONDATABASE', 'DATABASEMANAGEMENT') then 'Ingestion'
    else 'Other'
  end as LogType,
  count(*) 
from 
  logs_c 
where 
  Timestamp between 
    to_timestamp('2014-03-08') 
    and timestampadd(day, 10, to_timestamp('2014-03-08'))  
  and Source in ('IMAGINEFIRST0', 'HAVINGCOLUMN182', 'THEREFORESTORE156', 'HOSTNODES207')
group by 
  LogType;
