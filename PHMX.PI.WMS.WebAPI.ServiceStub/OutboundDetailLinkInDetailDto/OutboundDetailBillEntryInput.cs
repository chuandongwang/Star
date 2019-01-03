using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using BAH.BOS.Json;

namespace PHMX.PI.WMS.WebAPI.ServiceStub.OutboundDetailLinkInDetailDto
{
    /// <summary>
    /// 拣货明细单据体信息。
    /// </summary>
    [JsonObject]
    public class OutboundDetailBillEntryInput
    {
        /// <summary>
        /// 拣货明细单据体主键。
        /// </summary>
        public long SourceEntryId { get; set; }

        /// <summary>
        /// 拣货明细单据主键。
        /// </summary>
        public long SourceBillId { get; set; }
      
        /// <summary>
        /// 单位数量。
        /// </summary>
        public decimal Qty { get; set; }
        /// <summary>
        /// 单位。
        /// </summary>
        public string UnitId { get; set; }
        /// <summary>
        /// 容量。
        /// </summary>
        public decimal Cty { get; set; }
        /// <summary>
        /// 平均容量。
        /// </summary>
        public decimal AvgCty { get; set; }
        /// <summary>
        /// 行备注。
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 重量。
        /// </summary>
        public decimal PHMXWgt { get; set; }




    }
}
