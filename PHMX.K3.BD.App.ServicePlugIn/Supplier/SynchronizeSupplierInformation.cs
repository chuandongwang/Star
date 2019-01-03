using BAH.BOS.Core.Const.BillStatus;
using BAH.PI.BD.Core.Const;
using Kingdee.BOS;
using Kingdee.BOS.App;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace PHMX.K3.BD.App.ServicePlugIn.Supplier
{
    [Description("从ERP同步供应商数据至WMS")]
    //同步字段：名称+编码
    public class SynchronizeSupplierInformation : AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            e.FieldKeys.Add("FNumber");
            e.FieldKeys.Add("FName");
            e.FieldKeys.Add("FCreateOrgId");
            e.FieldKeys.Add("FUseOrgId");
            base.OnPreparePropertys(e);

        }
        public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
        {

            //（列表中批量审核场景下）取出所有数据包
            var queryService = ServiceHelper.GetService<IQueryService>();
            var viewService = ServiceHelper.GetService<IViewService>();
            var dataEntites = e.SelectedRows
                               .Select(data => data.DataEntity)
                               .Where(data => data.PkId<int>() == data.MasterId<int>())
                               .ToArray();
            if (!dataEntites.Any()) return;

            //取出所有的MasterId
            var masterIds = dataEntites.Select(data => data.PkId<int>()).ToArray();
            string targetFormId = PIBDFormPrimaryKey.Instance.Supplier();

            //找到对应普华物料的主键
            QueryBuilderParemeter para = new QueryBuilderParemeter();
            para.FormId = targetFormId;
            para.SelectItems = SelectorItemInfo.CreateItems("FID", "FMIRRORID");
            para.FilterClauseWihtKey = "FMirrorId in (Select FID From TABLE(fn_StrSplit(@MasterIds,',',1)))";
            para.SqlParams.Add(new SqlParam("@MasterIds", KDDbType.udt_inttable, masterIds));
            var ids = queryService.GetDynamicObjectCollection(this.Context, para).Select(data => data.Property<object>("FID")).ToArray();
            var mirrorids = queryService.GetDynamicObjectCollection(this.Context, para).Select(data => data.Property<int>("FMIRRORID")).ToArray();

            //用得到的主键去获取普华物料数据包
            var targetMetadata = FormMetaDataCache.GetCachedFormMetaData(this.Context, targetFormId);
            var targetBussinessInfo = targetMetadata.CreateBillView(this.Context);
            var targetBillView = targetMetadata.CreateBillView(this.Context);
            var targetBillService = targetBillView.AsDynamicFormViewService();
            var targetDataObjects = viewService.LoadFromCache(this.Context, ids, targetMetadata.BusinessInfo.GetDynamicObjectType()).ToList();


            //如果两边的数据包可以被关联，那么直接对应修改。
            dataEntites.Join(targetDataObjects,
                                left => left.MasterId<int>(),
                                right => right.FieldProperty<DynamicObject>(targetMetadata.BusinessInfo.GetField("FMirrorId")).MasterId<int>(),
                                (left, right) =>
                                {
                                    targetBillView.Edit(right);
                                    targetBillService.UpdateValue("FNumber", -1, left.BDNumber());
                                    targetBillService.UpdateValue("FName", -1, new LocaleValue(left.BDName(this.Context)));
                                    return right;
                                }).ToArray();

            //如果数据包没有关联，则新增
            var unmatchDataEntities = dataEntites.Where(data => (mirrorids.Contains(data.MasterId<int>()) == false))
                                              .ToArray();
            foreach (var data in unmatchDataEntities)
            {
                targetBillView.AddNew();
                targetBillService.UpdateValue("FNumber", -1, data.BDNumber());
                targetBillService.UpdateValue("FName", -1, new LocaleValue(data.BDName(this.Context)));
                targetBillService.SetItemValueByID("FMirrorId", data.PkId<int>(), -1);
                targetDataObjects.Add(targetBillView.Model.DataObject);
            }
            //合并待操作数据
            if (targetDataObjects.Any())
            {
                targetDataObjects.DoNothing(this.Context, targetMetadata.BusinessInfo, "Upload").Adaptive(op => this.OperationResult.MergeResult(op));
            }

            base.AfterExecuteOperationTransaction(e);
        }

    }
}
