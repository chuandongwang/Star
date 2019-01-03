using BAH.BOS.Core.Const.BillStatus;
using PHMX.PI.WMS.Contracts;
using PHMX.PI.WMS.Core.Connector;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace PHMX.PI.WMS.App.ServicePlugIn.Outbound
{
    [Description("发货明细，生成目标单据，操作插件。")]
    public class GenTarget : AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("FPHMXGenTargetStatus");
        }

        public override void BeginOperationTransaction(BeginOperationTransactionArgs e)
        {
            base.BeginOperationTransaction(e);
            var billIds = e.DataEntitys.Select(data => data.PkId()).ToArray();
            var connectorService = PHMSWMSServiceFactory.Instance.GetLocalService<IConnectorService>();
            var args = connectorService.GetGenTargetSource(this.Context, GenTargetSourceSqlFromOutbound.Instance, this.BusinessInfo.GetForm().Id, billIds);
            if (!args.Any())
            {
                throw new KDBusinessException(string.Empty, "未获取到生成目标单据的数据源！");
            }//end if

            var op = connectorService.Push(this.Context, args);
            this.OperationResult.MergeResult(op);

            //标记生成状态。
            var dataEntities = args.Join(e.DataEntitys,
                                          left => left.BillId.ToChangeType<long>(),
                                          right => right.PkId<long>(),
                                          (left, right) => right).ToArray();
            connectorService.MarkGenStatus(this.Context, this.BusinessInfo, dataEntities, LogicStatus.Instance.Yes());
        }//end method

    }//end class
}//end namespace
