using PHMX.PI.WMS.App.ServicePlugIn.InNotice;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace PHMX.K3.SCM.STK.App.ServicePlugIn.TransferApply
{
    [Description("调拨申请单，审核后(事务外)自动下推收货通知，操作插件。")]
    public class InvokeAutoPushToInNotice : InvokeAutoPushToNotice
    {
        public override void OnPrepareOperationServiceOption(OnPrepareOperationServiceEventArgs e)
        {
            base.OnPrepareOperationServiceOption(e);
            this.AutoPushFieldKey = "FPHMXAutoPushToInNotice";
        }
    }
}
