use IdentityService
Declare @ClientID NVarChar(100) = 'SAEON-CKAN-webtest'
Delete ClientClaims where ClientID = (Select Id from Clients where ClientId = @ClientID)
Delete ClientCorsOrigins where ClientID = (Select Id from Clients where ClientId = @ClientID)
Delete ClientGrantTypes where ClientID = (Select Id from Clients where ClientId = @ClientID)
Delete ClientIdPRestrictions where ClientID = (Select Id from Clients where ClientId = @ClientID)
Delete ClientPostLogoutRedirectUris where ClientID = (Select Id from Clients where ClientId = @ClientID)
Delete ClientProperties where ClientID = (Select Id from Clients where ClientId = @ClientID)
Delete ClientRedirectUris where ClientID = (Select Id from Clients where ClientId = @ClientID)
Delete ClientScopes where ClientID = (Select Id from Clients where ClientId = @ClientID)
Delete ClientSecrets where ClientID = (Select Id from Clients where ClientId = @ClientID)
Delete Clients where (ClientID = @ClientID)
