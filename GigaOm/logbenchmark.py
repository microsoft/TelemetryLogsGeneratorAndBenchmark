# logbenchmark.py
#
# Log Benchmark for AWS Athena, Elasticsearch, Snowflake, Google BigQuery, and Splunk
# v1.3
#
# Use at your own discretion. No warranties expressed or implied.
# 
# February 2022
# 

import os
import glob
import sys
import random
import requests
import json
import re
import time
from datetime import datetime
import threading
import collections
import csv
from optparse import OptionParser
#from pyathena import connect as athenaconnect
#import boto3
#from elasticsearch import Elasticsearch
from google.cloud import bigquery
import snowflake.connector
from snowflake.connector.errors import DatabaseError, ProgrammingError
from azure.kusto.data.exceptions import KustoServiceError
from azure.kusto.data.helpers import dataframe_from_result_table
from azure.kusto.data import KustoClient, KustoConnectionStringBuilder, ClientRequestProperties

greetings = u'  LogBenchmark  '
print(u'\u250c' + u'\u2500'*len(greetings) + u'\u2510')
print(u'\u2502' + greetings + u'\u2502')
print(u'\u2514' + u'\u2500'*len(greetings) + u'\u2518')

benchmark_sizes = [ '1TB', '10TB', '100TB']

kusto_cluster = "<your-adx-cluster>"
kusto_db = "<your-adx-database>"
azure_client_id = "<your-client-id>"
azure_client_secret = "<your-client-secret>"
azure_tenant_id = "<your-tenant-id>"

platforms = {
    'kusto': {
        'name': 'Azure Data Explorer',
        'version': '',
        'nodes': '',
        'vmtype': '',
        'location': '',
        'endpoint': kusto_cluster
    },
    'athena': {
        'name': 'AWS Athena',
        'version': '',
        'location': '<your-aws-region>',
        'endpoint': 's3://<your-s3-bucket>/' 
    },
    'elasticsearch': {
        'name': 'Elasticsearch Cloud',
        'version': '',
        'location': '',
        'endpoint': '', 
        'sqlUrl': '', 
        'user': '<your-username>', 
        'pwd': '<your-password>'
    },
    'bigquery': {
        'name': 'Google BigQuery',
        'version': '',
        'location': 'US',
        'endpoint': 'gigaom-adx'
    },
    'snowflake': {
        'name': 'Snowflake',
        'version': '',
        'location': 'Azure',
        'endpoint': '<your-snowflake>', 
        'user': '<your-username>', 
        'pwd': '<your-password>'
    }
}


# Parse the cli arguments
parser = OptionParser(usage="logbenchmark.py [options]")

parser.add_option("-p", "--platform", 
     help="Specifies the API platform being used")

parser.add_option("-d", "--datasize",
     help="Specifies the benchmark size to use")

parser.add_option("-u", "--users", type="int", default=1,
     help="Specifies the number of concurrent users")

parser.add_option("-q", "--queries", 
     help="Specifies a query file or folder")

parser.add_option("-e", "--echoresults", action="store_true", default=False,
     help="Specifies whether to echo results to stdout")

parser.add_option("-r", "--report", action="store_true", default=False,
     help="Specifies to create a detailed csv report")

parser.add_option("-t", "--timeout", type="int", default=3600,
     help="Specifies query timeout in seconds")

(opts, args) = parser.parse_args()

# Check to make sure all the necessary arguments are present and valid
if not (opts.datasize):
    parser.print_help()
    sys.stderr.write('Oops! You must specify a benchmark datasize.\n')
    sys.exit(1)

if (opts.datasize.upper() not in benchmark_sizes):
    sys.stderr.write('Oops! You must specify a valid benchmark datasize:\n')
    print([bs for bs in benchmark_sizes])
    sys.exit(1)

if not (opts.platform):
    parser.print_help()
    sys.stderr.write('Oops! You must specify a valid platform.\n')
    sys.exit(1)

if (opts.platform.lower() not in platforms):
    sys.stderr.write('Oops! You must specify a valid platform.\n')
    print([p for p in platforms])
    sys.exit(1)

if not (opts.queries):
    parser.print_help()
    sys.stderr.write('Oops! You must specify a query file or folder.\n')
    sys.exit(1)

if not (glob.glob(opts.queries)):
    sys.stderr.write('Oops! The file or folder %s does not exist.\n' % opts.queries)
    sys.exit(1)

