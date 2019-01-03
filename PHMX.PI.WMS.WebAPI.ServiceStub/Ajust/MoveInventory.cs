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

namespace PHMX.PI.WMS.WebAPI.ServiceStub.Ajust
{
    public class MoveInventory : AbstractWebApiBusinessService
    {
        public MoveInventory(KDServiceContext context) : base(context)
        {
        }
        /// <summary>
        /// 上传PDA发货信息
        public class Ajust
        {
            public string FFROMWHID { get; set; }//
            public string FFROMOWNERID { get; set; }  //
            public string FMaterialId { get; set; }  //
            public string FFROMTrackNo { get; set; }//
            public string FFROMLocId { get; set; }  //
            public string FTOTrackNo { get; set; }//
            public string FTOLocId { get; set; }  //
            public string FFromPackageId { get; set; }
            public string FLotNo { get; set; }  //
            public string FProduceDate { get; set; }  //
            public string FExpPeriod { get; set; }  //
            public string FExpUnit { get; set; }  //
            public decimal FFromQty { get; set; }  //
            public string  FFromUnitId { get; set; }  //
            public decimal FFromAvgCty { get; set; }  //
            public decimal FFromCty { get; set; }  //FSTDCTY
            //public decimal FSTDCTY { get; set; }
        }
        public ServiceResult ExecuteService(string data)
        {
            ServiceResult result = new ServiceResult();
            String EnableCapacity;
            if (data.IsNullOrEmptyOrWhiteSpace()) return result;

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
                List<Ajust> input = Serializer.Deserialize<List<Ajust>>(data);
                //
                var formId = "BAH_WMS_Move";
                var metadata = FormMetaDataCache.GetCachedFormMetaData(ctx, formId);
                var billView = metadata.CreateBillView(ctx);
                var billService = billView.AsDynamicFormViewService();
                billView.AddNew();
                var items = input[0];
                billService.SetItemValueByID("FBatchFromWHId", items.FFROMWHID, -1);
                billService.SetItemValueByID("FBatchFromOwnerId", items.FFROMOWNERID, -1);
                //
                billView.Model.ClearNoDataRow();
                billView.Model.BatchCreateNewEntryRow("FEntity", input.Count());
                for (int i = 0; i < input.Count(); i++)
                {
                    billView.Model.SetItemValueByID("FMaterialId", input[i].FMaterialId, i);
                    DynamicObject FMaterialId = billView.Model.GetValue("FMaterialId", i) as DynamicObject;
                    if(FMaterialId == null)
                    {
                        result.Code = (int)ResultCode.Fail;
                        result.Message = "该物料不存在或已禁用！";
                        return result;

                    }
                    else
                    {
                        DynamicObjectCollection WarehouseSub = FMaterialId["WarehouseSub"] as DynamicObjectCollection;
                        EnableCapacity = WarehouseSub.FirstOrDefault()["EnableCapacity"].ToString();
                    }
                   
                    billView.Model.SetValue("FFromTrackNo", input[i].FFROMTrackNo, i);
                    billView.Model.SetItemValueByID("FFromLocId", input[i].FFROMLocId, i);
                    billView.Model.SetItemValueByID("FToLocId", input[i].FTOLocId, i);
                    billView.Model.SetValue("FToTrackNo", input[i].FTOTrackNo, i);
                    //billView.Model.SetItemValueByID("FPackageId",input[i].FPackageId, i);
                    billView.Model.SetItemValueByID("FFromPackageId", input[i].FFromPackageId, i);
                    billView.Model.SetValue("FExpPeriod", input[i].FExpPeriod, i);
                    billView.Model.SetValue("FExpUnit", input[i].FExpUnit, i);
                    if (EnableCapacity == "False")
                    {
                        billView.Model.SetValue("FFromQty", input[i].FFromQty, i);
                        billView.Model.SetItemValueByID("FFromUnitId", input[i].FFromUnitId, i);
                    }
                    else
                    {
                        if (input[i].FFromAvgCty > 0)
                        {
                            billView.Model.SetValue("FFromAvgCty", input[i].FFromAvgCty, i);
                            billView.Model.SetValue("FFromQty", input[i].FFromQty, i);
                            billView.Model.SetItemValueByID("FFromUnitId", input[i].FFromUnitId, i);
                        }
                        else 
                        {
                            billView.Model.SetValue("FFromAvgCty", input[i].FFromAvgCty, i);
                            billView.Model.SetValue("FFromQty", input[i].FFromQty, i);
                            billView.Model.SetValue("FFromCty", input[i].FFromCty, i);
                        }
                    }
                    billView.Model.SetValue("FLotNo", input[i].FLotNo, i);
                    if (input[i].FProduceDate != null)
                    {
                        billView.Model.SetValue("FProduceDate", input[i].FProduceDate, i);
                    }
                }
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

