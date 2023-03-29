using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Kugar.Core.Configuration;
using Kugar.Core.ExtMethod;
using Kugar.Core.Log;
using Kugar.Server.MonitorCollectorRunner.Helpers;
using Kugar.Server.MonitorCollectors.Core;
using Newtonsoft.Json;

namespace Kugar.Server.MonitorCollectorRunner.Submitters
{
    public class HttpSubmitter:IDataSubmitter
    {
        private string _loginUrl="";
        private string _postDataUrl = "";
        private string _token = "";

        public HttpSubmitter()
        {
            var host = CustomConfigManager.Default["Submitter:Host"];
            var projectId = CustomConfigManager.Default["Submitter:ProjectId"];

            _loginUrl = $"{host}/CollectorApi/Users/Login";
            _postDataUrl = $"{host}/CollectorApi/Project/Data/UploadData/{projectId}";

        }

        public async Task Submit(IEnumerable<IEventDataBase> dataList)
        { 
            await Task.WhenAll(dataList.Select(postData)); 
        }

        public async Task postData(IEventDataBase data)
        {
            var json = data.Serialize();
            json.Remove("typeId");

            var response=await _postDataUrl
                .SetQueryParam("typeId", data.TypeId)
                .WithOAuthBearerToken(_token) 
                .PostJsonAsync(json)
                ;

            var ret =await response.GetResultReturn();

            if (!ret)
            {
                LoggerManager.Default.Error($"数据上传失败={data.TypeId}:\n{json}\n{JsonConvert.SerializeObject(ret)}");
            }
        }

        public async Task Init()
        {
            var result=await _loginUrl
                .PostJsonAsync(new
                {
                    loginUserName = CustomConfigManager.Default["Submitter:UserName"],
                    password = CustomConfigManager.Default["Submitter:Password"],
                })
                ;

            var ret = await result.GetResultReturn<string>();

            if (ret)
            {
                _token = ret.ReturnData;
            }
            else
            {
                Console.WriteLine("登录信息错误");
                return;
            }

            FlurlHttp.Configure(x =>
            {
                x.BeforeCall = (item) => item.RedirectedFrom.HttpRequestMessage.Headers.Add("Authorization", _token);
            });
        }
    }
}
