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
    /// 单据工作台接口，用于处理回传时数据的处理。
    /// </summary>
    public interface IBillBenchPlugIn
    {
        /// <summary>
        /// 单据视图代理。
        /// </summary>
        IBillView View { get; }

        /// <summary>
        /// 上下文对象。
        /// </summary>
        Context Context { get; }

        /// <summary>
        /// 设置上下文对象。
        /// </summary>
        /// <param name="view">动态表单视图。</param>
        void SetContext(IBillView view);

        /// <summary>
        /// 创建下推参数后事件。
        /// </summary>
        /// <param name="e">事件参数。</param>
        void AfterCreatePushArgs(AfterCreatePushArgsEventArgs e);

        /// <summary>
        /// 创建目标单据后事件。
        /// </summary>
        /// <param name="e">参数对象。</param>
        void AfterCreateTargetData(AfterCreateTargetDataEventArgs e);

        /// <summary>
        /// 上传目标单据前事件。
        /// </summary>
        /// <param name="e">参数对象。</param>
        void BeforeUploadTargetData(BeforeUploadTargetDataEventArgs e);
    }
}
