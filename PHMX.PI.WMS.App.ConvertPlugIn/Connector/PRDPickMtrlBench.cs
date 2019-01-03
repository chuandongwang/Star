
using PHMX.PI.WMS.Core.Connector.PlugIn;
using PHMX.PI.WMS.Core.Connector.PlugIn.Args;
using Kingdee.BOS;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.Metadata.ConvertElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace PHMX.PI.WMS.App.ConvertPlugIn.Connector
{
    [Description("生产领料单,单据工作台插件")]
    public class PRDPickMtrlBench : AbstractBillBenchPlugIn
    {
        public override void AfterCreatePushArgs(AfterCreatePushArgsEventArgs e)
        {
            if (e.Rule.SourceFormId.EqualsIgnoreCase("PRD_PPBOM") && e.Rule.TargetFormId.EqualsIgnoreCase("PRD_PickMtrl"))
            {
                if (e.Rule.TargetFormMetadata == null)
                {
                    e.Rule.TargetFormMetadata = FormMetaDataCache.GetCachedFormMetaData(this.Context, e.Rule.TargetFormId);
                }//end if

                //给生产领料单默认的单据类型。
                e.PushArgs.TargetBillTypeId = e.Rule.TargetFormMetadata.BusinessInfo.GetBillTypeField().GetDefaultBillTypeInfo().PkId<string>();
            }//end if
        }

        public override void AfterCreateTargetData(AfterCreateTargetDataEventArgs e)
        {
            if (!e.Rule.TargetFormId.EqualsIgnoreCase("PRD_PickMtrl")) return;

         
            var billService = this.View.AsDynamicFormViewService();
            var businessInfo = this.View.Model.BillBusinessInfo;

           

            //匹配源数据。
            var entryKey = e.Rule.Policies.Where(p => p is DefaultConvertPolicyElement)
                                          .Select(p => p.ToType<DefaultConvertPolicyElement>())
                                          .FirstOrDefault().TargetEntryKey;

            entryKey = entryKey.IsNullOrEmptyOrWhiteSpace() ? "FBillHead" : entryKey; //单据头/单据体
            var entryLinkKey = businessInfo.GetForm().LinkSet.LinkEntitys.FirstOrDefault().Key; //关联表设置

            var entryCollection = this.View.Model.GetEntityDataObject(businessInfo.GetEntity(entryKey));
            var entryArray = entryCollection.ToArray();
            var materialField = businessInfo.GetField("FMaterialId").AsType<BaseDataField>(); //物料
            var stockField = businessInfo.GetField("FStockId").AsType<BaseDataField>();      //仓库

            var rows = e.Rows.ToList();
            foreach (var entry in entryArray)
            {
                var sources = rows.Join(entry.Property<DynamicObjectCollection>(entryLinkKey),
                                        left => new { SourceEntryId = left.SId, SourceBillId = left.SBillId },
                                        right => new { SourceEntryId = right.Property<long>("SId"), SourceBillId = right.Property<long>("SBillId") },
                                        (left, right) => left).ToArray();
                if (!sources.Any())
                {
                    entryCollection.Remove(entry);
                    continue;
                }//end if

                for (int i = 0; i < sources.Count(); i++)
                {
                    var entryIndex = entryCollection.IndexOf(entry);
                    var rowIndex = entryIndex + i;
                    if (i > 0)
                    {
                        this.View.Model.CopyEntryRow(entryKey, entryIndex, rowIndex, true);
                    }//end if

                    var item = sources.ElementAt(i);

                    //单位。
                    if (this.View.Model.GetValue("FUnitID", rowIndex).AsType<DynamicObject>().PkId<int>() != this.View.Model.GetValue("FBaseUnitId", rowIndex).AsType<DynamicObject>().PkId<int>())
                    {
                        billService.SetItemValueByID("FUnitID", this.View.Model.GetValue("FBaseUnitId", rowIndex).AsType<DynamicObject>().PkId<int>(), rowIndex);
                    }//end if

                    //实发数量。
                    billService.UpdateValue("FActualQty", rowIndex, item.Cty > 0 ? item.Cty : item.Qty);

                    //辅库存单位。
                    if (materialField.Adaptive(field => this.View.Model.GetValue(field, rowIndex).AsType<DynamicObject>().FieldRefProperty<DynamicObject>(field, "FAuxUnitId")) != null &&
                        materialField.Adaptive(field => this.View.Model.GetValue(field, rowIndex).AsType<DynamicObject>().FieldRefProperty<DynamicObject>(field, "FAuxUnitId")).PkId<int>() != this.View.Model.GetValue("FSecUnitId", rowIndex).AsType<DynamicObject>().PkId<int>())
                    {
                        billService.SetItemValueByID("FSecUnitId", materialField.Adaptive(field => this.View.Model.GetValue(field, rowIndex).AsType<DynamicObject>().FieldRefProperty<DynamicObject>(field, "FAuxUnitId")), rowIndex);
                    }//end if

                    //辅库存单位实发数量
                    if (materialField.Adaptive(field => this.View.Model.GetValue(field, rowIndex).AsType<DynamicObject>().FieldRefProperty<DynamicObject>(field, "FSecUnitId")) != null)
                    {
                        billService.UpdateValue("FSecActualQty", rowIndex, item.Qty);
                    }//end if

                    //仓库
                    if (this.View.Model.GetValue("FStockId", rowIndex) == null)
                    {
                        billService.SetItemValueByID("FStockId", this.View.Model.GetValue(materialField, rowIndex).AsType<DynamicObject>().FieldRefProperty<DynamicObject>(materialField, "FStockId").PkId<int>(), rowIndex);
                    }//end if

                    //仓位
                    if (stockField.Adaptive(field => this.View.Model.GetValue(field, rowIndex).AsType<DynamicObject>().FieldRefProperty<bool>(field, "FIsOpenLocation")) &&
                        this.View.Model.GetValue("FStockLocId", rowIndex) == null)
                    {
                        billService.SetItemValueByID("FStockLocId", this.View.Model.GetValue(materialField, rowIndex).AsType<DynamicObject>().FieldRefProperty<DynamicObject>(materialField, "FStockPlaceId").PkId<int>(), rowIndex);
                    }//end if

                    //批号
                    if (materialField.Adaptive(field => this.View.Model.GetValue(field, rowIndex).AsType<DynamicObject>().FieldRefProperty<bool>(field, "FIsBatchManage")))
                    {
                        billService.SetItemValueByNumber("FLot", item.LotNo, rowIndex);
                    }//end if

                    //保质期
                    if (materialField.Adaptive(field => this.View.Model.GetValue(field, rowIndex).AsType<DynamicObject>().FieldRefProperty<bool>(field, "FIsKFPeriod")))
                    {
                        billService.UpdateValue("FProduceDate", rowIndex, item.ProduceDate.Value);
                        billService.UpdateValue("FExpiryDate", rowIndex, item.ExpiryDate.Value);
                    }//end if

                    //匹配完成后，从待处理列表中移除。
                    rows.Remove(item);
                }//end for

            }//end foreach

            //如果匹配结束后，列表里仍有数据，说明只下推了部分行，必须抛出异常进行阻止。
            if (rows.Any())
            {
                if (e.Rule.SourceFormMetadata == null) e.Rule.SourceFormMetadata = FormMetaDataCache.GetCachedFormMetaData(this.Context, e.Rule.SourceFormId);
                if (e.Rule.TargetFormMetadata == null) e.Rule.TargetFormMetadata = FormMetaDataCache.GetCachedFormMetaData(this.Context, e.Rule.TargetFormId);

                var attention = e.Rows.Select(row => new
                {
                    FormId = row.Parent.ObjectTypeId,
                    BillNo = row.Parent.BillNo
                })
                .Distinct()
                .Select(a => new
                {
                    FormName = FormMetaDataCache.GetCachedFormMetaData(this.Context, a.FormId).Name,
                    a.BillNo
                })
                .Select(a => string.Concat(a.FormName, a.BillNo))
                .ToArray()
                .Adaptive(array => string.Join("、", array));

                //准备反馈的抱错信息。
                var message = string.Format("从{0}下推{1}出现未完全下推的现象，请检查{2}上游单据的状态。",
                                            e.Rule.SourceFormMetadata.Name.Value(this.Context),
                                            e.Rule.TargetFormMetadata.Name.Value(this.Context),
                                            attention);
                throw new KDBusinessException(string.Empty, message);
            }//end if

        }//end method
    }//end class
}//end namesapce