if opts.platform.lower() == 'kusto':
    kcsb = KustoConnectionStringBuilder.with_aad_application_key_authentication(kusto_cluster, azure_client_id, azure_client_secret, azure_tenant_id)
    try:
        kc = KustoClient(kcsb)
    except:
        sys.stderr.write('Oops! Unable to connect to Kusto at %s.\n' % kusto_cluster)
        sys.exit(1)
    print('Connected to: %s' % kusto_cluster)
    r = kc.execute(kusto_db, ".show version")
    r = kc.execute(kusto_db, "Logs | take 1")
    for row in r.primary_results[0]:
        platforms['kusto']['version'] = str(row[0])

if opts.platform.lower() == 'athena':
    try:
        athena = athenaconnect(s3_staging_dir=platforms['athena']['endpoint'], region_name=platforms['athena']['location']).cursor()
        athena.execute("""
        SELECT current_date
        """)
        print(athena.description)
        print(athena.fetchall())
    except:
        sys.stderr.write('Oops! Unable to connect to Athena at %s.\n' % kusto_cluster)
        sys.exit(1)
    print('Connected to: Athena %s' % platforms['athena']['endpoint'])

if opts.platform.lower() == 'elasticsearch':
    try:
        es = Elasticsearch(hosts=[platforms['elasticsearch']['endpoint']], http_auth=(platforms['elasticsearch']['user'], platforms['elasticsearch']['pwd']), timeout=30)        
    except:
        sys.stderr.write('Oops! Unable to connect to Elasticsearch at %s.\n' % platforms['elasticsearch']['endpoint'])
        sys.exit(1)
    print('Connected to: %s' % platforms['elasticsearch']['endpoint'])
    platforms['elasticsearch']['version'] = es.info()["version"]["number"]

if opts.platform.lower() == 'bigquery':
    try:
        bq = bigquery.Client()
    except:
        sys.stderr.write('Oops! Unable to connect to BigQuery.\n')
        sys.exit(1)
    print('Connected to: BigQuery')
    cacheoff = bigquery.QueryJobConfig(use_query_cache=False)

if opts.platform.lower() == 'snowflake':
    try:
        sfc = snowflake.connector.connect(user=platforms['snowflake']['user'], password=platforms['snowflake']['pwd'], account=platforms['snowflake']['endpoint'])
        sf = sfc.cursor()
        sf.execute("USE DATABASE ADX")
        sf.execute("USE WAREHOUSE ADX")
    except:
        sys.stderr.write('Oops! Unable to connect to Snowflake.\n')
        sys.exit(1)
    print('Connected to: Snowflake')



print('Test: ' + opts.datasize.upper() + ' on ' + (platforms[opts.platform.lower()]['name'] + ' ' + platforms[opts.platform.lower()]['version']).rstrip())



queryfiles = sorted(glob.glob(opts.queries))

print(json.dumps(queryfiles, indent=2))

#sys.exit(0)

q = 1
percentiles = {}
qdict = {}
qchoices = []

for qf in queryfiles:

    with open(qf, 'r') as f:
        query = f.read()    
        qtext = re.sub(r"\s+", " ", query)
    qfbase = os.path.splitext(os.path.basename(qf))[0]
    qdict[q] = {'name': qfbase, 'query': qtext}
    qchoices.append(q)
    q = q + 1

print(json.dumps(qdict, indent=2))

if opts.report:
    results_filename = 'results-logbenchmark-' + opts.platform.lower() + '-' + opts.datasize.upper() + '.csv'
    results_file_exists = os.path.isfile(results_filename)
    csvfile = open(results_filename, 'a')
    headers = ['platform', 'location', 'endpoint', 'data_size', 'users', 'thread', 'query_num', 'query_size', 'query_name', 'query_text', 'result_rows', 'starttime', 'endtime', 'client_duration', 'server_duration', 'totalqueriescompleted', 'totaltimeelapsed', 'qph']        
    writer = csv.DictWriter(csvfile, delimiter=',', lineterminator='\n', fieldnames=headers)
    if not results_file_exists:
        writer.writeheader()  # file doesn't exist yet, write a header
        csvfile.flush()

totalqueriescompleted = 0
testbegan = datetime.today()


