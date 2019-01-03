using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using BAH.BOS.Json;

namespace PHMX.PI.WMS.WebAPI.ServiceStub.PickDetailLinkInDetailDto
{
    /// <summary>
    /// 拣货明细单据体信息。
    /// </summary>
    [JsonObject]
    public class PickDetailBillEntryInput
    {
        /// <summary>
        /// 收货明细单据体主键。
        /// </summary>
        public long SourceEntryId { get; set; }

        /// <summary>
        /// 收货明细单据主键。
        /// </summary>
        public long SourceBillId { get; set; }
        /// <summary>
        /// 包装。
        /// </summary>
        public string ToPackageId { get; set; }


        /// <summary>
        /// 单位数量。
        /// </summary>
        public decimal ToQty { get; set; }
        /// <summary>
        /// 单位。
        /// </summary>
        public string ToUnitId { get; set; }
        /// <summary>
        /// 移出跟踪号。
        /// </summary>
        public string FromTrackNo { get; set; }
        /// <summary>
        /// 移入跟踪号。
        /// </summary>
        public string ToTrackNo { get; set; }
        /// <summary>
        /// 目标库位。
        /// </summary>
        public string ToLocId { get; set; }
        /// <summary>
        /// 批号。
        /// </summary>
        public string BatchNo { get; set; }

        /// <summary>
        /// 容量。
        /// </summary>
        public decimal ToCty { get; set; }

        /// <summary>
        /// 平均容量。
        /// </summary>
        public decimal ToAvgCty { get; set; }
        /// <summary>
        /// 有效期。
        /// </summary>
        public int ExpPeriod { get; set; }
        /// <summary>
        /// 有效期单位
        /// </summary>
        public string ExpUnit { get; set; }

        /// <summary>
        /// 源库位。
        /// </summary>
        public string FromLocId { get; set; }
        /// <summary>
        /// 生产入库日期。
        /// </summary>
        public DateTime? KFDate { get; set; }
        /// <summary>
        /// 重量。
        /// </summary>
        public decimal PHMXWgt { get; set; }

    }
}
