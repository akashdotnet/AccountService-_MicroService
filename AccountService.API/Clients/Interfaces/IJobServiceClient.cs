using System.Collections.Generic;
using System.Threading.Tasks;
using AccountService.API.Dto.JobServiceClient;

namespace AccountService.API.Clients.Interfaces;

public interface IJobServiceClient
{
    Task<List<WorkOrderResponse>> GetWorkOrdersByBusinessLocationIds(List<int> businessLocationIds);
}
