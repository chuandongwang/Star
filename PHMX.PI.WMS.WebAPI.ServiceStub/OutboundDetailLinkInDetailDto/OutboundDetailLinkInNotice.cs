using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace PHMX.PI.WMS.WebAPI.ServiceStub.OutboundDetailLinkInDetailDto
{
    public class OutboundDetailLinkInNotice
    {
        /// <summary>
        /// 拣货通知主键。
        /// </summary>
        public long PickDetailId { get; set; }

        /// <summary>
        /// 发货明细数据行关联拣货明细数组。
        /// </summary>
        public OutboundDetailEntryLinkInNoticePlus[] OutboundDetailEntries { get; set; }
    }
}