class qthread (threading.Thread):
    def __init__(self, t, i):
        threading.Thread.__init__(self)
        self.t = t
        self.i = i
    def run(self):
    
        global totalqueriescompleted
        global testbegan
        global writer
        
        cd = 0
        frow = ''
        numofchoices = 0
        
        #for i in range(1,len(queryfiles)+1):
        for m in range(1,1+1):
        
            #thisquery = random.choice(qchoices)
            thisquery = self.i
            thisquerytext = qdict[thisquery]['query']
            print('Thread: %d Query: %d: %s' % (self.t, thisquery, thisquerytext))
            rrows = 0
        
            if opts.platform.lower() == 'kusto':
                while True:
                    try:
                        starttime = datetime.now()
                        r = kusto_exec(thisquerytext)
                        endtime = datetime.now()
                    except:
                        continue
                    break
                for row in r:
                    j = json.loads(str(row))
                cd = (endtime - starttime).total_seconds()
                try:
                    k = json.loads(str(j["data"][-1]["Payload"]))
                except:
                    sd = cd
                else:
                    sd = k["ExecutionTime"]
                for row in r.primary_results:
                    j = json.loads(str(row))
                    rrows = len(j["data"])
                    if rrows:
                        frow = j["data"][0]
                if opts.users == 1 and opts.echoresults:
                    print('Result(s):')
                    if rrows:
                        print(j["data"])
                    else:
                        print('None')
        
            if opts.platform.lower() == 'athena':
                starttime = datetime.now()
                r = athena_exec(thisquerytext)
                endtime = datetime.now()
                if r:
                    for row in r:
                        if not rrows:
                            frow = row
                        rrows = rrows + 1
                else:
                    continue
                cd = (endtime - starttime).total_seconds()
                sd = cd
                if opts.users == 1 and opts.echoresults:
                    print('Result(s):')
                    if frow:
                        print(frow)
                    else:
                        print('None')
    
            elif opts.platform.lower() == 'elasticsearch':
                severalqueries = thisquerytext.split(';')
                starttime = datetime.now()
                for subquery in severalqueries:
                    r = elasticsearch_exec(subquery)
                endtime = datetime.now()
                cd = (endtime - starttime).total_seconds()
                if r:
                    sd = cd
                else:
                     sd = 9999.999
                rrows = 1
                frow = ''
                if opts.users == 1 and opts.echoresults:
                    print('Result:')
                    if rrows:
                        print(r.text)
                    else:
                        print('None')
        
            elif opts.platform.lower() == 'bigquery':
                starttime = datetime.now()
#                 bqj = bigquery_exec(thisquerytext)                
                bqj = bq.query(thisquerytext, job_config=cacheoff)
                try:
                    r = bqj.result(timeout=opts.timeout)
                except:
                    print('WARN: %d sec timeout exceeded...cancelling query' % opts.timeout)
                    r = []
                    bqj.cancel()
                    while not (bqj.done()):
                        time.sleep(1)
                    endtime = datetime.now()
                    sd = opts.timeout
                    cd = opts.timeout
                else:
                    endtime = datetime.now()
                    sd = (bqj.ended - bqj.started).total_seconds()
                    cd = (endtime - starttime).total_seconds()
                for row in r:
                    if not rrows:
                        frow = row[0]
                    rrows = rrows + 1
                    if opts.users == 1 and opts.echoresults:
                        print('Result(s):')
                        print(*row, sep = "|")
        
            elif opts.platform.lower() == 'snowflake':
                starttime = datetime.now()
                try:
                    sf.execute(thisquerytext, timeout=opts.timeout)
                except ProgrammingError as db_ex:
                    print(f'Timeout error: {db_ex}')
                    endtime = datetime.now()
                    r = []
                    cd = opts.timeout
                    sd = opts.timeout
                except Exception as ex:
                    raise
                else:
                    r = sf.fetchall()
                    endtime = datetime.now()
                    cd = (endtime - starttime).total_seconds()
                    sd = cd
                for row in r:
                    if not rrows:
                        frow = row[0]
                    rrows = rrows + 1
                    if opts.users == 1 and opts.echoresults:
                        print('Result(s):')
                        print(*row, sep = "|")

        
            totalqueriescompleted = totalqueriescompleted + 1
            totaltimedelta = (datetime.today() - testbegan)
            totaltimeelapsed = totaltimedelta.days * 24 * 3600 + totaltimedelta.seconds + totaltimedelta.microseconds / 1000000
            qph = totalqueriescompleted / totaltimeelapsed * 3600
        
            print('Thread: %d Completed %d queries in %.3f seconds' % (self.t, totalqueriescompleted, totaltimeelapsed))
            print('QPH: %.1f' % (qph))

            if opts.users > 1 and opts.echoresults:
                print('Thread: %3d   Duration: %.3f seconds. Result: %s' % (self.t, sd, r))
            elif opts.users == 1:
                if not opts.echoresults:
                    print('First Result: {}'.format(frow))
                print("Returned: {} Row(s)".format(rrows))
                print('Duration: %.3f seconds.' % (sd))

            if opts.report:
                writer.writerow({
                    'platform': (platforms[opts.platform.lower()]['name'] + ' ' + platforms[opts.platform.lower()]['version']).rstrip(),
                    'location': platforms[opts.platform.lower()]['location'], 
                    'endpoint': platforms[opts.platform.lower()]['endpoint'], 
                    'data_size': opts.datasize.upper(), 
                    'users': opts.users, 
                    'thread': self.t, 
                    'query_num': thisquery,
                    'query_name': qdict[thisquery]['name'],
                    'query_text': thisquerytext,
                    'result_rows': rrows,
                    'starttime': starttime.isoformat(),
                    'endtime': endtime.isoformat(),
                    'client_duration': ('%.3f' % cd),
                    'server_duration': ('%.3f' % sd),
                    'totalqueriescompleted': totalqueriescompleted,
                    'totaltimeelapsed': totaltimeelapsed,
                    'qph': ('%.1f' % qph)
                })
                csvfile.flush()
            
            if totaltimeelapsed > 7200:
                break

