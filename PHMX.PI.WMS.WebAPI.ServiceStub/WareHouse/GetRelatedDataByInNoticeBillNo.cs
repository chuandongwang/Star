using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.JSON;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.WebApi.ServicesStub;
using Kingdee.BOS.ServiceFacade.KDServiceFx;
using BAH.BOS.WebAPI.ServiceStub;
using Kingdee.BOS;
using Kingdee.BOS.Core.Metadata.FieldElement;

namespace FYWT.PI.WMS.WebAPI.ServiceStub.WareHouse
{
    public class GetRelatedDataByInNoticeBillNo : AbstractWebApiBusinessService
    {
        public GetRelatedDataByInNoticeBillNo(KDServiceContext context) : base(context)
        {
        }
        /// <summary>
        /// 获取收货通知数据
        /// </summary>
        /// <param name="billno">收货通知单编号</param>
        /// <returns>返回服务结果。</returns>
        public ServiceResult ExecuteService(string billno)
        {
            var result = new ServiceResult<JSONObject>();
            var ctx = this.KDContext.Session.AppContext;
            // 检查上下文对象
            if (this.IsContextExpired(result)) return result;
            // 检查传入参数
            if (string.IsNullOrWhiteSpace(billno))
            {
                result.Code = (int)ResultCode.Fail;
                result.Message = "收货通知单编号不能为空！";
                return result;
            }
            //获取相关信息 
            try
            {
                //TODO:通过平台动态引擎获取数据
                var metadata = FormMetaDataCache.GetCachedFormMetaData(ctx, "BAH_WMS_InNotice");
                var businessInfo = metadata.BusinessInfo;
                var queryParameter = new QueryBuilderParemeter();
                queryParameter.FormId = businessInfo.GetForm().Id;
                queryParameter.FilterClauseWihtKey = "FDOCUMENTSTATUS = @FDOCUMENTSTATUS";
                queryParameter.SqlParams.Add(new SqlParam("@FDOCUMENTSTATUS", KDDbType.String, "C"));
                queryParameter.FilterClauseWihtKey = "FBillNo = @BillNo";
                queryParameter.SqlParams.Add(new SqlParam("@BillNo", KDDbType.String, billno));
                var dataObject = BusinessDataServiceHelper.Load(ctx, businessInfo.GetDynamicObjectType(), queryParameter).FirstOrDefault();
                JSONObject return_data = new JSONObject();
                //表头单据号信息
                return_data.Add("FBillNo", billno);
                //表头仓库信息
                return_data.Add("FBatchWHId", dataObject.FieldProperty<DynamicObject>(businessInfo.GetField("FBatchWHId")).PkId<String>());
                return_data.Add("FBatchWHNumber", businessInfo.GetField("FBatchWHId").AsType<BaseDataField>().Adaptive(field => dataObject.FieldProperty<DynamicObject>(field).FieldRefProperty<string>(field, "Number")));
                return_data.Add("FBatchWHName", dataObject.FieldProperty<DynamicObject>(businessInfo.GetField("FBatchWHId")).BDName(ctx));
                //表头货主信息
                return_data.Add("FBatchOwnerId", dataObject.FieldProperty<DynamicObject>(businessInfo.GetField("FBatchOwnerId")).PkId<String>());
                return_data.Add("FBatchOwnerNumber", businessInfo.GetField("FBatchOwnerId").AsType<BaseDataField>().Adaptive(field => dataObject.FieldProperty<DynamicObject>(field).FieldRefProperty<string>(field, "Number")));
                return_data.Add("FBatchOwnerName", dataObject.FieldProperty<DynamicObject>(businessInfo.GetField("FBatchOwnerId")).BDName(ctx));
                //表头供应商信息
                return_data.Add("FContactId", dataObject.FieldProperty<DynamicObject>(businessInfo.GetField("FContactId")).PkId<String>());
                return_data.Add("FContactNumber", businessInfo.GetField("FContactId").AsType<BaseDataField>().Adaptive(field => dataObject.FieldProperty<DynamicObject>(field).FieldRefProperty<string>(field, "Number")));
                return_data.Add("FContactName", dataObject.FieldProperty<DynamicObject>(businessInfo.GetField("FContactId")).BDName(ctx));
                return_data.Add("FContactFormName", businessInfo.GetField("FContactId").AsType<BaseDataField>().Adaptive(field => dataObject.FieldProperty<DynamicObject>(field).FieldRefProperty<DynamicObject>(field,"FFormId")).BDName(ctx));

                
                DynamicObjectCollection mat_objc = dataObject.EntryProperty(businessInfo.GetEntity("FEntity"));
                List<JSONObject> detail_list = new List<JSONObject>(); 
                //获取明细信息数据
                foreach (DynamicObject data in mat_objc)
                {
                    JSONObject each_detail = new JSONObject();
                    //each_detail.Add("FCUSTMATNUMBER", businessInfo.GetField("FCUSTMATID").AsType<BaseDataField>().Adaptive(field => data.FieldProperty<DynamicObject>(field).FieldRefProperty<string>(field, "Number")));
                    each_detail.Add("FTrackNo", data["TrackNo"].ToString());
                    each_detail.Add("FWrapNo", data["WrapNo"].ToString());
                    each_detail.Add("FOwnerId", data.FieldProperty<DynamicObject>(businessInfo.GetField("FOwnerId")).BDName(ctx));
                    each_detail.Add("FWHId", data.FieldProperty<DynamicObject>(businessInfo.GetField("FWHId")).BDName(ctx));
                    each_detail.Add("FAreaId", businessInfo.GetField("FLocId").AsType<BaseDataField>().Adaptive(field => data.FieldProperty<DynamicObject>(field).FieldRefProperty<DynamicObject>(field, "FAreaId")).BDName(ctx));
                    each_detail.Add("FLocId", data.FieldProperty<DynamicObject>(businessInfo.GetField("FLocId")).BDName(ctx));
                    //each_detail.Add("FMATERIALID", data.FieldProperty<DynamicObject>(businessInfo.GetField("FMATERIALID")).PkId<int>());
                    //each_detail.Add("FMATERIALNUMBER", businessInfo.GetField("FMATERIALID").AsType<BaseDataField>().Adaptive(field => data.FieldProperty<DynamicObject>(field).FieldRefProperty<String>(field, "Number")));
                    //each_detail.Add("FMATERIALNAME", data.FieldProperty<DynamicObject>(businessInfo.GetField("FMATERIALID")).BDName(ctx));
                    //each_detail .Add("FSPECIFICATION", businessInfo.GetField("FMATERIALID").AsType<BaseDataField>().Adaptive(field => data.FieldProperty<DynamicObject>(field).FieldRefProperty<LocaleValue>(field, "Specification").Value(ctx)));
                    detail_list.Add(each_detail);
                }
                return_data.Add("DetailList", detail_list);
                //返回数据
                result.Code = (int)ResultCode.Success;
                result.Data = return_data;
                result.Message = "成功返回数据！";
            }
            catch (Exception ex)
            {
                result.Code = (int)ResultCode.Fail;
                result.Message = ex.Message;
            }
            return result;
        }
    }
}
