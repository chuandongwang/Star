using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.JSON;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.WebApi.ServicesStub;
using Kingdee.BOS.ServiceFacade.KDServiceFx;
using BAH.BOS.WebAPI.ServiceStub;
using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using BAH.PI.BD.Contracts;
using Kingdee.BOS.Util;
using System.Web.Script.Serialization;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Log;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata.ConvertElement.ServiceArgs;

namespace PHMX.PI.WMS.WebAPI.ServiceStub.OutGoing
{
    public class CreateOutNoticeAndOutBoundByMaterial : AbstractWebApiBusinessService
    {
        public CreateOutNoticeAndOutBoundByMaterial(KDServiceContext context) : base(context)
        {
        }
        /// <summary>
        /// 快速发货直接生成发货明细
        /// </summary>
        /// <param name="materialNumber">收货通知编号</param>
        /// <returns>返回服务结果。</returns>
        public class InputData
        {
            public string FWHID { get; set; }//
            public string FOWNERID { get; set; }  //
            public string FCONTACTID { get; set; }  //
            public string FTrackNo { get; set; }//
            public string FToTrackNo { get; set; }//
            public string FFromLocId { get; set; }
            public string FToLocId { get; set; }
            public string FMaterialId { get; set; }  //
            public string FPackageId { get; set; }  //
            public string FUnitId { get; set; }
            public decimal FQty { get; set; }  //
            public decimal FAvgCty { get; set; }  //
            public decimal FCty { get; set; }  //FSTDCTY
            public int FExpPeriod { get; set; }
            public string FExpUnit { get; set; }
            public string FProduceDate { get; set; }
            public string FPHMXTargetFormId { get; set; }
            public string FPHMXProduceId { get; set; }
            public string FBillTypeNumber { get; set; }
            public string FPHMXConvertRuleNumber { get; set; }
            public decimal FPHMXWgt { get; set; }


            //public decimal FSTDCTY { get; set; }
        }
       
