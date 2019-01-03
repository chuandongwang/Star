using System;
using System.Linq;
using Kingdee.BOS.WebApi.ServicesStub;
using Kingdee.BOS.ServiceFacade.KDServiceFx;
using BAH.BOS.WebAPI.ServiceStub;
using PHMX.PI.WMS.WebAPI.ServiceStub.PickDetailLinkInDetailDto;
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

namespace PHMX.PI.WMS.WebAPI.ServiceStub.OutGoing
{
    public class UploadOutDetailData : AbstractWebApiBusinessService
    {
        public UploadOutDetailData(KDServiceContext context) : base(context)
        {
        }
        /// <summary>
        /// 上传PDA发货信息

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
            PickDetailBillEntryInput[] obj = Serializer.Deserialize<PickDetailBillEntryInput[]>(Ru);
            UploadPickDetailDataInput input = new UploadPickDetailDataInput();
            input.OutNoticeId = 0;
            input.PickDetailBillEntries = obj;
            //var input = TinyMapper.Map<UploadInDetailDataInput>(Rawinput);
            //检查输入参数。
            if (input == null || input.PickDetailBillEntries == null || !input.PickDetailBillEntries.Any())
            {
                result.Code = (int)ResultCode.Fail;
                result.Message = "发货明细数据参数不能为空！";
                return result;
            }//end if

            try
            {
                //var data = TinyMapper.Map<InDetailLinkInNotice>(input);
                //data.InDetailEntries = TinyMapper.Map<InDetailEntryLinkInNoticePlus[]>(input.InDetailBillEntries);
                var data = TinyMapper.Map<PickDetailEntryLinkInNotice[]>(input.PickDetailBillEntries);
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
        //public IOperationResult CreateNewBillFromInNoticeEntry(Context ctx, PutDetailLinkInDetailDto.InDetailEntryLinkInNotice data)
        //{
        //    return this.CreateNewBillsFromInNoticeEntities(ctx, new PutDetailLinkInDetailDto.InDetailEntryLinkInNotice[] { data });
        //}//end method
        /// <summary>
        /// 关联发货明细批量新增拣货明细。
        /// </summary>
        /// <param name="ctx">上下文对象。</param>
        /// <param name="dataArray">拣货明细关联发货通知数据实体数组。</param>
        /// <returns>返回新建保存事务结果。</returns>
        public IOperationResult CreateNewBillsFromInNoticeEntities(Context ctx, IEnumerable<PickDetailLinkInDetailDto.PickDetailEntryLinkInNotice> dataArray)
        {
            
            //取默认转换规则。
            var rule = ConvertServiceHelper.GetConvertRules(ctx, "BAH_WMS_OutNotice", "BAH_WMS_Pickup")
                                     .Where(element => element.IsDefault)
                                     .FirstOrDefault();
            if (rule == null)
            {
                throw new KDBusinessException("RuleNotFound", "未找到发货明细至拣货明细之间，启用的转换规则，无法自动下推！");
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
                        DynamicObject FMaterialId = inDetailBillView.Model.GetValue("FMaterialId", rowIndex) as DynamicObject;
                        DynamicObjectCollection WarehouseSub = FMaterialId["WarehouseSub"] as DynamicObjectCollection;
                        string EnableCapacity = WarehouseSub.FirstOrDefault()["EnableCapacity"].ToString();
                        var item = sources.ElementAt(i);
                        inDetailDynamicFormView.SetItemValueByID("FPHMXWgt", item.PHMXWgt, rowIndex);
                        inDetailDynamicFormView.UpdateValue("FFromTrackNo", rowIndex, item.FromTrackNo);
                        inDetailDynamicFormView.UpdateValue("FToTrackNo", rowIndex, item.ToTrackNo);
                        inDetailDynamicFormView.SetItemValueByID("FFromLocId", item.FromLocId, rowIndex);
                        inDetailDynamicFormView.SetItemValueByID("FToLocId", item.ToLocId, rowIndex);
                        inDetailDynamicFormView.UpdateValue("FLotNo", rowIndex, item.BatchNo);
                        if (EnableCapacity == "False")
                        {
                            inDetailDynamicFormView.SetItemValueByID("FFromUnitId", item.ToUnitId, rowIndex);
                            inDetailDynamicFormView.UpdateValue("FFromQty", rowIndex, item.ToQty);
                        }
                        else
                        {
                            if(item.ToAvgCty == 0)
                            {
                                inDetailDynamicFormView.UpdateValue("FFromCty", rowIndex, item.ToCty);
                            }
                            else
                            {
                                inDetailDynamicFormView.UpdateValue("FFromAvgCty", rowIndex, item.ToAvgCty);
                                inDetailDynamicFormView.SetItemValueByID("FFromUnitId", item.ToUnitId, rowIndex);
                                inDetailDynamicFormView.UpdateValue("FFromQty", rowIndex, item.ToQty);

                            }
                        }
                        //inDetailDynamicFormView.SetItemValueByID("FToUnitId", item.ToUnitId, rowIndex);
                        //inDetailDynamicFormView.UpdateValue("FToQty", rowIndex, item.ToQty);
                        //inDetailDynamicFormView.SetItemValueByID("FStockId", item.StockId, rowIndex);
                        //inDetailDynamicFormView.SetItemValueByID("FStockPlaceId", item.StockPlaceId, rowIndex);
                      
                        //inDetailDynamicFormView.UpdateValue("FProduceDate", rowIndex, item.KFDate);
                        //inDetailDynamicFormView.UpdateValue("FExpPeriod", rowIndex, item.ExpPeriod);
                        //inDetailDynamicFormView.UpdateValue("FSerialNo", rowIndex, item.SerialNo);    
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
