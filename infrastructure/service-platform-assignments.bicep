param platformResourcePrefix string
param environmentResourcePrefix string
param serviceName string

resource acr 'Microsoft.ContainerRegistry/registries@2021-09-01' existing = {
  name: replace('${platformResourcePrefix}-registry', '-', '')
}

resource svcUser 'Microsoft.ManagedIdentity/userAssignedIdentities@2022-01-31-preview' existing = {
  name: '${environmentResourcePrefix}-svc-${serviceName}'
  scope: resourceGroup('${environmentResourcePrefix}-svc-${serviceName}')
}

// Allow the service to pull images from the Azure Container Registry
resource svcUserAcrPull 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid('acrPull', svcUser.id)
  scope: acr
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')
    principalId: svcUser.properties.principalId
    principalType: 'ServicePrincipal'
  }
}
