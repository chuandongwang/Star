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
    public class ReturnWareArea : AbstractWebApiBusinessService
    {
        public ReturnWareArea(KDServiceContext context) : base(context)
        {
        }
        /// <summary>
        /// 获取库区数据
        /// </summary>
    
        /// <returns>返回服务结果。</returns>
        public ServiceResult ExecuteService( string whid)
        {
            var result = new ServiceResult<List<JSONObject>>();
            var ctx = this.KDContext.Session.AppContext;
            // 检查上下文对象
            if (this.IsContextExpired(result)) return result;
            // 检查传入参数
            //检查传入参数。
            if (string.IsNullOrWhiteSpace(whid))
            {
                result.Code = (int)ResultCode.Fail;
                result.Message = "仓库参数不能为空！";
                return result;
            }
            //获取相关信息 
            try
            {
                //TODO:通过平台动态引擎获取数据
                //TODO:通过平台动态引擎获取数据
                var metadata = FormMetaDataCache.GetCachedFormMetaData(ctx, "BAH_BD_Area");
                var businessInfo = metadata.BusinessInfo;
                var queryParameter = new QueryBuilderParemeter();
                queryParameter.FormId = businessInfo.GetForm().Id;
                queryParameter.SelectItems = SelectorItemInfo.CreateItems("FID,FNumber,FName,FWHId,FWHId.FNumber,FWHId.FName");
                queryParameter.FilterClauseWihtKey = "FDOCUMENTSTATUS = 'C' and FFORBIDSTATUS = 'A' and FWHID ='" + whid + "' ";
                queryParameter.OrderByClauseWihtKey = "FNUMBER";
                //queryParameter.FilterClauseWihtKey = "FDOCUMENTSTATUS = @FDOCUMENTSTATUS";
                //queryParameter.SqlParams.Add(new SqlParam("@FDOCUMENTSTATUS", KDDbType.String, "C"));
                //queryParameter.FilterClauseWihtKey = "FJOINSTATUS = @FJOINSTATUS";
                //queryParameter.SqlParams.Add(new SqlParam("@FJOINSTATUS", KDDbType.String, "A"));
                //queryParameter.FilterClauseWihtKey = "FMANUALCLOSE = @FMANUALCLOSE";
                //queryParameter.SqlParams.Add(new SqlParam("@FMANUALCLOSE", KDDbType.String, " "));
                //var dataObjectCollection = BusinessDataServiceHelper.Load(ctx, businessInfo.GetDynamicObjectType(), queryParameter);
                var dataObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(ctx, queryParameter);




                //var metadata = FormMetaDataCache.GetCachedFormMetaData(ctx, "BAH_BD_Area");
                //var businessInfo = metadata.BusinessInfo;
                //var queryParameter = new QueryBuilderParemeter();
                //queryParameter.FormId = businessInfo.GetForm().Id;
                //queryParameter.FilterClauseWihtKey = "FDOCUMENTSTATUS = @FDOCUMENTSTATUS";
                //queryParameter.SqlParams.Add(new SqlParam("@FDOCUMENTSTATUS", KDDbType.String, "C"));
                //queryParameter.FilterClauseWihtKey = "FFORBIDSTATUS = @FFORBIDSTATUS";
                //queryParameter.SqlParams.Add(new SqlParam("@FFORBIDSTATUS", KDDbType.String, "A"));
                //queryParameter.FilterClauseWihtKey = "FWHID = @FWHID";
                //queryParameter.SqlParams.Add(new SqlParam("@FWHID", KDDbType.String, whid));
                //var dataObjectCollection = BusinessDataServiceHelper.Load(ctx, businessInfo.GetDynamicObjectType(), queryParameter);
                JSONObject Finaldata = new JSONObject();
                List<JSONObject> return_data = new List<JSONObject>();
                foreach (DynamicObject dataObject in dataObjectCollection)
                {
                    JSONObject data = new JSONObject();
                    data.Add("FID", dataObject["FId"].ToString());
                    data.Add("FNUMBER", dataObject["FNumber"].ToString());
                    data.Add("FName", dataObject["FName"].ToString());
                    data.Add("FWHId", dataObject["FWHId"]);
                    data.Add("FWHNumber", dataObject["FWHId_FNumber"]);
                    data.Add("FWHName", dataObject["FWHId_FName"]);
                    return_data.Add(data);
                }
                Finaldata.Add("WareArea", return_data);
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
