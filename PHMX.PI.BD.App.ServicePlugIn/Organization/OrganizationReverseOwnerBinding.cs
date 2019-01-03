using Kingdee.BOS.App;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;


namespace PHMX.PI.BD.App.ServicePlugIn.Organization
{
    [Description("普华智造组织，与ERP组织货主绑定关系，操作插件。")]
    public class OrganizationReverseOwnerBinding : AbstractOperationServicePlugIn
    {
        public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
        {
            var mirrorField = this.BusinessInfo.GetField("FMirrorId").AsType<BaseDataField>();
            var orgMetadata = FormMetaDataCache.GetCachedFormMetaData(this.Context, mirrorField.LookUpObject.FormId);
            var orgFieldKeys = mirrorField.GetRefPropertyKeys();
            if (orgFieldKeys.Any(key => !key.EqualsIgnoreCase("FPHMXOwnerId"))) orgFieldKeys.Add("FPHMXOwnerId");
            var orgBusinessInfo = orgMetadata.BusinessInfo.GetSubBusinessInfo(orgFieldKeys);
            var orgField = orgBusinessInfo.GetField("FPHMXOwnerId").AsType<BaseDataField>();

            //获取物料数据包。
            var dataEntities = e.SelectedRows.Select(data => data.DataEntity)
                                .Where(data => data.FieldProperty<DynamicObject>(mirrorField) != null)
                                .Where(data => !data.PkId<string>().IsNullOrEmptyOrWhiteSpace())
                                .Where(data => !data.PkId<string>().EqualsIgnoreCase(data.FieldProperty<DynamicObject>(mirrorField).FieldRefProperty<DynamicObject>(mirrorField, "FPHMXOwnerId").PkId<string>()))
                                .Select(data =>
                                {
                                    var org = data.FieldProperty<DynamicObject>(mirrorField);
                                    orgField.RefIDDynamicProperty.SetValue(org, data.PkId<string>());//先把对应的主键赋值过去。
                                    return org;
                                }).ToArray();

            //再补充里面字段数据包并无事务保存。
            if (dataEntities.Any())
            {
                var viewService = ServiceHelper.GetService<IViewService>();
                dataEntities.Mend(orgField, ids => viewService.LoadFromCache(this.Context, ids, this.BusinessInfo.GetSubBusinessInfo(orgField.GetRefPropertyKeys()).GetDynamicObjectType()));

                var saveSevice = ServiceHelper.GetService<ISaveService>();
                saveSevice.Save(this.Context, dataEntities);
            }

            base.AfterExecuteOperationTransaction(e);
        }//end method

    }//end class
}//end namespace
