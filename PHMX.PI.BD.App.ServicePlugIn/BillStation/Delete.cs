using BAH.BOS.Core.Const.BillStatus;
using BAH.PI.WMS.Core.Const;
using PHMX.PI.WMS.Contracts;
using PHMX.PI.WMS.Core.Connector;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace PHMX.PI.BD.App.ServicePlugIn.BillStation
{
    [Description("单据中转站，删除触发回撤目标单据，操作插件。")]
    public class Delete : AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("FGenTargetStatus");
            e.FieldKeys.Add("FSourceObjectTypeId");
            e.FieldKeys.Add("FSourceBillNo");
            e.FieldKeys.Add("FSourceBillId");
        }

        public override void BeginOperationTransaction(BeginOperationTransactionArgs e)
        {
            base.BeginOperationTransaction(e);
            var dataEntities = e.DataEntitys.Where(data => data.FieldProperty<string>(this.BusinessInfo.GetField("FGenTargetStatus")).EqualsIgnoreCase(LogicStatus.Instance.Yes())).ToArray();
            if (!dataEntities.Any()) return;

            var connectorService = PHMSWMSServiceFactory.Instance.GetLocalService<IConnectorService>();
            var args = new List<GenTargetArgs>();
            //整理发货明细数据。
            foreach (var data in dataEntities)
            {
                var entries = data.EntryProperty(this.BusinessInfo.GetEntity("FEntity"));
                entries.Where(entry => entry.FieldProperty<string>(this.BusinessInfo.GetField("FSourceObjectTypeId")).EqualsIgnoreCase(PIWMSFormPrimaryKey.Instance.Outbound()))
                       .Select(entry => entry.FieldProperty<string>(this.BusinessInfo.GetField("FSourceBillId")).ToChangeType<long>())
                       .Cast<object>()
                       .ToArray()
                       .Adaptive(billIds =>
                       {
                           var sources = connectorService.GetGenTargetSource(this.Context, GenTargetSourceSqlFromOutboundGroupByOutNotice.Instance, this.BusinessInfo.GetForm().Id, billIds);
                           sources.ForEach(item =>
                           {
                               item.BillId = data.PkId<long>();
                               item.BillNo = data.BillNo();
                           });
                           args.AddRange(sources);
                       });
            }//end foreach
            //整理收货明细数据。
            foreach (var data in dataEntities)
            {
                var entries = data.EntryProperty(this.BusinessInfo.GetEntity("FEntity"));
                entries.Where(entry => entry.FieldProperty<string>(this.BusinessInfo.GetField("FSourceObjectTypeId")).EqualsIgnoreCase(PIWMSFormPrimaryKey.Instance.Inbound()))
                       .Select(entry => entry.FieldProperty<string>(this.BusinessInfo.GetField("FSourceBillId")).ToChangeType<long>())
                       .Cast<object>()
                       .ToArray()
                       .Adaptive(billIds =>
                       {
                           var sources = connectorService.GetGenTargetSource(this.Context, GenTargetSourceSqlFromInBoundGroupByInNotice.Instance, this.BusinessInfo.GetForm().Id, billIds);
                           sources.ForEach(item =>
                           {
                               item.BillId = data.PkId<long>();
                               item.BillNo = data.BillNo();
                           });
                           args.AddRange(sources);
                       });
            }//end foreach
            if (!args.Any()) throw new KDBusinessException(string.Empty, "未获取到回撤目标单据的数据源！");

            var op = connectorService.Pull(this.Context, args.ToArray());
            this.OperationResult.MergeResult(op);
            if (!this.OperationResult.IsSuccess)
            {
                this.OperationResult.RemoveSuccessResult();
                this.OperationResult.ThrowWhenUnSuccess(false);
            }//end if
        }

    }//end class
}//end namespace
