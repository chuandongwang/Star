using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Core.SqlBuilder;
using System;
using System.ComponentModel;
using System.Linq;

namespace PHMX.PI.WMS.App.ServicePlugIn.Outbound
{
    [Description("检查发货明细上游的发货通知有没有生成目标单据，若已生成，则不允许反审核。")]
    public class CheckOutNotice : AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("FOriginFormId");
            e.FieldKeys.Add("FOriginBillNo");
        }

        public override void BeforeExecuteOperationTransaction(BeforeExecuteOperationTransaction e)
        {

            base.BeforeExecuteOperationTransaction(e);

            if (e.SelectedRows.Count() == 0) return;
            var dataEntities = e.SelectedRows.Select(data => data.DataEntity).ToArray();
            foreach (DynamicObject dataEntry in dataEntities)
            {
                DynamicObject BillEntry = dataEntry["BillEntry"].AsType<DynamicObjectCollection>().First();
                //获取发货通知数据
                string OrginBillNo = BillEntry["OriginBillNo"].ToString();
                string OriginFormId = "BAH_WMS_OutNotice";

                FormMetadata meta = MetaDataServiceHelper.Load(this.Context, OriginFormId) as FormMetadata;
                QueryBuilderParemeter queryParam = new QueryBuilderParemeter();
                queryParam.FormId = OriginFormId;
                queryParam.BusinessInfo = meta.BusinessInfo;

                queryParam.FilterClauseWihtKey = string.Format(" {0} = '{1}' ", meta.BusinessInfo.GetBillNoField().Key, OrginBillNo);

                var objs = BusinessDataServiceHelper.Load(this.Context, meta.BusinessInfo.GetDynamicObjectType(), queryParam);

                if (objs[0]["PHMXGenTargetStatus"].ToString().Equals("B") == true)
                {
                    e.Cancel = true;
                    e.CancelMessage = string.Format("编号为{0}的发货通知已生成目标单据，不允许反审核！", OrginBillNo);

                    //throw new Exception(string.Format("编号为{0}的发货通知已生成目标单据，不允许反审核！", OrginBillNo));
                }
            }
        }
    }
}
