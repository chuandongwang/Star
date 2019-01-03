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
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace PHMX.K3.BD.App.ServicePlugIn.Material
{
    [Description("从ERP同步物料数据至WMS")]
    //同步字段：名称+规格型号+编码
    public class SynchronizeMaterialInformation : AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("FNumber");
            e.FieldKeys.Add("FName");
            e.FieldKeys.Add("FSpecification");
            e.FieldKeys.Add("FCreateOrgId");
            e.FieldKeys.Add("FUseOrgId");
            e.FieldKeys.Add("FBaseUnitId");
            e.FieldKeys.Add("FPHMXPkgId");
            e.FieldKeys.Add("FPHMXStdCty");
            e.FieldKeys.Add("FIsBatchManage");
            e.FieldKeys.Add("FIsKFPeriod");
            e.FieldKeys.Add("FIsExpParToFlot");
            e.FieldKeys.Add("FExpPeriod");
            e.FieldKeys.Add("FExpUnit");
            e.FieldKeys.Add("FPHMXEnablePHManagement");
            e.FieldKeys.Add("SubHeadEntity");
        }
        public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
        {
            base.AfterExecuteOperationTransaction(e);

            //TODO：
            //0.泛亚场景，当Id和MasterId相同时，才触发同步事件。
            //1.准备物料的数据包，e.DataEntities，取物料MasterId。
            //2.用MasterId关联查询普华智造里物料MirrorId，通过这个步骤知道，哪些需要修改(调用Save方法)，哪些需要新增(调用Draft方法)。
            //3.需要修改的，直接关联同步修改。
            //4.需要新增的，通过表单代理直接创建新数据包。
            //5.统一调用上传操作。

            //（列表中批量审核场景下）取出所有数据包
            var queryService = ServiceHelper.GetService<IQueryService>();
            var viewService = ServiceHelper.GetService<IViewService>();
            var dataEntites = e.SelectedRows
                               .Select(data => data.DataEntity)
                               .Where(data => data.FieldProperty<bool>(this.BusinessInfo.GetField("FPHMXEnablePHManagement")))
                               .Where(data => data.PkId<int>() == data.MasterId<int>())
                               .ToArray();
            if (!dataEntites.Any()) return;

            //取出所有的MasterId
            var masterIds = dataEntites.Select(data => data.PkId<int>()).ToArray();
            string targetFormId = PIBDFormPrimaryKey.Instance.Material();

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
            var targetBusinessInfo = targetMetadata.BusinessInfo;
            var targetBillView = targetMetadata.CreateBillView(this.Context);
            var targetBillService = targetBillView.AsDynamicFormViewService();
            var targetDataObjects = viewService.LoadFromCache(this.Context, ids, targetBusinessInfo.GetDynamicObjectType()).ToList();

            //如果两边的数据包可以被关联，那么直接对应修改。
            dataEntites.Join(targetDataObjects,
                                left => left.MasterId<int>(),
                                right => right.FieldProperty<DynamicObject>(targetMetadata.BusinessInfo.GetField("FMirrorId")).MasterId<int>(),
                                (left, right) =>
                                {
                                    targetBillView.Edit(right);
                                    targetBillService.UpdateValue("FNumber", -1, left.BDNumber());
                                    targetBillService.UpdateValue("FName", -1, new LocaleValue(left.BDName(this.Context)));
                                    targetBillService.UpdateValue("FSpecification", -1, new LocaleValue(left.FieldProperty<LocaleValue>(this.BusinessInfo.GetField("FSpecification")).Value(this.Context)));

                                    //基本
                                    left.SubHeadProperty(this.BusinessInfo.GetEntity("SubHeadEntity"))
                                        .Adaptive(sub =>
                                        {
                                            this.BusinessInfo.GetField("FBaseUnitId").AsType<BaseDataField>()
                                                .Adaptive(field =>
                                                {
                                                    if (!sub.FieldProperty<DynamicObject>(field).FieldRefProperty<bool>(field, "FPHMXEA") ||
                                                        left.SubHeadProperty(this.BusinessInfo.GetEntity("SubHeadEntity1")).FieldProperty<decimal>(this.BusinessInfo.GetField("FPHMXStdCty")) > 0M)
                                                    {
                                                        targetBillService.UpdateValue("FEnableCapacity", -1, true);
                                                        targetBillService.UpdateValue("FCapacityUnit", -1, new LocaleValue(sub.FieldProperty<DynamicObject>(field).BDName(this.Context)));
                                                        targetBillService.UpdateValue("FCapacityScale", -1, sub.FieldProperty<DynamicObject>(field).FieldRefProperty<int>(field, "FPrecision"));
                                                    }//end if
                                                });
                                        });

                                    //库存
                                    var pkgId = left.SubHeadProperty(this.BusinessInfo.GetEntity("SubHeadEntity1")).FieldProperty<DynamicObject>(this.BusinessInfo.GetField("FPHMXPkgId")).PkId<string>();
                                    if (!pkgId.IsNullOrEmptyOrWhiteSpace())
                                    {
                                        var entries = targetBillView.Model.GetEntityDataObject(targetBusinessInfo.GetEntity("FPackageEntry"));
                                        if (entries.Any(entry => entry.FieldProperty<DynamicObject>(targetBusinessInfo.GetField("FEntryPackageId")).PkId<string>().EqualsIgnoreCase(pkgId)) == false)
                                        {
                                            targetBillView.Model.InsertEntryRow("FPackageEntry", 0);
                                            targetBillService.SetItemValueByID("FEntryPackageId", pkgId, 0);
                                        }//end if

                                        //设置标准容量。
                                        entries.Where(entry => targetBillView.Model.GetValue("FEnableCapacity").ToType<bool>() && entry.FieldProperty<DynamicObject>(targetBusinessInfo.GetField("FEntryPackageId")).PkId<string>().EqualsIgnoreCase(pkgId))
                                               .FirstOrDefault()
                                               .Adaptive(pkg =>
                                               {
                                                   if (pkg != null)
                                                   {
                                                       var index = entries.IndexOf(pkg);
                                                       targetBillService.UpdateValue("FStdCty", index, left.SubHeadProperty(this.BusinessInfo.GetEntity("SubHeadEntity1")).FieldProperty<decimal>(this.BusinessInfo.GetField("FPHMXStdCty")));
                                                   }//end if
                                               });

                                        //设置默认包装。
                                        targetBillService.SetItemValueByID("FPackageId", pkgId, -1);
                                    }//end if

                                    //设置有效期。
                                    if (targetBillView.Model.GetValue("FEnableExpiry").ToType<bool>())
                                    {
                                        targetBillService.UpdateValue("FExpPeriod", -1, left.SubHeadProperty(this.BusinessInfo.GetEntity("SubHeadEntity1")).FieldProperty<int>(this.BusinessInfo.GetField("FExpPeriod")));
                                        targetBillService.UpdateValue("FExpUnit", -1, left.SubHeadProperty(this.BusinessInfo.GetEntity("SubHeadEntity1")).FieldProperty<string>(this.BusinessInfo.GetField("FExpUnit")));
                                    }//end if
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
                targetBillService.UpdateValue("FSpecification", -1, new LocaleValue(data.FieldProperty<LocaleValue>(this.BusinessInfo.GetField("FSpecification")).Value(this.Context)));

                //基本
                data.SubHeadProperty(this.BusinessInfo.GetEntity("SubHeadEntity"))
                    .Adaptive(sub => 
                    {
                        this.BusinessInfo.GetField("FBaseUnitId").AsType<BaseDataField>()
                            .Adaptive(field => 
                            {
                                if (!sub.FieldProperty<DynamicObject>(field).FieldRefProperty<bool>(field, "FPHMXEA"))
                                {
                                    targetBillService.UpdateValue("FEnableCapacity", -1, true);
                                    targetBillService.UpdateValue("FCapacityUnit", -1, new LocaleValue(sub.FieldProperty<DynamicObject>(field).BDName(this.Context)));
                                    targetBillService.UpdateValue("FCapacityScale", -1, sub.FieldProperty<DynamicObject>(field).FieldRefProperty<int>(field, "FPrecision"));
                                }//end if
                            });
                    });

                //库存
                data.SubHeadProperty(this.BusinessInfo.GetEntity("SubHeadEntity1"))
                    .Adaptive(sub => 
                    {
                        //设置分录包装。
                        var pkgId = sub.FieldProperty<DynamicObject>(this.BusinessInfo.GetField("FPHMXPkgId")).PkId<string>();
                        if (targetBusinessInfo.GetEntity("FPackageEntry").DefaultRows < 1) targetBillView.Model.InsertEntryRow("FPackageEntry", 0);
                        targetBillService.SetItemValueByID("FEntryPackageId", pkgId, 0);

                        if (targetBillView.Model.GetValue("FEnableCapacity").ToType<bool>())
                        {
                            targetBillService.UpdateValue("FStdCty", 0, sub.FieldProperty<decimal>(this.BusinessInfo.GetField("FPHMXStdCty")));
                        }//end if

                        //设置默认包装。
                        targetBillService.SetItemValueByID("FPackageId", pkgId, -1);
                    });
                targetBillService.UpdateValue("FEnableLot", -1, data.SubHeadProperty(this.BusinessInfo.GetEntity("SubHeadEntity1")).FieldProperty<bool>(this.BusinessInfo.GetField("FIsBatchManage")));
                targetBillService.UpdateValue("FEnableExpiry", -1, data.SubHeadProperty(this.BusinessInfo.GetEntity("SubHeadEntity1")).FieldProperty<bool>(this.BusinessInfo.GetField("FIsKFPeriod")));
                targetBillService.UpdateValue("FExpPeriod", -1, data.SubHeadProperty(this.BusinessInfo.GetEntity("SubHeadEntity1")).FieldProperty<int>(this.BusinessInfo.GetField("FExpPeriod")));
                targetBillService.UpdateValue("FExpUnit", -1, data.SubHeadProperty(this.BusinessInfo.GetEntity("SubHeadEntity1")).FieldProperty<string>(this.BusinessInfo.GetField("FExpUnit")));
                targetBillService.UpdateValue("FLotExpiryUnique", -1, data.SubHeadProperty(this.BusinessInfo.GetEntity("SubHeadEntity1")).FieldProperty<bool>(this.BusinessInfo.GetField("FIsExpParToFlot")));
                targetBillService.SetItemValueByID("FMirrorId", data.PkId<int>(), -1);
                targetDataObjects.Add(targetBillView.Model.DataObject);
            }//end foreach

            //统一调用上传操作。
            if (targetDataObjects.Any())
            {
                var op = targetDataObjects.DoNothing(this.Context, targetBusinessInfo, "Upload");
                this.OperationResult.MergeResult(op);
            }//end if
        }//end method

    }//end class
}//end namespace
