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

namespace PHMX.PI.BD.App.ServicePlugIn.Material
{
    [Description("普华智造物料，与ERP物料绑定关系，操作插件。")]
    public class MaterialReverseBinding : AbstractOperationServicePlugIn
    {
        public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
        {
            var viewService = ServiceHelper.GetService<IViewService>();
            var saveSevice = ServiceHelper.GetService<ISaveService>();

            var mirrorField = this.BusinessInfo.GetField("FMirrorId").AsType<BaseDataField>();
            var mtlMetadata = FormMetaDataCache.GetCachedFormMetaData(this.Context, mirrorField.LookUpObject.FormId);
            var mtlBusinessInfo = mtlMetadata.BusinessInfo.GetSubBusinessInfo("FPHMXMtlId");
            var mtlField = mtlBusinessInfo.GetField("FPHMXMtlId").AsType<BaseDataField>();//ERP物料里的“仓网物料”字段定义。

            //获取ERP物料数据包并修改。
            var dataEntities = e.SelectedRows.Select(data => data.DataEntity)
                                .Where(data => data.FieldProperty<DynamicObject>(mirrorField) != null)
                                .Where(data => !data.PkId<string>().IsNullOrEmptyOrWhiteSpace())
                                .Where(data => !data.PkId<string>().EqualsIgnoreCase(data.FieldProperty<DynamicObject>(mirrorField).FieldRefProperty<DynamicObject>(mirrorField, "FPHMXMtlId").PkId<string>()))
                                .Select(data =>
                                {
                                    var mtl = data.FieldProperty<DynamicObject>(mirrorField);
                                    mtlField.RefIDDynamicProperty.SetValue(mtl, data.PkId<string>());//先把对应的主键赋值过去。
                                    return mtl;
                                }).ToArray();
            if (!dataEntities.Any()) return;

            //获取分配的物料数据包。
            var masterIds = dataEntities.Select(data => data.MasterId())
                                        .Distinct().ToList();
            if (!masterIds.Any()) masterIds.Add(0);
            var para = new QueryBuilderParemeter();
            para.FormId = mirrorField.LookUpObject.FormId;
            para.FilterClauseWihtKey = "FMasterId in (Select FID From TABLE(fn_StrSplit(@MasterIds,',',1))) and FMaterialId <> FMasterId";
            para.SqlParams.Add(new SqlParam("@MasterIds", KDDbType.udt_inttable, masterIds));
            var allocateEntities = viewService.LoadFromCache(this.Context, mirrorField.RefFormDynamicObjectType, para);

            //为分配的数据包关联赋值。
            dataEntities.Join(allocateEntities,
                              left => left.MasterId<int>(),
                              right => right.MasterId<int>(),
                              (left, right) =>
                              {
                                  mtlField.RefIDDynamicProperty.SetValue(right, left.FieldRefIdProperty<string>(mtlField));
                                  return right;
                              }).ToArray();

            //合并两个数据包，再补充里面字段数据包并无事务保存。
            var merge = dataEntities.Concat(allocateEntities).ToArray();
            if (merge.Any())
            {
                //merge.Mend(mtlField, ids => viewService.LoadFromCache(this.Context, ids, mtlField.RefFormDynamicObjectType));
                saveSevice.Save(this.Context, merge);
            }

            base.AfterExecuteOperationTransaction(e);
        }//end method

    }//end class
}//end namespace
