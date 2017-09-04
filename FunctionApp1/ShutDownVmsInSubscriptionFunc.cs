using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Collections.Generic;
using Microsoft.Azure;
using Microsoft.Azure.Management.Resources;
using Microsoft.Azure.Management.Resources.Models;
using System;
using Microsoft.Azure.Management.Compute;
using Microsoft.Rest;
using FunctionApp1.ShutDownVmsInSubscription;

namespace FunctionApp1
{
    public static class ShutDownVmsInSubscriptionFunc
    {
        [FunctionName("ShutdownVmsInSubscription")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");
            var input = ShutDownVmsInSubscription.FunctionInput.GetInput(log);
            var credentials =await new Tuple<string, string, string, string>(input.TenantId, input.ApplicationId, input.ApplicationSecret, input.SubscriptionId).GetSubscriptionCredentials();
            try
            {
                await StopVirtualMachines(credentials.Item1, credentials.Item2, input.ResourceGroupNameFilter, log);
            }
            catch (Exception e)
            {
                log.Error($"Error stopping VMs in subscription {e.Message}");
                return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);
            }
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        }

        private static async Task StopVirtualMachines(TokenCredentials credential, SubscriptionCloudCredentials cloudCredentials, HashSet<string> resourceGroupNameFilter, TraceWriter log)
        {
            var client = new ComputeManagementClient( credential);
            log.Info($"Succesully got client");
            IEnumerable<string> groups;
            try
            {
                client.SubscriptionId = cloudCredentials.SubscriptionId;
                groups = await GetAllGroupsInSubscription(cloudCredentials, resourceGroupNameFilter);
            }
            catch (Exception e)
            {
                log.Error($"error getting groups in subscription: {e.Message}");
                return;
            }
            foreach (var group in groups)
            {
                log.Info($"Searching for VMs in group: {group}");
                try
                {
                    var vmResult = await client.VirtualMachines.ListAsync(group);
                    foreach (var vm in vmResult)
                    {
                        log.Info($"shutting down VM {vm.Name}");
                        try
                        {
                            var result = await client.VirtualMachines.PowerOffAsync(group, vm.Name);

                            if (result.Error != null)
                             log.Error($"error shutting down vm {vm.Name}: {result.Status}, {result.Error} ");
                        }
                        catch (Exception e)
                        {
                            log.Error($"error shutting down vm {vm.Name}: {e.Message}");
                        }
                    }

                }
                catch (Exception e)
                {
                    log.Error($"error getting vms in group {group}: {e.Message}");
                }
            }
        }

        private static async Task<IEnumerable<string>> GetAllGroupsInSubscription(SubscriptionCloudCredentials credential, HashSet<string> resourceGroupNameFilter)
        {
            var client = new ResourceManagementClient(credential);
            var list = await client.ResourceGroups.ListAsync(new ResourceGroupListParameters());
            return list.ResourceGroups
                .Where(group=> resourceGroupNameFilter.Count == 0? true : resourceGroupNameFilter.Contains(group.Name))
                .Select(group => group.Name);
        }

    }
}