using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace PHMX.PI.WMS.WebAPI.ServiceStub.PickDetailLinkInDetailDto
{
    public class UploadPickDetailDataInput
    {
        /// <summary>
        /// 发货明细主键。
        /// </summary>
        [JsonProperty]
        public long OutNoticeId { get; set; }

        /// <summary>
        /// 收货明细信息。
        /// </summary>
        [JsonProperty]
        public PickDetailBillEntryInput[] PickDetailBillEntries { get; set; }
    }
}
