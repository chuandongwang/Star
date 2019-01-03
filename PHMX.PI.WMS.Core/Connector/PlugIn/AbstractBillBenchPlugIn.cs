using PHMX.PI.WMS.Core.Connector.PlugIn.Args;
using Kingdee.BOS;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Metadata.ConvertElement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PHMX.PI.WMS.Core.Connector.PlugIn
{
    /// <summary>
    /// 单据工作台抽象类。
    /// </summary>
    public abstract class AbstractBillBenchPlugIn : IBillBenchPlugIn
    {
        /// <summary>
        /// 单据视图代理。
        /// </summary>
        public IBillView View { get; protected set; }

        /// <summary>
        /// 上下文对象。
        /// </summary>
        public Context Context { get; protected set; }

        /// <summary>
        /// 设置上下文对象。
        /// </summary>
        /// <param name="view">动态表单视图。</param>
        public void SetContext(IBillView view)
        {
            this.View = view;
            this.Context = view.Context;
        }

        /// <summary>
        /// 创建下推参数后事件。
        /// </summary>
        /// <param name="e">事件参数。</param>
        public virtual void AfterCreatePushArgs(AfterCreatePushArgsEventArgs e) { }

        /// <summary>
        /// 创建目标单据后事件。
        /// </summary>
        /// <param name="e">事件参数。</param>
        public virtual void AfterCreateTargetData(AfterCreateTargetDataEventArgs e) { }

        /// <summary>
        /// 上传目标单据前事件。
        /// </summary>
        /// <param name="e">事件参数。</param>
        public virtual void BeforeUploadTargetData(BeforeUploadTargetDataEventArgs e) { }

    }//end class
}//end namespace
