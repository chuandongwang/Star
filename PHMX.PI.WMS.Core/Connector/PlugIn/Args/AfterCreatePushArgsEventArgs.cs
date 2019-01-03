using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Metadata.ConvertElement;
using Kingdee.BOS.Core.Metadata.ConvertElement.ServiceArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PHMX.PI.WMS.Core.Connector.PlugIn.Args
{
    /// <summary>
    /// 创建下推参数后事件参数。
    /// </summary>
    public class AfterCreatePushArgsEventArgs : EventArgs, IBillBenchPlugInEventArgs
    {
        /// <summary>
        /// 单据转换规则。
        /// </summary>
        public ConvertRuleElement Rule { get; set; }

        /// <summary>
        /// 下推参数对象。
        /// </summary>
        public PushArgs PushArgs { get; set; }

        /// <summary>
        /// 构造方法。
        /// </summary>
        /// <param name="rule">单据转换规则。</param>
        /// <param name="op">操作结果。</param>
        /// <param name="rows">关联数据源。</param>
        public AfterCreatePushArgsEventArgs(ConvertRuleElement rule, PushArgs pushArgs)
        {
            this.Rule = rule;
            this.PushArgs = pushArgs;
        }
    }
}
