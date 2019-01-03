using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace PHMX.PI.WMS.WebAPI.ServiceStub.PutDetailLinkInDetailDto
{
    public class UploadPutDetailDataInput
    {
       
        /// <summary>
        /// 收货明细信息。
        /// </summary>
        [JsonProperty]
        public PutDetailBillEntryInput[] PutDetailBillEntries { get; set; }
    }
}
