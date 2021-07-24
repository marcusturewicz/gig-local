// Name of this app
param appName string

// Environment name for deployment
param environment string

// Storage account container name
param containerName string

// Web App params
@allowed([
    'F1'
])
param webAppSku string

@minValue(1)
param webAppSkuCapacity int

// Data params
param sqlAdminLogin string

@secure()
param sqlAdminPassword string

param sqlDatabaseName string

@allowed([
    'Basic'
])
param sqlDatabaseSku string

var location = resourceGroup().location
var resourceNameSuffix = '${appName}${environment}${uniqueString(resourceGroup().id)}'

// Variables
var hostingPlanName = 'HostingPlan${resourceNameSuffix}'
var websiteName = 'Website${resourceNameSuffix}'
var sqlserverName = 'SqlServer${resourceNameSuffix}'
var storageName = 'storage${toLower(resourceNameSuffix)}'
var appInsightsName = 'AppInsights${resourceNameSuffix}'

// Storage account
resource storageAccount 'Microsoft.Storage/storageAccounts@2021-04-01' = {
    name: storageName
    location: location
    sku: {
        name: 'Standard_LRS'
    }
    kind: 'StorageV2'
}
resource storageAccountContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2021-04-01' = {
    name: '${storageAccount.name}/default/${containerName}'
}


// Data resources
resource sqlserver 'Microsoft.Sql/servers@2021-02-01-preview' = {
  name: sqlserverName
  location: location
  properties: {
    administratorLogin: sqlAdminLogin
    administratorLoginPassword: sqlAdminPassword
  }
}
resource sqlDatabase 'Microsoft.Sql/servers/databases@2021-02-01-preview' = {
  name: '${sqlserver.name}/${sqlDatabaseName}'
  location: location
  sku: {
    name: sqlDatabaseSku
  }
}
resource sqlServerFirewall 'Microsoft.Sql/servers/firewallRules@2021-02-01-preview' = {
  name: '${sqlserver.name}/AllowAllWindowsAzureIps'
  properties: {
    endIpAddress: '0.0.0.0'
    startIpAddress: '0.0.0.0'
  }
}

// Web App resources
resource hostingPlan 'Microsoft.Web/serverfarms@2021-01-15' = {
  name: hostingPlanName
  location: location
  kind: 'linux'
  sku: {
    name: webAppSku
    capacity: webAppSkuCapacity
  }
}
resource webSite 'Microsoft.Web/sites@2021-01-15' = {
  name: websiteName
  location: location
  tags: {
    'hidden-related:${hostingPlan.id}': 'empty'
    displayName: 'Website'
  }
  properties: {
    serverFarmId: hostingPlan.id
    siteConfig: {
        linuxFxVersion: 'DOTNETCORE|5.0'
    }
  }
}
resource webSiteConnectionStrings 'Microsoft.Web/sites/config@2021-01-15' = {
  name: '${webSite.name}/connectionstrings'
  properties: {
    DefaultConnection: {
      value: 'Data Source=tcp:${sqlserver.properties.fullyQualifiedDomainName},1433;Initial Catalog=${sqlDatabaseName};User Id=${sqlAdminLogin}@${sqlserver.properties.fullyQualifiedDomainName};Password=${sqlAdminPassword};'
      type: 'SQLAzure'
    }
  }
}

// Monitor
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  tags: {
    'hidden-link:${webSite.id}': 'Resource'
    displayName: 'AppInsightsComponent'
  }
  kind: 'web'
  properties: {
    Application_Type: 'web'
  }
}
