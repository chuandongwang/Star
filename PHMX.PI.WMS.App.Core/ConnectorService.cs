using BAH.BOS.Core.Const.FormOperation;
using PHMX.PI.WMS.Contracts;
using PHMX.PI.WMS.Core.Connector;
using PHMX.PI.WMS.Core.Connector.PlugIn;
using PHMX.PI.WMS.Core.Connector.PlugIn.Args;
using Kingdee.BOS;
using Kingdee.BOS.App;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.Const;
using Kingdee.BOS.Core.DependencyRules;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Interaction;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.ConvertElement;
using Kingdee.BOS.Core.Metadata.ConvertElement.ServiceArgs;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PHMX.PI.WMS.App.Core
{
    /// <summary>
    /// 实现接口：IConnectorService。
    /// </summary>
    public class ConnectorService : IConnectorService
    {
        /// <summary>
        /// 从收货明细获取生成目标单据的数据源。
        /// </summary>
        /// <param name="ctx">上下文对象。</param>
        /// <param name="sql">SQL语句。</param>
        /// <param name="objectTypeId">单据类型。</param>
        /// <param name="billIds">单据主键。</param>
        /// <returns>返回查询的结果集。</returns>
        public GenTargetArgs[] GetGenTargetSource(Context ctx, GenTargetSourceSql sql, string objectTypeId, object[] billIds)
        {
            if (!billIds.Any()) return new GenTargetArgs[0];

            var dbService = ServiceHelper.GetService<IDBService>();
            var sqlParams = new List<SqlParam>();
            sqlParams.Add(new SqlParam(sql.BillIdsParamName, KDDbType.udt_inttable, billIds.Distinct().ToArray()));
            var collection = dbService.ExecuteDynamicObject(ctx: ctx, strSQL: sql.SqlString, paramList: sqlParams.ToArray());

            var result = collection.GroupBy(g => new
            {
                ObjectTypeId = objectTypeId,
                BillId = g.Property<long>("FBILLID"),
                BillNo = g.Property<string>("FBILLNO"),
                SourceFormId = g.Property<string>("FSOURCEFORMID"),
                TargetFormId = g.Property<string>("FTARGETFORMID"),
                ConvertRuleId = g.Property<string>("FCONVERTRULEID"),
                Date = g.Property("FDATE", DateTime.Now).Date
            }).Select(g =>
            {
                var args = new GenTargetArgs();
                args.ObjectTypeId = g.Key.ObjectTypeId;
                args.BillId = g.Key.BillId;
                args.BillNo = g.Key.BillNo;
                args.SourceFormId = g.Key.SourceFormId;
                args.TargetFormId = g.Key.TargetFormId;
                args.ConvertRuleId = g.Key.ConvertRuleId;
                args.Date = g.Key.Date;

                g.Select(b =>
                {
                    DataLinkSource row = new DataLinkSource(args);
                    row.SId = b.Property<long>("FSID");
                    row.SBillId = b.Property<long>("FSBILLID");
                    row.NoticeFormId = b.Property<string>("FNOTICEFORMID");
                    row.Qty = b.Property<long>("FQTY");
                    row.PHMXWgt = b.Property<long>("FPHMXWgt");
                    row.Cty = b.Property<long>("FCTY");
                    row.LotNo = b.Property<string>("FLOTNO");
                    row.ProduceDate = b.Property<object>("FPRODUCEDATE").Adaptive(date => date == null || date.Equals(default(DateTime)) ? default(DateTime?) : new DateTime?(date.ToType<DateTime>()));
                    row.ExpiryDate = b.Property<object>("FEXPIRYDATE").Adaptive(date => date == null || date.Equals(default(DateTime)) ? default(DateTime?) : new DateTime?(date.ToType<DateTime>()));
                    return row;
                }).ToList().Adaptive(lst => args.DataRows.AddRange(lst));

                return args;
            }).ToArray();

            return result;
        }//end method

        /// <summary>
        /// 执行下推生成目标单据。
        /// </summary>
        /// <param name="ctx">上下文对象。</param>
        /// <param name="args">生成目标单据参数。</param>
        /// <returns>返回操作结果。</returns>
        public IOperationResult Push(Context ctx, GenTargetArgs[] genArgs)
        {
            var operationResult = new OperationResult();
            if (!genArgs.Any()) return operationResult;

            var convertService = ServiceHelper.GetService<IConvertService>();
            var doNothingService = ServiceHelper.GetService<IDoNothingService>();
            var uploadOption = default(OperateOption);

            var proxy = new BillBenchPlugInProxy();
            foreach (var arg in genArgs)
            {
                //初始化单据工作台代理。
                var metadata = FormMetaDataCache.GetCachedFormMetaData(ctx, arg.TargetFormId);
                var billView = metadata.CreateBillView(ctx);
                var executeContext = new BOSActionExecuteContext(billView);
                proxy.Initialize(ctx, billView);

                var rule = convertService.GetConvertRule(ctx, arg.ConvertRuleId, true).Adaptive(item => item == null ? default(ConvertRuleElement) : item.Rule);
                if (rule == null) throw new KDBusinessException(string.Empty, "未配置有效的单据转换规则！");

                var sourceEntryKey = rule.Policies.Where(p => p is DefaultConvertPolicyElement)
                                                  .Select(p => p.ToType<DefaultConvertPolicyElement>())
                                                  .FirstOrDefault().SourceEntryKey;
                var selectedRows = arg.DataRows.Select(row => new ListSelectedRow(row.SBillId.ToString(), row.SId.ToString(), 0, arg.SourceFormId) { EntryEntityKey = sourceEntryKey }).ToArray();
                PushArgs pushArgs = new PushArgs(rule, selectedRows);
                proxy.FireAfterCreatePushArgs(new AfterCreatePushArgsEventArgs(rule, pushArgs));
                var pushResult = convertService.Push(ctx, pushArgs);
                operationResult.MergeResult(pushResult);

                //整理下推生成的数据包。
                var dataEntities = pushResult.TargetDataEntities.Select(data => data.DataEntity).ToArray();

                //逐行赋值。
                foreach (var data in dataEntities)
                {
                    billView.Edit(data);
                    billView.RuleContainer.Suspend();
                    billView.Model.SetValue("FDate", arg.Date);
                    billView.Model.SetItemValueByID("FPHMXWMSFormId", arg.ObjectTypeId, -1);
                    billView.Model.SetValue("FPHMXWMSBillId", arg.BillId.ToString());
                    billView.Model.SetValue("FPHMXWMSBillNo", arg.BillNo);
                    proxy.FireAfterCreateTargetData(new AfterCreateTargetDataEventArgs(rule, operationResult, arg.DataRows));
                    billView.RuleContainer.Resume(executeContext);
                }//end foreach

                //调用上传操作完成暂存、保存、提交、审核一系列操作。
                var uploadOperation = metadata.BusinessInfo.GetForm().FormOperations.Where(operation => operation.Operation.Contains("Upload")).FirstOrDefault();
                if (uploadOperation == null)
                {
                    throw new KDBusinessException(string.Empty, "未配置有效的上传操作！");
                }//end if

                uploadOption = OperateOption.Create();
                uploadOption.SetIgnoreWarning(true);
                uploadOption.SetIgnoreInteractionFlag(true);
                uploadOption.SetThrowWhenUnSuccess(false);
                proxy.FireBeforeUploadTargetData(new BeforeUploadTargetDataEventArgs(rule, dataEntities, arg, uploadOption));

                try
                {
                    var uploadResult = doNothingService.DoNothingWithDataEntity(ctx, metadata.BusinessInfo, dataEntities, uploadOperation.Operation, uploadOption);
                    operationResult.MergeResult(uploadResult);
                }
                catch
                {
                    var inner = uploadOption.GetVariableValue<IOperationResult>(BOSConst.CST_KEY_OperationResultKey);
                    if (inner != null)
                    {
                        operationResult.MergeResult(inner);
                    }
                    else
                    {
                        throw;
                    }

                }//end catch
            }//end foreach

            return operationResult;
        }//end method

        /// <summary>
        /// 执行上查删除目标单据。
        /// </summary>
        /// <param name="ctx">上下文对象。</param>
        /// <param name="genArgs">生成目标单据参数。</param>
        /// <param name="filter">过滤条件。</param>
        /// <param name="sqlParam">SQL参数。</param>
        /// <returns>返回操作结果。</returns>
        public IOperationResult Pull(Context ctx, GenTargetArgs[] genArgs)
        {
            var operationResult = new OperationResult();
            if (!genArgs.Any()) return operationResult;

            var viewService = ServiceHelper.GetService<IViewService>();
            var saveService = ServiceHelper.GetService<ISaveService>();
            var doNothingService = ServiceHelper.GetService<IDoNothingService>();
            var unloadOption = default(OperateOption);

            var group = genArgs.GroupBy(g => new { g.ObjectTypeId, g.TargetFormId }).Select(g => new
            {
                Key = g.Key,
                BillIds = g.Select(b => b.BillId).Distinct().ToArray()
            }).ToArray();
           

            foreach (var g in group)
            {
                var metadata = FormMetaDataCache.GetCachedFormMetaData(ctx, g.Key.TargetFormId);
                var businessInfo = metadata.BusinessInfo;
                var dataEntities = viewService.Load(ctx, businessInfo.GetDynamicObjectType(), new QueryBuilderParemeter().Adaptive(para =>
                {
                    para.FormId = g.Key.TargetFormId;
                    para.FilterClauseWihtKey = "FPHMXWMSFormId <> '' AND FPHMXWMSFormId = @FormId AND FPHMXWMSBillId IN (SELECT FID FROM TABLE(fn_StrSplit(@BillIds,',',1)))";
                    para.SqlParams.Add(new SqlParam("@FormId", KDDbType.String, g.Key.ObjectTypeId));
                    para.SqlParams.Add(new SqlParam("@BillIds", KDDbType.udt_inttable, g.BillIds));
                }));
                if (!dataEntities.Any()) continue;

                //在回撤之前，把仓网单据类型设为空，
                //如果不设置，单据无法删除。
                foreach(var data in dataEntities)
                {
                    businessInfo.GetField("FPHMXWMSFormId").AsType<BaseDataField>().Adaptive(field =>
                    {
                        field.RefIDDynamicProperty.SetValue(data, string.Empty);
                        field.DynamicProperty.SetValue(data, null);
                    });
                }//end foreach
                saveService.Save(ctx, dataEntities);

                //调用回撤操作完成反审核、撤销、删除一系列操作。
                var uploadOperation = metadata.BusinessInfo.GetForm().FormOperations.Where(operation => operation.Operation.Contains("Unload")).FirstOrDefault();
                if (uploadOperation == null)
                {
                    throw new KDBusinessException(string.Empty, "未配置有效的回撤操作！");
                }//end if

                unloadOption = OperateOption.Create();
                unloadOption.SetIgnoreWarning(true);
                unloadOption.SetIgnoreInteractionFlag(true);
                unloadOption.SetThrowWhenUnSuccess(false);
                try
                {
                    var unloadResult = doNothingService.DoNothingWithDataEntity(ctx, metadata.BusinessInfo, dataEntities, uploadOperation.Operation, unloadOption);
                    operationResult.MergeResult(unloadResult);
                }
                catch
                {
                    var inner = unloadOption.GetVariableValue<IOperationResult>(BOSConst.CST_KEY_OperationResultKey);
                    if (inner != null)
                    {
                        operationResult.MergeResult(inner);
                    }
                    else
                    {
                        throw;
                    }
                }

            }//end foreach

            return operationResult;
        }//end method

        /// <summary>
        /// 标记状态。
        /// </summary>
        /// <param name="ctx">上下文对象。</param>]
        /// <param name="businessInfo">业务对象。</param>
        /// <param name="dataEntities">数据包。</param>
        /// <param name="statusValue">状态值。</param>
        /// <param name="fieldKey">状态字段。</param>
        public void MarkGenStatus(Context ctx, BusinessInfo businessInfo, DynamicObject[] dataEntities, string statusValue, string fieldKey = "FPHMXGenTargetStatus")
        {
            //标记生成状态。
            var statusField = businessInfo.GetField(fieldKey);
            foreach(var data in dataEntities)
            {
                statusField.DynamicProperty.SetValue(data, statusValue);
            }
            if (dataEntities.Any())
            {
                var saveService = ServiceHelper.GetService<ISaveService>();
                saveService.Save(ctx, dataEntities);
            }//end if
        }

    }//end class
}//end namespace
