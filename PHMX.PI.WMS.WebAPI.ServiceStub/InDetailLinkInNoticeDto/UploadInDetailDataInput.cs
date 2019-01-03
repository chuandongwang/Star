using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace PHMX.PI.WMS.WebAPI.ServiceStub.InDetailLinkInNoticeDto
{
    public class UploadInDetailDataInput
    {
        /// <summary>
        /// 收货通知主键。
        /// </summary>
        [JsonProperty]
        public long InNoticeId { get; set; }

        /// <summary>
        /// 收货明细信息。
        /// </summary>
        [JsonProperty]
        public InDetailBillEntryInput[] InDetailBillEntries { get; set; }
    }
}
