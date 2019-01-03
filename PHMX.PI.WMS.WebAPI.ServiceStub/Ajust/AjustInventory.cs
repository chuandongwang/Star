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
using Kingdee.BOS.Util;
using Newtonsoft.Json;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.JSON;

namespace PHMX.PI.WMS.WebAPI.ServiceStub.Ajust
{
    public class AjustInventory : AbstractWebApiBusinessService
    {
        public AjustInventory(KDServiceContext context) : base(context)
        {
        }
        /// <summary>
        /// 上传PDA发货信息
        public class Ajust
        {
            public string FWHID { get; set; }//
            public string FOWNERID { get; set; }  //
            public string FTrackNo { get; set; }//
            public string FLocId { get; set; }  //
            public string FMaterialId { get; set; }  //
            public string FPackageId { get; set; }  //
            public string FLotNo { get; set; }  //
            public string FProduceDate { get; set; }  //
            public string FExpPeriod { get; set; }  //
            public string FExpUnit { get; set; }  //
            public string FUnitId { get; set; }
            public decimal FQty { get; set; }  //
            public decimal FAvgCty { get; set; }  //
            public decimal FCty { get; set; }  //FSTDCTY
            //public decimal FSTDCTY { get; set; }
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
                //List<Ajust> input = JsonConvert.DeserializeObject<List<Ajust>>(data);

                List<Ajust> input = Serializer.Deserialize<List<Ajust>>(data);
                //
                var formId = "BAH_WMS_Adjust";
                var metadata = FormMetaDataCache.GetCachedFormMetaData(ctx, formId);
                var billView = metadata.CreateBillView(ctx);
                var billService = billView.AsDynamicFormViewService();
                billView.AddNew();
                var items = input[0];
                billService.SetItemValueByID("FBatchWHId", items.FWHID, -1);
                billService.SetItemValueByID("FBatchOwnerId", items.FOWNERID, -1);
                //
                billView.Model.ClearNoDataRow();
                billView.Model.BatchCreateNewEntryRow("FEntity", input.Count());

                for (int i = 0; i < input.Count(); i++)
                {
                    JSONObject datas = new JSONObject();
                    billView.Model.SetValue("FTrackNo", input[i].FTrackNo, i);
                    billView.Model.SetItemValueByID("FLocId", input[i].FLocId, i);
                    billView.Model.SetItemValueByID("FMaterialId", input[i].FMaterialId, i);
                    DynamicObject FMaterialId = billView.Model.GetValue("FMaterialId", i) as DynamicObject;
                    DynamicObjectCollection WarehouseSub = FMaterialId["WarehouseSub"] as DynamicObjectCollection;
                    EnableCapacity = WarehouseSub.FirstOrDefault()["EnableCapacity"].ToString();
                    billView.Model.SetItemValueByID("FPackageId", input[i].FPackageId, i);
                    billView.Model.SetValue("FExpPeriod", input[i].FExpPeriod, i);
                    billView.Model.SetValue("FExpUnit", input[i].FExpUnit, i);
                    //billView.Model.SetValue("FDirectionForQty", input[i].FDirectionForQty, i);
                    if (EnableCapacity == "False")
                    {
                        if (input[i].FQty > 0)
                        {
                            billService.UpdateValue("FDirectionForQty", i, "Increase");
                            billView.Model.SetValue("FQty", input[i].FQty, i);
                            billView.Model.SetItemValueByID("FUnitId", input[i].FUnitId, i);
                        }
                        else
                        {
                            billService.UpdateValue("FDirectionForQty", i, "Reduce");
                            billView.Model.SetValue("FQty", -input[i].FQty, i);
                            billView.Model.SetItemValueByID("FUnitId", input[i].FUnitId, i);
                        }
                    }
                    else
                    {
                        if (input[i].FAvgCty > 0)
                        {
                            if(input[i].FQty > 0)
                            {
                                billView.Model.SetValue("FAvgCty", input[i].FAvgCty, i);
                                billService.UpdateValue("FDirectionForQty", i, "Increase");
                                billView.Model.SetValue("FQty", input[i].FQty, i);
                                billView.Model.SetItemValueByID("FUnitId", input[i].FUnitId, i);
                            }
                            else if (input[i].FQty < 0)
                            { 
                                billView.Model.SetValue("FAvgCty", input[i].FAvgCty, i);
                                billService.UpdateValue("FDirectionForQty", i, "Reduce");
                                billView.Model.SetValue("FQty", -input[i].FQty, i);
                                billView.Model.SetItemValueByID("FUnitId", input[i].FUnitId, i);
                            }
                        }
                        else if (input[i].FAvgCty == 0)
                        {
                            if (input[i].FCty > 0)
                            {
                                billView.Model.SetValue("FAvgCty", input[i].FAvgCty, i);
                                billService.UpdateValue("FDirectionForCty", i, "Increase");
                                billView.Model.SetValue("FCty", input[i].FCty, i);

                            }
                            else if (input[i].FCty < 0)
                            {
                                billView.Model.SetValue("FAvgCty", input[i].FAvgCty, i);
                                billService.UpdateValue("FDirectionForCty", i, "Reduce");
                                billView.Model.SetValue("FCty", -input[i].FCty, i);
                            }
                        }
                    }
                    billView.Model.SetValue("FLotNo", input[i].FLotNo, i);
                    if (input[i].FProduceDate != null)
                    {
                        billView.Model.SetValue("FProduceDate", input[i].FProduceDate, i);
                    }
                } //
                billView.Model.ClearNoDataRow();
                var op = billView.Model.DataObject.DoNothing(ctx, billView.BillBusinessInfo, "Upload");
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

    }
}

