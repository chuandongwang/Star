using BAH.PI.WMS.Core.Const;
using Kingdee.BOS;
using Kingdee.BOS.App;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata.BusinessService;
using Kingdee.BOS.Core.Metadata.ConvertElement;
using Kingdee.BOS.Core.Metadata.ConvertElement.ServiceArgs;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using System;
using System.ComponentModel;
using System.Linq;

namespace PHMX.PI.WMS.App.ServicePlugIn.InNotice
{
    [Description("收货通知，源单审核后(事务外)自动下推，操作插件。")]
    public class InvokeAutoPushToNotice: AbstractOperationServicePlugIn
    {
        public string AutoPushFieldKey { get; set; }

        public override void OnPrepareOperationServiceOption(OnPrepareOperationServiceEventArgs e)
        {
            base.OnPrepareOperationServiceOption(e);
            this.AutoPushFieldKey = "FPHMXAutoPushToNotice";
        }

        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add(this.AutoPushFieldKey);
        }

        public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
        {
            base.AfterExecuteOperationTransaction(e);
            if (!e.DataEntitys.Any()) return;
            var metas = this.FormOperation.AppBusinessService.Where(item => item.IsEnabled)
                                                             .Where(item => !item.IsForbidden)
                                                             .Where(item => item is AutoPushBusinessServiceMeta)
                                                             .Cast<AutoPushBusinessServiceMeta>()
                                                             .Where(item => item.RealSourceFormId.EqualsIgnoreCase(this.BusinessInfo.GetForm().Id))
                                                             .Where(item => item.TargetFormId.EqualsIgnoreCase(PIWMSFormPrimaryKey.Instance.InNotice()))
                                                             .ToArray();
            if (!metas.Any()) return;
            var convertService = ServiceHelper.GetService<IConvertService>();
            var doNothingService = ServiceHelper.GetService<IDoNothingService>();
            foreach (var meta in metas)
            {
                var rule = convertService.GetConvertRules(this.Context, meta.RealSourceFormId, meta.TargetFormId)
                                         .Where(item => item.Status)
                                         .Where(item => meta.ConvertRuleKey.IsNullOrEmptyOrWhiteSpace() ? item.IsDefault : meta.ConvertRuleKey.EqualsIgnoreCase(item.Key))
                                         .FirstOrDefault();
                if (rule == null) continue;

                var entryKey = rule.GetDefaultConvertPolicyElement().SourceEntryKey;
                var selectedRows = e.DataEntitys.SelectMany(data => data.EntryProperty(this.BusinessInfo.GetEntity(entryKey))
                                                                        .Where(entry => entry.FieldProperty<bool>(this.BusinessInfo.GetField(this.AutoPushFieldKey)))
                                                                        .Select(entry => new { EntryId = entry.PkId().ToChangeTypeOrDefault<string>(), BillId = data.PkId().ToChangeTypeOrDefault<string>() }))
                                                .Select(a => new ListSelectedRow(a.BillId, a.EntryId, 0, rule.SourceFormId).Adaptive(row => 
                                                {
                                                    row.EntryEntityKey = entryKey;
                                                    return row;
                                                })).ToArray();
                if (!selectedRows.Any()) continue;

                PushArgs pushArgs = new PushArgs(rule, selectedRows);
                var pushResult = convertService.Push(this.Context, pushArgs);
                this.OperationResult.MergeResult(pushResult);
                if (pushResult.IsSuccess)
                {
                    rule.TargetFormMetadata = FormMetaDataCache.GetCachedFormMetaData(this.Context, rule.TargetFormId);
                    var dataEntities = pushResult.TargetDataEntities.Select(data => data.DataEntity).ToArray();
                    var uploadResult = doNothingService.DoNothingWithDataEntity(this.Context, rule.TargetFormMetadata.BusinessInfo, dataEntities, "Upload");
                    this.OperationResult.MergeResult(uploadResult);
                }//end if
            }//end foreach
        }//end method

    }
}
