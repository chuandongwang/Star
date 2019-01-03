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
    public class ReturnContacter : AbstractWebApiBusinessService
    {
        public ReturnContacter(KDServiceContext context) : base(context)
        {
        }
        /// <summary>
        /// 返回供应商数据
        /// </summary>
    
        /// <returns>返回服务结果。</returns>
        public ServiceResult ExecuteService()
        {
            var result = new ServiceResult<List<JSONObject>>();
            var ctx = this.KDContext.Session.AppContext;
            // 检查上下文对象
            if (this.IsContextExpired(result)) return result;
            // 检查传入参数

            //获取相关信息 
            try
            {
               
                StringBuilder sql_builder = new StringBuilder("/*dialect*/ select v.FID ,v.FNUMBER,v1.FNAME,v.fformid as FFORMID from dbo.BAH_V_BD_CONTACT v  ");
                sql_builder.Append(" inner join dbo.BAH_V_BD_CONTACT_L v1 on v.FID = v1.FID");
                sql_builder.Append(" where v.FDOCUMENTSTATUS = 'C' and v.FFORBIDSTATUS = 'A' AND V1.FLOCALEID = 2052");
                sql_builder.Append(" order by v.FNUMBER ");
                DynamicObjectCollection query_result = DBServiceHelper.ExecuteDynamicObject(ctx, sql_builder.ToString(), null, null, System.Data.CommandType.Text);

                if (query_result.Count == 0)
                {
                    result.Code = (int)ResultCode.Fail;
                    result.Message = "未检索到对应信息！";
                }
                else
                {
                    result.Code = (int)ResultCode.Success;
                    JSONObject Finaldata = new JSONObject();
                    List<JSONObject> return_data = new List<JSONObject>();
                    //构建JSONObject数据
                    foreach (DynamicObject data_obj in query_result)
                    {
                        JSONObject data = new JSONObject();
                        data.Add("FID", data_obj["FID"]);
                        data.Add("FNUMBER", data_obj["FNUMBER"]);
                        data.Add("FNAME", data_obj["FNAME"]);
                        data.Add("FFORMID", data_obj["FFORMID"]);
                        return_data.Add(data);

                    }
                    Finaldata.Add("Contacter", return_data);
                    //返回数据
                    result.Code = (int)ResultCode.Success;
                    result.Data = return_data;
                    result.Message = "成功返回数据！";
                }
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
