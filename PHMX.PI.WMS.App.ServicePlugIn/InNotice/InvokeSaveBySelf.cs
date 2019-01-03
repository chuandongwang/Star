using Kingdee.BOS.App;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.ServiceHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Kingdee.BOS.Util;
using Kingdee.BOS.Core.Interaction;

namespace PHMX.PI.WMS.App.ServicePlugIn.InNotice
{
    [Description("收货通知，触发自我保存操作，操作插件。")]
    public class InvokeSaveBySelf : AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            var metadata = FormMetaDataCache.GetCachedFormMetaData(this.Context, this.BusinessInfo.GetForm().Id);
            var businessInfo = metadata.BusinessInfo;
            foreach (var field in businessInfo.GetFieldList())
            {
                e.FieldKeys.Add(field.Key);
            }
        }

        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            if (!e.DataEntitys.Any()) return;

            var metadata = FormMetaDataCache.GetCachedFormMetaData(this.Context, this.BusinessInfo.GetForm().Id);
            var businessInfo = metadata.BusinessInfo;
            var saveService = ServiceHelper.GetService<ISaveService>();

            var option = this.Option.Copy();
            option.SetIgnoreWarning(true);
            option.SetIgnoreInteractionFlag(true);
            saveService.Save(this.Context, businessInfo, e.DataEntitys, option)
                       .ThrowWhenUnSuccess(op => op.GetResultMessage());
        }
    }
}
