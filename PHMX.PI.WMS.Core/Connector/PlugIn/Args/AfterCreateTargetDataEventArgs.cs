using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Metadata.ConvertElement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PHMX.PI.WMS.Core.Connector.PlugIn.Args
{
    /// <summary>
    /// 创建目标单据后事件参数。
    /// </summary>
    public class AfterCreateTargetDataEventArgs : EventArgs, IBillBenchPlugInEventArgs
    {
        /// <summary>
        /// 单据转换规则。
        /// </summary>
        public ConvertRuleElement Rule { get; set; }

        /// <summary>
        /// 操作结果。
        /// </summary>
        public IOperationResult OperationResult { get; set; }

        /// <summary>
        /// 关联数据源。
        /// </summary>
        public IEnumerable<DataLinkSource> Rows { get; set; }

        /// <summary>
        /// 构造方法。
        /// </summary>
        /// <param name="rule">单据转换规则。</param>
        /// <param name="op">操作结果。</param>
        /// <param name="rows">关联数据源。</param>
        public AfterCreateTargetDataEventArgs(ConvertRuleElement rule, IOperationResult op, IEnumerable<DataLinkSource> rows)
        {
            this.Rule = rule;
            this.OperationResult = op;
            this.Rows = rows;
        }
    }
}
