using Kingdee.BOS;
using Kingdee.BOS.App;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Log;
using Kingdee.BOS.KDThread;
using Kingdee.BOS.Log;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace PHMX.PI.WMS.App.ServicePlugIn.Outbound
{
    [Description("发货明细，在线程工厂里生成目标单据，操作插件。")]
    public class GenTargetInWorker : AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("FPHMXGenTargetStatus");
        }

        public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
        {
            base.AfterExecuteOperationTransaction(e);

            //仅支持从WebAPI提交数据后自动在线程中下推目标单据。
            if (this.Context.ClientInfo.ClientType != ClientType.WebApi) return;

            var dataEntities = e.SelectedRows.Select(row => row.DataEntity).ToArray();
            MainWorker.QuequeTask(() =>
            {
                var doNothingService = ServiceHelper.GetService<IDoNothingService>();
                var result = doNothingService.DoNothingWithDataEntity(this.Context, this.BusinessInfo, dataEntities, "PHMXGenTarget");
                result.RepairPKValue();
                var collection = result.OperateResult.GetFailResult();
                if (!collection.Any()) return;
                var logs = collection.Select(item => new LogObject
                {
                    SubSystemId = this.BusinessInfo.GetForm().SubsysId,
                    ObjectTypeId = this.BusinessInfo.GetForm().Id,
                    pkValue = item.PKValueIsNullOrEmpty ? item.PKValue.ToString() : string.Empty,
                    OperateName = this.FormOperation.OperationName.Value(this.Context),
                    Description = item.Message,
                    Environment = OperatingEnvironment.BizOperate
                }).ToList();
                if (logs.Any())
                {
                    var logService = ServiceHelper.GetService<ILogService>();
                    logService.BatchWriteLog(this.Context, logs);
                }//end if
            },
            callback =>
            {
                if (callback.Success)
                {
                    Logger.Error(this.GetType().FullName, callback.Message, callback.Exception);
                }//end if
            });

        }//end method

    }//end class
}//end namespace
