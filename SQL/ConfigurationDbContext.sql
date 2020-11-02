--ApiScopes -> ApiResourceScopes
Insert into ApiResourceScopes
  (Scope, ApiResourceId)
Select
  ApiResources.Name, ApiResources.Id
from
  ApiResources

--ApiSecrets -> ApiResourceSecrets
SET IDENTITY_INSERT ApiResourceSecrets ON;  
GO

INSERT INTO ApiResourceSecrets
 (Id, [Description], [Value], Expiration, [Type], Created, ApiResourceId)
SELECT 
 Id, [Description], [Value], Expiration, [Type], Created, ApiResourceId
FROM ApiSecrets
GO

SET IDENTITY_INSERT ApiResourceSecrets OFF;  
GO


--IdentityClaims -> IdentityResourceClaims
SET IDENTITY_INSERT IdentityResourceClaims ON;  
GO

INSERT INTO IdentityResourceClaims
 (Id, [Type], IdentityResourceId)
SELECT 
 Id, [Type], IdentityResourceId
FROM IdentityClaims
GO

SET IDENTITY_INSERT IdentityResourceClaims OFF;  
GO

DROP TABLE ApiSecrets
DROP TABLE IdentityClaims

Update
  ClientCorsOrigins
set
  Origin = Replace(Origin,'/ndao','')
where
  Origin like '%/ndao'

Update ApiScopes set Enabled = 1