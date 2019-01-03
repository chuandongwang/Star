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
using Kingdee.BOS.App.Data;

namespace PHMX.PI.WMS.WebAPI.ServiceStub
{
    public class ReturnPackNameByPackId : AbstractWebApiBusinessService
    {
        public ReturnPackNameByPackId(KDServiceContext context) : base(context)
        {
        }
        /// <summary>
        /// 通过包装FID获取包装单位
        /// </summary>

        /// <returns>返回服务结果。</returns>
        public ServiceResult ExecuteService(string packfid)
        {
            var result = new ServiceResult<List<JSONObject>>();
            var ctx = this.KDContext.Session.AppContext;
            // 检查上下文对象
            if (this.IsContextExpired(result)) return result;
            // 检查传入参数
            if (string.IsNullOrWhiteSpace(packfid))
            {
                result.Code = (int)ResultCode.Fail;
                result.Message = "包装主键不能为空！";
                return result;
            }
            //获取相关信息 
            try
            {
                string sqlSelect = string.Format(@"/*dialect*/
                SELECT t4.FENTRYID as FID,t5.FNAME,t4.FQTY
                FROM dbo.BAH_T_BD_PKGUOM t4
                LEFT JOIN dbo.BAH_T_BD_PKGUOM_L t5 ON t4.FENTRYID = t5.FENTRYID
                WHERE t5.FNAME IS NOT NULL and t4.FQTY > 0
                AND t4.FID = '{0}'


                   
                 ;", packfid);// or a.num is null

                DynamicObjectCollection query_result = DBUtils.ExecuteDynamicObject(ctx, sqlSelect, null, null);

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
                        data.Add("FNAME", data_obj["FNAME"]);
                        data.Add("FQTY", data_obj["FQTY"]);
                        return_data.Add(data);

                    }
                    Finaldata.Add("PackDetail", return_data);
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
