using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestSharp;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ScaleService
{
    public class GetWtParams
    {
        public GetWtParams(string scaleIP, string inOrOut)
        {
            this.wt_ip = scaleIP;
            this.in_out_type = inOrOut;
        }

        public string wt_ip { get; set; }
        public string in_out_type { get; set; }
    }

    public class GatePassParams : GetWtParams
    {
        public GatePassParams(string scaleIP, string inOrOut, string tk_no, string wt_num) : base(scaleIP, inOrOut)
        {
            this.tk_no = tk_no;
            this.wt_num = wt_num;
        }

        public string tk_no { get; set; }
        public string wt_num { get; set; }
    }

    public class GetWtResponse
    {
        public string status { get; set; }
        public string tk_no { get; set; }
        public string wt_num { get; set; }
    }
    public class GatePassResponse
    {
        public string status { get; set; }
        public string reason { get; set; }
    }

    public class ScaleRestClient : IDisposable
    {
        private readonly RestClient restClient;

        public ScaleRestClient(ILogger<Worker> logger, IConfiguration configRoot)
        {
            this.restClient = new RestClient(configRoot["ServiceUri"]);
        }

        public async Task<GetWtResponse> GetWtAsync(string scaleIP, string inOrOut)
        {
            try
            {
                var request = new RestRequest("getWt", Method.POST);
                request.AddJsonBody(new { wt_ip = scaleIP, in_out_type = inOrOut});
                var response = await restClient.ExecuteAsync<GetWtResponse>(request);
                if (response.StatusCode == HttpStatusCode.OK)
                    return response.Data;
                else
                    throw new Exception("Service通讯失败");
            }
            catch(Exception e)
            {
                //Logger.Trace("发生异常：{0}", e.Message);
                throw new Exception("Service通讯失败："+e.Message);
            }
        }

        public async Task<GatePassResponse> GatePassAsync(string scaleIP, string inOrOut, string tk_no, string wt_num)
        {
            try
            {
                var request = new RestRequest("gatePass", Method.POST);
                request.AddJsonBody(new { wt_ip = scaleIP, in_out_type = inOrOut, tk_no = tk_no, wt_num = wt_num });
                var response = await restClient.ExecuteAsync<GatePassResponse>(request);
                return response.Data;
            }
            catch (Exception e)
            {
                //Logger.Trace("发生异常：{0}", e.Message);
                throw new Exception("Service通讯失败：" + e.Message);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //restClient.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~RestClient()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
