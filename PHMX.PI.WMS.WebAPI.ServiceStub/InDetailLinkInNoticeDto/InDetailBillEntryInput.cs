using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using BAH.BOS.Json;

namespace PHMX.PI.WMS.WebAPI.ServiceStub.InDetailLinkInNoticeDto
{
    /// <summary>
    /// 收货明细单据体信息。
    /// </summary>
    [JsonObject]
    public class InDetailBillEntryInput
    {
        /// <summary>
        /// 收货通知明细主键。
        /// </summary>
        public long SourceEntryId { get; set; }

        /// <summary>
        /// 收货通知单据主键。
        /// </summary>
        public long SourceBillId { get; set; }
        /// <summary>
        /// 包装。
        /// </summary>
        public string PackageId { get; set; }


        /// <summary>
        /// 单位数量。
        /// </summary>
        public decimal Qty { get; set; }
        /// <summary>
        /// 单位。
        /// </summary>
        public string UnitId { get; set; }
        /// <summary>
        /// 跟踪号。
        /// </summary>
        public string TrackNo { get; set; }

        /// <summary>
        /// 批号。
        /// </summary>
        public string BatchNo { get; set; }

        /// <summary>
        /// 生产入库日期。
        /// </summary>
        public DateTime? KFDate { get; set; }

        /// <summary>
        /// 库位。
        /// </summary>
        public string LocId { get; set; }
        /// <summary>
        /// 容量。
        /// </summary>
        public decimal Cty { get; set; }
        /// <summary>
        /// 平均容量。
        /// </summary>
        public decimal AvgCty { get; set; }
        /// <summary>
        /// 有效期。
        /// </summary>
        public int ExpPeriod { get; set; }
        /// <summary>
        /// 有效期单位。
        /// </summary>
        public string ExpUnit { get; set; }

        /// <summary>
        /// 重量。
        /// </summary>
        public decimal PHMXWgt { get; set; }

    }
}
