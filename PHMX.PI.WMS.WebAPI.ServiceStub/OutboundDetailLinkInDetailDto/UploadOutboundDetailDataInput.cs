using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace PHMX.PI.WMS.WebAPI.ServiceStub.OutboundDetailLinkInDetailDto
{
    public class UploadOutboundDetailDataInput
    {
        /// <summary>
        /// 拣货明细主键。
        /// </summary>
        [JsonProperty]
        public long PickDetailId { get; set; }

        /// <summary>
        /// 拣货明细信息。
        /// </summary>
        [JsonProperty]
        public OutboundDetailBillEntryInput[] OutboundDetailBillEntries { get; set; }
    }
}