        public class FinalInputData
        {
            public string FCONTACTID { get; set; }
            public List<InputData> FinalInputDataGroupByContactId { get; set; }
    }
        public ServiceResult ExecuteService(string data)
        {
            ServiceResult result = new ServiceResult();
            if (data.IsNullOrEmptyOrWhiteSpace()) return result;
            String EnableCapacity;
            var ctx = this.KDContext.Session.AppContext;
            // 检查传入字符串数据
            if (this.IsContextExpired(result)) return result;
            // 检查传入参数
            if (string.IsNullOrWhiteSpace(data))
            {
                result.Code = (int)ResultCode.Fail;
                result.Message = "传入表单数据不能为空！";
                return result;
            }
            try
            {
                //序列化
                JavaScriptSerializer Serializer = new JavaScriptSerializer();
                //List<InputData> input = JsonConvert.DeserializeObject<List<InputData>>(data);
                List<FinalInputData> FinalInput = new List<FinalInputData>();
                List<InputData> Input = Serializer.Deserialize<List<InputData>>(data);
                FinalInput = Input.GroupBy(x => x.FCONTACTID).Select(x => new FinalInputData { FCONTACTID = x.Key, FinalInputDataGroupByContactId = x.ToList() }).ToList();
                
                foreach (var items in FinalInput)
                {
                    var formId = "BAH_WMS_OutNotice";
                    var metadata = FormMetaDataCache.GetCachedFormMetaData(ctx, formId);
                    var billView = metadata.CreateBillView(ctx);
                    var billService = billView.AsDynamicFormViewService();
                    billView.AddNew();

                    billService.SetItemValueByNumber("FBillTypeId", items.FinalInputDataGroupByContactId.FirstOrDefault().FBillTypeNumber, -1);

                    billService.SetItemValueByID("FPHMXTargetFormId", items.FinalInputDataGroupByContactId.FirstOrDefault().FPHMXTargetFormId, -1); //

                   

                    billService.SetItemValueByNumber("FPHMXConvertRuleId", items.FinalInputDataGroupByContactId.FirstOrDefault().FPHMXConvertRuleNumber, -1);

                    billService.SetItemValueByID("FBatchWHId", items.FinalInputDataGroupByContactId.FirstOrDefault().FWHID, -1);
                    billService.SetItemValueByID("FBatchOwnerId", items.FinalInputDataGroupByContactId.FirstOrDefault().FOWNERID, -1);
                    billService.SetItemValueByID("FContactId", items.FCONTACTID, -1);
                    billService.SetItemValueByNumber("FPHMXProduceId", items.FinalInputDataGroupByContactId.FirstOrDefault().FPHMXProduceId, -1);

                    billView.Model.BatchCreateNewEntryRow("FEntity", items.FinalInputDataGroupByContactId.Count());
                    for (int i = 0; i < items.FinalInputDataGroupByContactId.Count(); i++)
                    {
                        JSONObject datas = new JSONObject();
                        billView.Model.SetItemValueByID("FMaterialId", items.FinalInputDataGroupByContactId[i].FMaterialId, i);
                        billView.Model.SetItemValueByID("FLocId", items.FinalInputDataGroupByContactId[i].FFromLocId, i);
                        billView.Model.SetItemValueByID("FTrackNo",items.FinalInputDataGroupByContactId[i].FTrackNo,i);

                        DynamicObject FMaterialId = billView.Model.GetValue("FMaterialId", i) as DynamicObject;
                        DynamicObjectCollection WarehouseSub = FMaterialId["WarehouseSub"] as DynamicObjectCollection;
                        EnableCapacity = WarehouseSub.FirstOrDefault()["EnableCapacity"].ToString();
                        billView.Model.SetItemValueByID("FPackageId", items.FinalInputDataGroupByContactId[i].FPackageId, i);
                        //billView.Model.SetValue("FDirectionForQty", input[i].FDirectionForQty, i);
                        if (EnableCapacity == "False")
                        {
                            billView.Model.SetValue("FQty", items.FinalInputDataGroupByContactId[i].FQty, i);
                            billView.Model.SetItemValueByID("FUnitId", items.FinalInputDataGroupByContactId[i].FUnitId, i);
                        }
                        else
                        {
                            if (items.FinalInputDataGroupByContactId[i].FAvgCty == 0)
                            {
                                billView.Model.SetValue("FAvgCty", items.FinalInputDataGroupByContactId[i].FAvgCty, i);
                                billView.Model.SetValue("FCty", items.FinalInputDataGroupByContactId[i].FCty, i);
                            }
                            else
                            {
                                billView.Model.SetValue("FAvgCty", items.FinalInputDataGroupByContactId[i].FAvgCty, i);
                                billView.Model.SetValue("FQty", items.FinalInputDataGroupByContactId[i].FQty, i);
                                billView.Model.SetItemValueByID("FUnitId", items.FinalInputDataGroupByContactId[i].FUnitId, i);
                            }
                        }
                       
                    } //
                    billView.Model.ClearNoDataRow();
                    //var op = billView.Model.DataObject.DoNothing(ctx, billView.BillBusinessInfo, "Upload");
                    bool op = billView.InvokeFormOperation("Save");
                    bool op1 = billView.InvokeFormOperation("Submit");
                    bool op2 = billView.InvokeFormOperation("Audit");
                    List<OutboundDetailLinkInDetailDto.OutboundDetailEntryLinkInNotice> pushdatas = new List<OutboundDetailLinkInDetailDto.OutboundDetailEntryLinkInNotice>();
                    
                    for (int i = 0; i < items.FinalInputDataGroupByContactId.Count(); i++)
                    {
                        OutboundDetailLinkInDetailDto.OutboundDetailEntryLinkInNotice pushdata = new OutboundDetailLinkInDetailDto.OutboundDetailEntryLinkInNotice();
                        pushdata.SourceEntryId = (long)billView.Model.GetEntryPKValue("FEntity", i);     
                        pushdata.PHMXWgt = items.FinalInputDataGroupByContactId[i].FPHMXWgt;
                        pushdatas.Add(pushdata);
                    }
                    var back = CreateNewBillsFromInNoticeEntities(ctx,pushdatas);
                    //foreach (var entry in billView.Model.)
                    //{

                    //}
                    result.Code = op2 == true ? 1 : 0;
                    result.Message = op2 == true ? "成功生成" : "生成失败";
                    billView.Close();
                }
            }
            catch (Exception ex)
            {
                result.Code = (int)ResultCode.Fail;
                result.Message = ex.Message;
                Logger.Error(this.GetType().AssemblyQualifiedName, ex.Message, ex);
            }
            return result;
            
        }
        public IOperationResult CreateNewBillsFromInNoticeEntities(Context ctx, IEnumerable<OutboundDetailLinkInDetailDto.OutboundDetailEntryLinkInNotice> dataArray)
        {

            //取默认转换规则。
            var rule = ConvertServiceHelper.GetConvertRules(ctx, "BAH_WMS_OutNotice", "BAH_WMS_Outbound")
                                     .Where(element => element.IsDefault)
                                     .FirstOrDefault();
            if (rule == null)
            {
                throw new KDBusinessException("RuleNotFound", "未找到发货通知至拣货明细之间，启用的转换规则，无法自动下推！");
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
                        inDetailBillView.Model.SetItemValueByID("FPHMXWgt", item.PHMXWgt, rowIndex);

                    }//end for

                }//end foreach
            }//end foreach

            //调用上传操作，将暂存、保存、提交、审核操作放置在同一事务中执行。
            return inDetailDataObjects.DoNothing(ctx, inDetailBusinessInfo, "Upload");
        }//end method
    }
}
