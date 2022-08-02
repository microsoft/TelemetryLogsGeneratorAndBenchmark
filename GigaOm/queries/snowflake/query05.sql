select 
  count(*) 
from
  logs_c
where
  ClientRequestId in (
    'd71ab629-ebaf-9380-5fe8-942541387ce5', 
    '6bb29a30-ce0d-1288-36f0-27dbd57d66b0', 
    '1f82e290-a7c4-ac84-7117-52209b3b9c91', 
    'ecc12181-8c5a-4f87-1ca3-712b4a82c4f0', 
    'd275a6f0-ba1d-22cf-b06b-6dac508ece4b', 
    'f0565381-29db-bf73-ca1b-319e80debe1c', 
    '54807a9a-e442-883f-6d8b-186c1c2a1041', 
    'f1d10647-fc31-dbc3-9e25-67f68a6fe194'
  );
