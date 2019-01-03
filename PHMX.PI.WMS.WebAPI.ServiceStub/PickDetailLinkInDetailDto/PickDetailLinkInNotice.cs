using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace PHMX.PI.WMS.WebAPI.ServiceStub.PickDetailLinkInDetailDto
{
    public class PickDetailLinkInNotice
    {
        /// <summary>
        /// 发货通知主键。
        /// </summary>
        public long OutNoticeId { get; set; }

        /// <summary>
        /// 收货明细数据行关联到货通知数组。
        /// </summary>
        public PickDetailEntryLinkInNoticePlus[] PickDetailEntries { get; set; }
    }
}
