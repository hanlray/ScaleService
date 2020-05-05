using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ScaleService
{
    public class UniOPManager
    {
        public UniOPManager(
            ILogger<Worker> logger,
            IConfiguration configRoot,
            ScaleRestClient restClient,
            RelayWatcher relayWatcher)
        {


        }
    }
}