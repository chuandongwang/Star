using Kingdee.BOS.Core.Metadata.ConvertElement;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PHMX.PI.WMS.Core.Connector.PlugIn.Args
{
    /// <summary>
    /// 上传目标单据前事件参数。
    /// </summary>
    public class BeforeUploadTargetDataEventArgs : EventArgs, IBillBenchPlugInEventArgs
    {
        /// <summary>
        /// 单据转换规则。
        /// </summary>
        public ConvertRuleElement Rule { get; set; }

        /// <summary>
        /// 数据包。
        /// </summary>
        public DynamicObject[] DataEntities { get; set; }

        /// <summary>
        /// 生成目标单据参数。
        /// </summary>
        public GenTargetArgs Argument { get; set; }

        /// <summary>
        /// 操作额外参数。
        /// </summary>
        public OperateOption Option { get; set; }

        /// <summary>
        /// 构造方法。
        /// </summary>
        /// <param name="rule">单据转换规则。</param>
        /// <param name="dataEntities">数据包。</param>
        /// <param name="argument">生成目标单据参数。</param>
        /// <param name="option">操作额外参数。</param>
        public BeforeUploadTargetDataEventArgs(ConvertRuleElement rule, DynamicObject[] dataEntities, GenTargetArgs argument, OperateOption option)
        {
            this.Rule = rule;
            this.DataEntities = dataEntities;
            this.Argument = argument;
            this.Option = option;
        }

    }//end class
}//end namespace
