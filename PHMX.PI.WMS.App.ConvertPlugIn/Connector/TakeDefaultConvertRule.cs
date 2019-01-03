using BAH.PI.WMS.Core.Const;
using Kingdee.BOS.App;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace PHMX.PI.WMS.App.ConvertPlugIn.Connector
{
    [Description("ERP和WMS连接器，获取来源对象和目标对象之间的默认转换规则，单据转换插件。")]
    public class TakeDefaultConvertRule : AbstractConvertPlugIn
    {
        public override void OnAfterFieldMapping(AfterFieldMappingEventArgs e)
        {
            base.OnAfterFieldMapping(e);

            //检查下游单据是收货通知或发货通知。
            if (!e.TargetBusinessInfo.GetForm().Id.EqualsIgnoreCase(PIWMSFormPrimaryKey.Instance.InNotice()) &&
                !e.TargetBusinessInfo.GetForm().Id.EqualsIgnoreCase(PIWMSFormPrimaryKey.Instance.OutNotice()))
            {
                return;
            }//end if

            var dataEntities = e.TargetExtendDataEntitySet.FindByEntityKey("FBillHead");
            var sourceField = e.TargetBusinessInfo.GetField("FPHMXSourceFormId").AsType<BaseDataField>();
            var targetField = e.TargetBusinessInfo.GetField("FPHMXTargetFormId").AsType<BaseDataField>();
            var ruleField = e.TargetBusinessInfo.GetField("FPHMXConvertRuleId").AsType<BaseDataField>();
            var convertService = ServiceHelper.GetService<IConvertService>();
            foreach (var data in dataEntities)
            {
                var sourceFormId = data.DataEntity.FieldProperty<DynamicObject>(sourceField).PkId<string>();
                var targetFormId = data.DataEntity.FieldProperty<DynamicObject>(targetField).PkId<string>();
                if (sourceFormId.IsNullOrEmptyOrWhiteSpace() || targetFormId.IsNullOrEmptyOrWhiteSpace()) continue;

                var convertRuleId = data.DataEntity.FieldProperty<DynamicObject>(ruleField).PkId<string>();
                if (!convertRuleId.IsNullOrEmptyOrWhiteSpace())
                {
                    if (convertService.GetConvertRule(this.Context, convertRuleId).Adaptive(metadata => metadata != null && metadata.Rule.Status))
                    {
                        continue;
                    }
                }//end if

                //取启用状态的默认规则。
                var rule = convertService.GetConvertRules(this.Context, sourceFormId, targetFormId)
                                         .Where(item => item.Status)
                                         .Where(item => item.IsDefault)
                                         .FirstOrDefault();
                //如果取到则赋值。
                if (rule != null)
                {
                    ruleField.RefIDDynamicProperty.SetValue(data.DataEntity, rule.Id);
                }//end if
            }//end foreach

            //将规则字段数据包补充完整。
            var viewService = ServiceHelper.GetService<IViewService>();
            dataEntities.Select(data => data.DataEntity)
                        .ToArray()
                        .Mend(ruleField, ids => viewService.LoadFromCache(this.Context, ids, ruleField.RefFormDynamicObjectType));

        }//end method

    }//end class
}//end namespace
