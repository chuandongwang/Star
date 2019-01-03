using Kingdee.BOS;
using Kingdee.BOS.App;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace PHMX.PI.BD.App.ServicePlugIn.Stock
{
    [Description("普华智造仓库，与ERP仓库绑定关系，操作插件。")]
    public class StockReverseBinding : AbstractOperationServicePlugIn
    {
        public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
        {

            var viewService = ServiceHelper.GetService<IViewService>();
            var saveService = ServiceHelper.GetService<ISaveService>();

            var mirrorField = this.BusinessInfo.GetField("FMirrorId").AsType<BaseDataField>();
            var stockMetadata = FormMetaDataCache.GetCachedFormMetaData(this.Context, mirrorField.LookUpObject.FormId);
            var stockBusinessInfo = stockMetadata.BusinessInfo.GetSubBusinessInfo("FPHMXContactId");
            var stockField = stockBusinessInfo.GetField("FPHMXContactId").AsType<BaseDataField>();

            //获取ERP物料数据包。
            var dataEntities = e.SelectedRows.Select(data => data.DataEntity)
                                .Where(data => data.FieldProperty<DynamicObject>(mirrorField) != null)
                                .Where(data => !data.PkId<string>().IsNullOrEmptyOrWhiteSpace())
                                .Where(data => !data.PkId<string>().EqualsIgnoreCase(data.FieldProperty<DynamicObject>(mirrorField).FieldRefProperty<DynamicObject>(mirrorField, "FPHMXContactId").PkId<string>()))
                                .Select(data =>
                                {
                                    var stock = data.FieldProperty<DynamicObject>(mirrorField);
                                    stockField.RefIDDynamicProperty.SetValue(stock, data.PkId<string>());//先把对应的主键赋值过去。
                                    return stock;
                                }).ToArray();
            if (!dataEntities.Any()) return;

            //获取分配的数据包
            var masterIds = dataEntities.Select(data => data.MasterId()).Distinct().ToList();

            if (!masterIds.Any()) masterIds.Add(0);
            var para = new QueryBuilderParemeter();
            para.FormId = mirrorField.LookUpObject.FormId;
            para.FilterClauseWihtKey = "FMasterId in (Select FID From TABLE(fn_StrSplit(@MasterIds,',',1))) and FSTOCKID <> FMasterId";
            para.SqlParams.Add(new SqlParam("@MasterIds", KDDbType.udt_inttable, masterIds));
            var allocateEntities = viewService.LoadFromCache(this.Context, mirrorField.RefFormDynamicObjectType, para);

            //为分配的数据包关联赋值。
            dataEntities.Join(allocateEntities,
                              left => left.MasterId<int>(),
                              right => right.MasterId<int>(),
                              (left, right) =>
                              {
                                  stockField.RefIDDynamicProperty.SetValue(right, left.FieldRefIdProperty<string>(stockField));
                                  return right;
                              }).ToArray();

            //合并两个数据包，再补充里面字段数据包并无事务保存。
            var merge = dataEntities.Concat(allocateEntities).ToArray();
            if (merge.Any())
            {
                saveService.Save(this.Context, merge);
            }


            base.AfterExecuteOperationTransaction(e);
        }//end method

    }//end class
}//end namespace
