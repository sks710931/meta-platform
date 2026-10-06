# Standalone lightweight persistence/API smoke check; standard-library Python only.
import urllib.request,urllib.error,json,uuid,concurrent.futures
import argparse
parser = argparse.ArgumentParser(description="Writes test fixtures: use an empty, isolated database and a Development API host.")
parser.add_argument("--base-url", required=True)
base = parser.parse_args().base_url.rstrip("/")
def req(method,path,payload=None):
 request=urllib.request.Request(base+path,data=json.dumps(payload).encode() if payload is not None else None,method=method,headers={'Content-Type':'application/json'})
 try:r=urllib.request.urlopen(request)
 except urllib.error.HTTPError as e:r=e
 body=r.read();data=json.loads(body) if body else None
 return r.status,data
assert req('GET', '/api/organizations') == (200, []), 'Requires an empty isolated validation database.'
status,org=req('POST','/api/organizations',{'name':'Onboarding validation'});assert status==201
status,other=req('POST','/api/organizations',{'name':'Other tenant'});assert status==201
orgId=org['organizationId'];otherId=other['organizationId']
def start(orgid=orgId):
 status,session=req('POST',f'/api/organizations/{orgid}/whatsapp/onboarding-sessions');assert status==201,(status,session)
 assert session['status']=='Pending' and session['completedAt'] is None and session['manualCompletionAvailable']
 return session['sessionId']
def data(waba='1001',messaging='2001',phone='3001'):
 return {'externalWhatsAppAccountId':waba,'externalMessagingAccountId':messaging,'displayName':'  Example account ',
 'phoneNumbers':[{'externalPhoneNumberId':phone,'displayPhoneNumber':'+1 555 0100','verifiedName':'Example'}, {'externalPhoneNumberId':str(int(phone)+1),'displayPhoneNumber':'+1 555 0101','verifiedName':None}]}
def complete(session,payload):return req('POST',f'/api/whatsapp/onboarding-sessions/{session}/complete',payload)
sid=start();payload=data()
status,result=complete(sid,payload);assert status==201,(status,result)
assert result['session']['status']=='Completed' and result['session']['completedAt'] is not None
account=result['account'];accountId=account['whatsAppAccountId'];assert len(account['phoneNumbers'])==2 and account['displayName']=='Example account'
assert accountId!=payload['externalWhatsAppAccountId']
status,replay=complete(sid,payload);assert status==200 and replay['alreadyCompleted'] and replay['account']==account
status,detail=req('GET',f'/api/whatsapp-accounts/{accountId}');assert status==200 and detail==account
status,items=req('GET',f'/api/organizations/{orgId}/whatsapp-accounts');assert status==200 and len(items)==1
status,items=req('GET',f'/api/organizations/{otherId}/whatsapp-accounts');assert status==200 and items==[]
status,result=complete(sid,data('1009','2009','3009'));assert status==409
# Global uniqueness collisions for each external-ID type must roll back the whole graph and keep the session pending.
for payload in [data('1001','2011','3011'),data('1011','2001','3021'),data('1021','2021','3001')]:
 conflict=start(otherId);status,error=complete(conflict,payload);assert status==409,(status,error)
 status,state=req('GET',f'/api/whatsapp/onboarding-sessions/{conflict}');assert status==200 and state['status']=='Pending' and state['completedAt'] is None
status,items=req('GET',f'/api/organizations/{otherId}/whatsapp-accounts');assert status==200 and items==[]
invalid=start()
for payload in [data('invalid'),{**data('1041','2041','3041'),'phoneNumbers':[]}, {**data('1041','2041','3041'),'accessToken':'must-not-be-accepted'}]:
 status,error=complete(invalid,payload);assert status==400,(status,error)
status,state=req('GET',f'/api/whatsapp/onboarding-sessions/{invalid}');assert state['status']=='Pending'
# Concurrent identical callbacks must converge to one internal account.
concurrentSession=start();concurrentPayload=data('1051','2051','3051')
with concurrent.futures.ThreadPoolExecutor(max_workers=6) as pool:
 results=list(pool.map(lambda _:complete(concurrentSession,concurrentPayload),range(6)))
assert all(status in (200,201) for status,_ in results),results
assert sum(status==201 for status,_ in results)==1
assert len({body['account']['whatsAppAccountId'] for _,body in results})==1
for path in ['/api/whatsapp/onboarding-sessions/not-a-uuid','/api/whatsapp-accounts/not-a-uuid']:
 assert req('GET',path)[0]==400
assert req('POST',f'/api/organizations/{uuid.uuid4()}/whatsapp/onboarding-sessions')[0]==404
assert req('GET',f'/api/whatsapp/onboarding-sessions/{uuid.uuid4()}')[0]==404
assert req('GET',f'/api/whatsapp-accounts/{uuid.uuid4()}')[0]==404
print('PASS: start, complete, persisted graph, same-payload replay, conflicting replay, tenant-scoped list isolation, global uniqueness for all three external IDs, rollback, malformed requests, sensitive-field rejection, 6 concurrent callbacks, 400/404')
