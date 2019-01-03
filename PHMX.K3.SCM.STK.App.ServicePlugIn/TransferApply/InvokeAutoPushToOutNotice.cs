using PHMX.PI.WMS.App.ServicePlugIn.OutNotice;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace PHMX.K3.SCM.STK.App.ServicePlugIn.TransferApply
{
    [Description("调拨申请单，审核后(事务外)自动下推发货通知，操作插件。")]
    public class InvokeAutoPushToOutNotice : InvokeAutoPushToNotice
    {
        public override void OnPrepareOperationServiceOption(OnPrepareOperationServiceEventArgs e)
        {
            base.OnPrepareOperationServiceOption(e);
            this.AutoPushFieldKey = "FPHMXAutoPushToOutNotice";
        }
    }
}
