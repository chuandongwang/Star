using PHMX.PI.WMS.Core.Connector.PlugIn.Args;
using Kingdee.BOS;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.PlugInProxy;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PHMX.PI.WMS.Core.Connector.PlugIn
{
    /// <summary>
    /// 单据工作台代理类。
    /// </summary>
    public class BillBenchPlugInProxy : AbstractPlugInProxy<Tuple<string, IBillBenchPlugIn>>
    {
        protected BillBenchPlugInRegistration Registration { get; set; }

        public BillBenchPlugInProxy()
        {
            this.Registration = TypesContainer.GetOrRegisterSingletonInstance("PHMX.PI.WMS.Core.Connector.PlugIn.BillBenchPlugInRegistration,PHMX.PI.WMS.Core").AsType<BillBenchPlugInRegistration>();
        }

        protected void InvokeMethod(IBillBenchPlugInEventArgs e, Action<IBillBenchPlugIn> action)
        {
            var formId = e.Rule.TargetFormId;
            if (!this.PlugIns.Any(p => p.Item1.EqualsIgnoreCase(formId)) && this.Registration.Any(reg => reg.Key.EqualsIgnoreCase(formId)))
            {
                var className = this.Registration[formId];
                this.RegisterPlugIn(new Tuple<string, IBillBenchPlugIn>(formId, TypesContainer.CreateInstance<IBillBenchPlugIn>(className)));
            }//end if

            var billView = this.View.AsType<IBillView>();
            foreach (var plugin in this.PlugIns)
            {
                plugin.Item2.SetContext(billView);
                if (action != null) action.Invoke(plugin.Item2);
            }
        }


        /// <summary>
        /// 触发创建下推参数后事件。
        /// </summary>
        /// <param name="e">事件参数。</param>
        public void FireAfterCreatePushArgs(AfterCreatePushArgsEventArgs e)
        {
            this.InvokeMethod(e, plugin => plugin.AfterCreatePushArgs(e));
        }

        /// <summary>
        /// 触发创建目标单据后事件。
        /// </summary>
        /// <param name="e">事件参数。</param>
        public void FireAfterCreateTargetData(AfterCreateTargetDataEventArgs e)
        {
            this.InvokeMethod(e, plugin => plugin.AfterCreateTargetData(e));
        }//end method

        /// <summary>
        /// 触发上传目标单据前事件。
        /// </summary>
        /// <param name="e">事件参数。</param>
        public void FireBeforeUploadTargetData(BeforeUploadTargetDataEventArgs e)
        {
            this.InvokeMethod(e, plugin => plugin.BeforeUploadTargetData(e));
        }//end method
    }
}
