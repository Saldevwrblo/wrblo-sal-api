
param location string = resourceGroup().location

param appName string = 'myapplication'

param environmentName string = 'production'

param acrName string

param logAnalyticsName string

param containerAppName string

param imageName string = 'myapplication'

param imageTag string = 'latest'

param cpu int = 0.5

param memory string = '1Gi'

@secure()
param databaseConnectionString string

// Container Registry

resource acr 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  name: acrName
  location: location
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: false
  }
}

// Monitoring

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: logAnalyticsName
  location: location
  properties: {
    retentionInDays: 30
    sku: {
      name: 'PerGB2018'
    }
  }
}

// Container Apps Environment

resource containerEnvironment 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: environmentName
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
    }
  }
}

// Container Application

resource containerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: containerAppName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    managedEnvironmentId: containerEnvironment.id

    configuration: {
      activeRevisionsMode: 'Single'

      registries: [
        {
          server: acr.properties.loginServer
          identity: 'system'
        }
      ]

      secrets: [
        {
          name: 'database-connection'
          value: databaseConnectionString
        }
      ]

      ingress: {
        external: true
        targetPort: 8080
        transport: 'http'
      }
    }

    template: {
      containers: [
        {
          name: appName
          image: '${acr.properties.loginServer}/${imageName}:${imageTag}'

          resources: {
            cpu: cpu
            memory: memory
          }

          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: 'Production'
            }
            {
              name: 'ASPNETCORE_HTTP_PORTS'
              value: '8080'
            }
            {
              name: 'ConnectionStrings__DefaultConnection'
              secretRef: 'database-connection'
            }
          ]

          probes: [
            {
              type: 'Liveness'
              httpGet: {
                path: '/health/live'
                port: 8080
              }
              initialDelaySeconds: 10
              periodSeconds: 30
            }
            {
              type: 'Readiness'
              httpGet: {
                path: '/health/ready'
                port: 8080
              }
              initialDelaySeconds: 10
              periodSeconds: 15
            }
          ]
        }
      ]

      scale: {
        minReplicas: 1
        maxReplicas: 3
      }
    }
  }
}

output acrLoginServer string = acr.properties.loginServer

output containerAppUrl string = containerApp.properties.configuration.ingress.fqdn
