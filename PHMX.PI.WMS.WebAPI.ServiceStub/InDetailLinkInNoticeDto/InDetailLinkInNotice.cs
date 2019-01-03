using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace PHMX.PI.WMS.WebAPI.ServiceStub.InDetailLinkInNoticeDto
{
    public class InDetailLinkInNotice
    {
        /// <summary>
        /// 到货通知主键。
        /// </summary>
        public long InNoticeId { get; set; }

        /// <summary>
        /// 收货明细数据行关联到货通知数组。
        /// </summary>
        public InDetailEntryLinkInNoticePlus[] InDetailEntries { get; set; }
    }
}
