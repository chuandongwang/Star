using Kingdee.BOS.Core.List;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PHMX.PI.WMS.Core.Connector
{
    /// <summary>
    /// 关联源数据。
    /// </summary>
    public class DataLinkSource
    {
        /// <summary>
        /// 父项信息。
        /// </summary>
        public GenTargetArgs Parent { get; protected set; }

        /// <summary>
        /// 行主键。
        /// </summary>
        public long SId { get; set; }

        /// <summary>
        /// 单据主键。
        /// </summary>
        public long SBillId { get; set; }

        /// <summary>
        /// 通知表单标识。
        /// </summary>
        public string NoticeFormId { get; set; }

        /// <summary>
        /// 数量。
        /// </summary>
        public decimal Qty { get; set; }
        /// <summary>
        /// 重量。
        /// </summary>
        public decimal PHMXWgt { get; set; }
        /// <summary>
        /// 容量。
        /// </summary>
        public decimal Cty { get; set; }

        /// <summary>
        /// 批号。
        /// </summary>
        public string LotNo { get; set; }

        /// <summary>
        /// 生产日期。
        /// </summary>
        public DateTime? ProduceDate { get; set; }

        /// <summary>
        /// 有效期至。
        /// </summary>
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// 构造方法。
        /// </summary>
        /// <param name="args">上级对象。</param>
        public DataLinkSource(GenTargetArgs args)
        {
            this.Parent = args;
        }
    }
}
