using System.Threading.Tasks;
using Pulumi;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.ContainerService;
using Pulumi.AzureNative.ContainerService.Inputs;
using Pulumi.AzureAD;
using Pulumi.Random;

class MyStack : Stack
{
    public MyStack()
    {
        // Create an Azure Resource Group
        var resourceGroup = new ResourceGroup("resourceGroup");
        var application = new Application("pk-nodepool-app-server", new ApplicationArgs
        {
            DisplayName = "pk-nodepool-app-server"
        });

        var principal = new ServicePrincipal("pk-nodepool-service-principal", new ServicePrincipalArgs
        {
            ApplicationId = application.ApplicationId
        });

        var password = new RandomPassword("pk-nodepool-pw", new RandomPasswordArgs
        {
            Length = 20,
            Special = true
        });

        var servicePrincipalPassword = new ServicePrincipalPassword("pk-nodepool-sp-pw", new ServicePrincipalPasswordArgs
        {
            ServicePrincipalId = principal.Id,
            Value = password.Result
        });

        var cluster = new ManagedCluster("pk-nodepool-aks", new ManagedClusterArgs
        {
            ResourceGroupName = resourceGroup.Name,
            ServicePrincipalProfile = new ManagedClusterServicePrincipalProfileArgs
            {
                ClientId = application.ApplicationId,
                Secret = servicePrincipalPassword.Value
            },
            EnableRBAC = true,
            AgentPoolProfiles = 
            {
                new ManagedClusterAgentPoolProfileArgs 
                {
                    Mode = AgentPoolMode.System,
                    Name = "agentpool1",
                    Count = 2,
                    VmSize = "Standard_DS3_v2"
                },
                new ManagedClusterAgentPoolProfileArgs
                {
                    Mode = AgentPoolMode.System,
                    Name = "agentpool2",
                    Count = 2,
                    VmSize = "Standard_DS3_v2"
                }
            },
            DnsPrefix = "pknodepool"
        });
    }
}
