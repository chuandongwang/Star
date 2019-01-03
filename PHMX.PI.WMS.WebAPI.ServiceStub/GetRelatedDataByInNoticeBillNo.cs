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

namespace FYWT.PI.WMS.WebAPI.ServiceStub
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
               
                var dataObjectCollection = BusinessDataServiceHelper.Load(ctx, businessInfo.GetDynamicObjectType(), queryParameter);
                JSONObject Finaldata = new JSONObject();
                List<JSONObject> return_data = new List<JSONObject>();
                foreach (DynamicObject dataObject in dataObjectCollection)
                {
                    JSONObject data = new JSONObject();
                    data.Add("FID", dataObject["Id"].ToString());
                    data.Add("FNUMBER", dataObject["Number"].ToString());
                    data.Add("FName", dataObject["Name"].ToString());
                    return_data.Add(data);
                }
                Finaldata.Add("WareHouse", return_data);
                //返回数据
                result.Code = (int)ResultCode.Success;
                result.Data = Finaldata;
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