def get_var_char_values(d):
    return [obj['VarCharValue'] for obj in d['Data']]

def athena_exec(q):
    params = {
        'region': 'us-west-1',
        'database': 'default',
        'bucket': 'mcg-eventgen',
        'path': 'temp/athena/output',
        'query': q
    }
    session = boto3.Session()
    client = session.client('athena')
    response_query_execution_id = client.start_query_execution(
        QueryString = params['query'],
        QueryExecutionContext = {
            'Database' : "default"
        },
        ResultConfiguration = {
            'OutputLocation': 's3://' + params['bucket'] + '/' + params['path']
        }
    )
    response_get_query_details = client.get_query_execution(
        QueryExecutionId = response_query_execution_id['QueryExecutionId']
    )
    status = 'RUNNING'

    while True:
        response_get_query_details = client.get_query_execution(
        QueryExecutionId = response_query_execution_id['QueryExecutionId']
        )
        status = response_get_query_details['QueryExecution']['Status']['State']
    
        if (status == 'FAILED') or (status == 'CANCELLED') :
            failure_reason = response_get_query_details['QueryExecution']['Status']['StateChangeReason']
            print(failure_reason)
            return False

        elif status == 'SUCCEEDED':
            location = response_get_query_details['QueryExecution']['ResultConfiguration']['OutputLocation']

            ## Function to get output results
            response_query_result = client.get_query_results(
                QueryExecutionId = response_query_execution_id['QueryExecutionId']
            )
            result_data = response_query_result['ResultSet']
        
            if len(response_query_result['ResultSet']['Rows']) > 1:
                s3 = boto3.resource('s3')
                s3obj = s3.Object( params['bucket'] , params['path'] + '/' + response_query_execution_id['QueryExecutionId'] + '.csv')
                result = s3obj.get()["Body"].read().decode('utf-8').split('\n')
                return result
            else:
                return None
        time.sleep(1)

def kusto_exec(q):
    return kc.execute(kusto_db, q)
    

def elasticsearch_exec(q):
# Uncomment these lines to use the Python elasticsearch connector and comment out the requests.post method below
#     qobj = {"query": q}
#     try:
#         return es.sql.query(qobj)
#     except:
#         return False
#     return es.search(body=q, index=elasticsearchIndex)
# Uncomment this line to use the requests.post method and comment out the Python elasticsearch connector method below
    return requests.post(platforms['elasticsearch']['endpoint'] + '/_sql?format=json', auth=(platforms['elasticsearch']['user'], platforms['elasticsearch']['pwd']), json={"query": q})

def bigquery_exec(q):
    bqjob = bq.query(q, job_config=cacheoff)
    return bqjob

def main():

    txns = []
    

    for i in range(1,len(queryfiles)+1):
        for t in range(1, opts.users + 1):
    
            txn = qthread(t, i)
            txn.start()
            print("Thread %d start" % t)
            txns.append(txn)
        
        for txn in txns:
            txn.join()

            

    if opts.report:
        csvfile.close()
        print("File closed")
    
    return 0
    
if __name__ == "__main__":
    sys.exit(main())
