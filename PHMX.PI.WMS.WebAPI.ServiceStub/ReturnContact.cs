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

namespace PHMX.PI.WMS.WebAPI.ServiceStub
{
    public class ReturnContact : AbstractWebApiBusinessService
    {
        public ReturnContact(KDServiceContext context) : base(context)
        {
        }
        /// <summary>
        /// 获取客户数据
        /// </summary>

        /// <returns>返回服务结果。</returns>
        public ServiceResult ExecuteService()
        {
            var result = new ServiceResult<List<JSONObject>>();
            var ctx = this.KDContext.Session.AppContext;
            // 检查上下文对象
            if (this.IsContextExpired(result)) return result;
            
           
            //获取相关信息 
            try
            {
                //TODO:通过平台动态引擎获取数据
                var metadata = FormMetaDataCache.GetCachedFormMetaData(ctx, "BAH_BD_Customer");
                var businessInfo = metadata.BusinessInfo;
                var queryParameter = new QueryBuilderParemeter();
                queryParameter.FormId = businessInfo.GetForm().Id;
                queryParameter.SelectItems = SelectorItemInfo.CreateItems("FID,FNUMBER,FName");
                queryParameter.FilterClauseWihtKey = "FDOCUMENTSTATUS = 'C' and FFORBIDSTATUS = 'A'";
                queryParameter.OrderByClauseWihtKey = "FNUMBER";
                var dataObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(ctx, queryParameter);


                //queryParameter.FilterClauseWihtKey = "FDOCUMENTSTATUS = @FDOCUMENTSTATUS";
                //queryParameter.SqlParams.Add(new SqlParam("@FDOCUMENTSTATUS", KDDbType.String, "C"));
                //queryParameter.FilterClauseWihtKey = "FFORBIDSTATUS = @FFORBIDSTATUS";
                //queryParameter.SqlParams.Add(new SqlParam("@FFORBIDSTATUS", KDDbType.String, "A"));
                //var dataObjectCollection = BusinessDataServiceHelper.Load(ctx, businessInfo.GetDynamicObjectType(), queryParameter);
                JSONObject Finaldata = new JSONObject();
                List<JSONObject> return_data = new List<JSONObject>();
                foreach (DynamicObject dataObject in dataObjectCollection)
                {
                    JSONObject data = new JSONObject();
                    data.Add("FID", dataObject["FId"].ToString());
                    data.Add("FNUMBER", dataObject["FNumber"].ToString());
                    data.Add("FName", dataObject["FName"].ToString());
                    return_data.Add(data);
                }
                Finaldata.Add("Contact", return_data);
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
