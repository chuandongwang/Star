using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PHMX.PI.WMS.Core.Connector
{
    /// <summary>
    /// 生成目标单据所需的参数。
    /// </summary>
    public class GenTargetArgs
    {
        /// <summary>
        /// 单据类型。
        /// </summary>
        public string ObjectTypeId { get; set; }

        /// <summary>
        /// 单据主键。
        /// </summary>
        public object BillId { get; set; }

        /// <summary>
        /// 单据编号。
        /// </summary>
        public string BillNo { get; set; }

        /// <summary>
        /// 来源单据。
        /// </summary>
        public string SourceFormId { get; set; }

        /// <summary>
        /// 目标单据。
        /// </summary>
        public string TargetFormId { get; set; }

        /// <summary>
        /// 转换规则。
        /// </summary>
        public string ConvertRuleId { get; set; }

        /// <summary>
        /// 业务日期。
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// 明细数据行。
        /// </summary>
        public List<DataLinkSource> DataRows { get; protected set; }

        /// <summary>
        /// 构造方法。
        /// </summary>
        public GenTargetArgs()
        {
            this.Date = DateTime.Now;
            this.DataRows = new List<DataLinkSource>();
        }
    }
}
