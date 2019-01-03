using System;
using System.Linq;
using Kingdee.BOS.WebApi.ServicesStub;
using Kingdee.BOS.ServiceFacade.KDServiceFx;
using BAH.BOS.WebAPI.ServiceStub;
using PHMX.PI.WMS.WebAPI.ServiceStub.InDetailLinkInNoticeDto;
using Nelibur.ObjectMapper;
using Kingdee.BOS.Log;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Core.Metadata;
using System.Collections.Generic;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata.ConvertElement.ServiceArgs;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Core.Bill;
using System.Web.Script.Serialization;

namespace PHMX.PI.WMS.WebAPI.ServiceStub.WareHouse
{
    public class UploadInDetailData : AbstractWebApiBusinessService
    {
        public UploadInDetailData(KDServiceContext context) : base(context)
        {
        }
        /// <summary>
        /// 上传PDA收货信息
        
        public ServiceResult ExecuteService(string Rawinput)
        {
            var result = new ServiceResult();
            //检查上下文对象。
            var ctx = this.KDContext.Session.AppContext;
            if (this.IsContextExpired(result)) return result;

            int IndexofA = Rawinput.IndexOf("[");
            int IndexofB = Rawinput.IndexOf("]");
            string Ru = Rawinput.Substring(IndexofA, IndexofB - IndexofA + 1);

            JavaScriptSerializer Serializer = new JavaScriptSerializer();
            InDetailBillEntryInput[] obj = Serializer.Deserialize<InDetailBillEntryInput[]>(Ru);
            UploadInDetailDataInput input = new UploadInDetailDataInput();
            input.InNoticeId = 0;
            input.InDetailBillEntries = obj;

            //var input = TinyMapper.Map<UploadInDetailDataInput>(Rawinput);

            //检查输入参数。
            if (input == null || input.InDetailBillEntries == null || !input.InDetailBillEntries.Any())
            {
                result.Code = (int)ResultCode.Fail;
                result.Message = "收货明细数据参数不能为空！";
                return result;
            }//end if

            try
            {
                var data = TinyMapper.Map<InDetailLinkInNoticeDto.InDetailEntryLinkInNotice[]>(input.InDetailBillEntries);
                var op = CreateNewBillsFromInNoticeEntities(ctx, data);
                result.Code = op.IsSuccess ? (int)ResultCode.Success : (int)ResultCode.Fail;
                result.Message = op.GetResultMessage();
            }
            catch (Exception ex)
            {
                result.Code = (int)ResultCode.Fail;
                result.Message = ex.Message;
                Logger.Error(this.GetType().AssemblyQualifiedName, ex.Message, ex);
            }
            return result;
        }

