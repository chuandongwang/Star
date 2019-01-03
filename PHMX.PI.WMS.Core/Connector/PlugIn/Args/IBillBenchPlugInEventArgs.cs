using Kingdee.BOS.Core.Metadata.ConvertElement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PHMX.PI.WMS.Core.Connector.PlugIn.Args
{
    /// <summary>
    /// 单据工作台插件事件参数接口。
    /// </summary>
    public interface IBillBenchPlugInEventArgs
    {
        /// <summary>
        /// 单据转换规则。
        /// </summary>
        ConvertRuleElement Rule { get; }
    }
}
