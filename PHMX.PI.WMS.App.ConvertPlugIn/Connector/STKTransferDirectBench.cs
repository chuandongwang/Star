using BAH.BOS.Core.Const.FormOperation;
using BAH.PI.WMS.Core.Const;

using PHMX.PI.WMS.Core.Connector.PlugIn;
using PHMX.PI.WMS.Core.Connector.PlugIn.Args;
using Kingdee.BOS;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.DynamicForm;
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
    [Description("直接调拨单，单据工作台插件")]
    public class STKTransferDirectBench : AbstractBillBenchPlugIn
    {
        public override void AfterCreateTargetData(AfterCreateTargetDataEventArgs e)
        {
            if (!e.Rule.TargetFormId.EqualsIgnoreCase("STK_TransferDirect")) return;

 
            var billService = this.View.AsDynamicFormViewService();
            var businessInfo = this.View.Model.BillBusinessInfo;

          

            //匹配源数据。
            var entryKey = e.Rule.Policies.Where(p => p is DefaultConvertPolicyElement)
                                          .Select(p => p.ToType<DefaultConvertPolicyElement>())
                                          .FirstOrDefault().TargetEntryKey;
            entryKey = entryKey.IsNullOrEmptyOrWhiteSpace() ? "FBillHead" : entryKey;
            var entryLinkKey = businessInfo.GetForm().LinkSet.LinkEntitys.FirstOrDefault().Key;

            var entryCollection = this.View.Model.GetEntityDataObject(businessInfo.GetEntity(entryKey));
            var entryArray = entryCollection.ToArray();
            var materialField = businessInfo.GetField("FMaterialId").AsType<BaseDataField>();
            var stockField = businessInfo.GetField("FStockId").AsType<BaseDataField>();

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

                    //调拨数量。
                    billService.UpdateValue("FQty", rowIndex, item.Cty > 0 ? item.Cty : item.Qty);

                    //库存辅单位。
                    if (materialField.Adaptive(field => this.View.Model.GetValue(field, rowIndex).AsType<DynamicObject>().FieldRefProperty<DynamicObject>(field, "FAuxUnitId")) != null &&
                        materialField.Adaptive(field => this.View.Model.GetValue(field, rowIndex).AsType<DynamicObject>().FieldRefProperty<DynamicObject>(field, "FAuxUnitId")).PkId<int>() != this.View.Model.GetValue("FSecUnitId", rowIndex).AsType<DynamicObject>().PkId<int>())
                    {
                        billService.SetItemValueByID("FSecUnitId", materialField.Adaptive(field => this.View.Model.GetValue(field, rowIndex).AsType<DynamicObject>().FieldRefProperty<DynamicObject>(field, "FAuxUnitId")), rowIndex);
                    }//end if

                    //调拨数量(库存辅单位)
                    if (materialField.Adaptive(field => this.View.Model.GetValue(field, rowIndex).AsType<DynamicObject>().FieldRefProperty<DynamicObject>(field, "FSecUnitId")) != null)
                    {
                        billService.UpdateValue("FSecQty", rowIndex, item.Qty);
                    }//end if

                    //如果WMS通知源单是收货通知
                    if (item.NoticeFormId.EqualsIgnoreCase(PIWMSFormPrimaryKey.Instance.InNotice()))
                    {
                        //调入仓库
                        if (this.View.Model.GetValue("FDestStockId", rowIndex) == null)
                        {
                            billService.SetItemValueByID("FDestStockId", this.View.Model.GetValue(materialField, rowIndex).AsType<DynamicObject>().FieldRefProperty<DynamicObject>(materialField, "FStockId").PkId<int>(), rowIndex);
                        }//end if

                        //调入仓位
                        if (stockField.Adaptive(field => this.View.Model.GetValue(field, rowIndex).AsType<DynamicObject>().FieldRefProperty<bool>(field, "FIsOpenLocation")) &&
                            this.View.Model.GetValue("FDestStockLocId", rowIndex) == null)
                        {
                            billService.SetItemValueByID("FDestStockLocId", this.View.Model.GetValue(materialField, rowIndex).AsType<DynamicObject>().FieldRefProperty<DynamicObject>(materialField, "FStockPlaceId").PkId<int>(), rowIndex);
                        }//end if
                    }//end if

                    //如果WMS通知源单是发货通知
                    if (item.NoticeFormId.EqualsIgnoreCase(PIWMSFormPrimaryKey.Instance.OutNotice()))
                    {
                        //调出仓库
                        if (this.View.Model.GetValue("FSrcStockId", rowIndex) == null)
                        {
                            billService.SetItemValueByID("FSrcStockId", this.View.Model.GetValue(materialField, rowIndex).AsType<DynamicObject>().FieldRefProperty<DynamicObject>(materialField, "FStockId").PkId<int>(), rowIndex);
                        }//end if

                        //调出仓位
                        if (stockField.Adaptive(field => this.View.Model.GetValue(field, rowIndex).AsType<DynamicObject>().FieldRefProperty<bool>(field, "FIsOpenLocation")) &&
                            this.View.Model.GetValue("FSrcStockLocId", rowIndex) == null)
                        {
                            billService.SetItemValueByID("FSrcStockLocId", this.View.Model.GetValue(materialField, rowIndex).AsType<DynamicObject>().FieldRefProperty<DynamicObject>(materialField, "FStockPlaceId").PkId<int>(), rowIndex);
                        }//end if
                    }//end if

                    //调出批号
                    if (materialField.Adaptive(field => this.View.Model.GetValue(field, rowIndex).AsType<DynamicObject>().FieldRefProperty<bool>(field, "FIsBatchManage")))
                    {
                        billService.UpdateValue("FLot", rowIndex, item.LotNo);
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

        public override void BeforeUploadTargetData(BeforeUploadTargetDataEventArgs e)
        {
            if (!e.Rule.TargetFormId.EqualsIgnoreCase("STK_TransferDirect")) return;
            if (e.Argument.SourceFormId.EqualsIgnoreCase("STK_TRANSFERAPPLY") && 
                (e.Argument.ObjectTypeId.EqualsIgnoreCase(PIWMSFormPrimaryKey.Instance.OutNotice()) || e.Argument.ObjectTypeId.EqualsIgnoreCase(PIWMSFormPrimaryKey.Instance.Outbound()) || e.Argument.DataRows.Any(arg => arg.NoticeFormId.EqualsIgnoreCase(PIWMSFormPrimaryKey.Instance.OutNotice())))
               )
            {
                //如果是由发货相关业务触发生成直接调拨单，单据置于提交状态。
                e.Option.SetCutoffOperation(FormOperationEnum.Submit.ToString());
            }//end if
        }//end method

    }//end class
}//end namespace