        /// <summary>
        /// 关联到货通知新增收货明细。
        /// </summary>
        /// <param name="ctx">上下文对象。</param>
        /// <param name="data">收货明细关联到货通知数据实体。</param>
        /// <returns>返回新建保存事务结果。</returns>s
        public IOperationResult CreateNewBillFromInNoticeEntry(Context ctx, InDetailLinkInNoticeDto.InDetailEntryLinkInNotice data)
        {
            return this.CreateNewBillsFromInNoticeEntities(ctx, new InDetailLinkInNoticeDto.InDetailEntryLinkInNotice[] { data });
        }
        //end method
        /// <summary>
        /// 关联到货通知批量新增收货明细。
        /// </summary>
        /// <param name="ctx">上下文对象。</param>
        /// <param name="dataArray">收货明细关联到货通知数据实体数组。</param>
        /// <returns>返回新建保存事务结果。</returns>
        public IOperationResult CreateNewBillsFromInNoticeEntities(Context ctx, IEnumerable<InDetailLinkInNoticeDto.InDetailEntryLinkInNotice> dataArray)
        {
        
            //取默认转换规则。
            var rule = ConvertServiceHelper.GetConvertRules(ctx, "BAH_WMS_InNotice", "BAH_WMS_Inbound")
                                     .Where(element => element.IsDefault)
                                     .FirstOrDefault();
            if (rule == null)
            {
                throw new KDBusinessException("RuleNotFound", "未找到到货通知至收货明细之间，启用的转换规则，无法自动下推！");
            }

            ListSelectedRowCollection listSelectedRowCollection = new ListSelectedRowCollection();
            foreach (var data in dataArray)
            {
                var row = new ListSelectedRow(data.SourceBillId.ToString(), data.SourceEntryId.ToString(), 0, rule.SourceFormId) { EntryEntityKey = "FEntity" };
                listSelectedRowCollection.Add(row);
            }//end foreach

            //将需要传入的数量作为参数传递进下推操作，并执行下推操作。
            PushArgs args = new PushArgs(rule, listSelectedRowCollection.ToArray());
            var inDetailDataObjects = ConvertServiceHelper.Push(ctx, args)
                                                    .Adaptive(result => result.ThrowWhenUnSuccess(op => op.GetResultMessage()))
                                                    .Adaptive(result => result.TargetDataEntities.Select(entity => entity.DataEntity).ToArray());

            //修改明细行数据包。
            var inDetailMetadata = FormMetaDataCache.GetCachedFormMetaData(ctx, rule.TargetFormId);
            var inDetailBillView = inDetailMetadata.CreateBillView(ctx);
            var inDetailDynamicFormView = inDetailBillView as IDynamicFormViewService;
            var inDetailBusinessInfo = inDetailMetadata.BusinessInfo;
            var inDetailEntryEntity = inDetailBusinessInfo.GetEntity("FEntity");
            var inDetailEntryLink = inDetailBusinessInfo.GetForm().LinkSet.LinkEntitys.FirstOrDefault();

            foreach (var data in inDetailDataObjects)
            {
                //新建并加载已有数据包。
                inDetailBillView.AddNew(data);

                //逐行检索，并关联复制行。
                var entryCollection = inDetailBillView.Model.GetEntityDataObject(inDetailEntryEntity);
                var entryMirrorArray = entryCollection.ToArray();
                foreach (var entry in entryMirrorArray)
                {
                    var sources = dataArray.Join(entry.Property<DynamicObjectCollection>(inDetailEntryLink.Key),
                                                 left => new { SourceEntryId = left.SourceEntryId, SourceBillId = left.SourceBillId },
                                                 right => new { SourceEntryId = right.Property<long>("SId"), SourceBillId = right.Property<long>("SBillId") },
                                                 (left, right) => left).ToArray();
                    for (int i = 0; i < sources.Count(); i++)
                    {
                        var entryIndex = entryCollection.IndexOf(entry);
                        var rowIndex = entryIndex + i;
                        if (i > 0)
                        {
                            inDetailBillView.Model.CopyEntryRow("FEntity", entryIndex, rowIndex, true);
                        }//end if

                        var item = sources.ElementAt(i);
                        inDetailDynamicFormView.UpdateValue("FTrackNo", rowIndex, item.TrackNo);
                        inDetailDynamicFormView.SetItemValueByID("FLocId", item.LocId, rowIndex);
                        inDetailDynamicFormView.SetItemValueByID("FPackageId", item.PackageId, rowIndex);
                        if (item.BatchNo.Length != 0)
                        {
                            inDetailDynamicFormView.UpdateValue("FLotNo", rowIndex, item.BatchNo);
                        }
                        if (item.KFDate != null)
                        {
                            inDetailDynamicFormView.UpdateValue("FProduceDate", rowIndex, item.KFDate);
                        }
                        if (item.ExpPeriod != 0)
                        {
                            inDetailDynamicFormView.UpdateValue("FExpPeriod", rowIndex, item.ExpPeriod);
                        }
                        if (item.ExpUnit != null)
                        {
                            inDetailDynamicFormView.UpdateValue("FExpUnit", rowIndex, item.ExpUnit);
                        }
                        inDetailDynamicFormView.UpdateValue("FQty", rowIndex, item.Qty);
                        inDetailDynamicFormView.SetItemValueByID("FUnitId", item.UnitId, rowIndex);
                       
                        
                        //inDetailDynamicFormView.SetItemValueByID("FStockId", item.StockId, rowIndex);
                        //inDetailDynamicFormView.SetItemValueByID("FStockPlaceId", item.StockPlaceId, rowIndex);
                    

                        //inDetailDynamicFormView.UpdateValue("FExpPeriod", rowIndex, item.ExpPeriod);
                        //inDetailDynamicFormView.UpdateValue("FSerialNo", rowIndex, item.SerialNo);
                       
                        if(item.AvgCty == 0)
                        {
                            inDetailDynamicFormView.UpdateValue("FAvgCty", rowIndex, item.AvgCty);
                            inDetailDynamicFormView.UpdateValue("FCty", rowIndex, item.Cty);
                        }
                        else
                        {
                            inDetailDynamicFormView.UpdateValue("FAvgCty", rowIndex, item.AvgCty);
                            inDetailDynamicFormView.UpdateValue("FQty", rowIndex, item.Qty);
                            inDetailDynamicFormView.SetItemValueByID("FUnitId", item.UnitId, rowIndex);

                        }
                        //增加重量
                        inDetailDynamicFormView.UpdateValue("FPHMXWgt", rowIndex, item.PHMXWgt);

                        //inDetailDynamicFormView.UpdateValue("FTrayNo", rowIndex, item.TrayNo);
                        //inDetailDynamicFormView.UpdateValue("FEntryRemark", rowIndex, item.Remark);
                    }//end for

                }//end foreach
            }//end foreach

            //调用上传操作，将暂存、保存、提交、审核操作放置在同一事务中执行。
            return inDetailDataObjects.DoNothing(ctx, inDetailBusinessInfo, "Upload");
        }//end method
    }
}
