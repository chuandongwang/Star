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
    public class ReturnWareLocationByNumber : AbstractWebApiBusinessService
    {
        public ReturnWareLocationByNumber(KDServiceContext context) : base(context)
        {
        }
        /// <summary>
        /// 通过库位FNumber获取Fid和FName
        /// </summary>
        /// <returns>返回服务结果。</returns>
        public ServiceResult ExecuteService(string locationnumber, string whid, string areaid)
        {
            var result = new ServiceResult<List<JSONObject>>();
            var ctx = this.KDContext.Session.AppContext;
            // 检查上下文对象
            if (this.IsContextExpired(result)) return result;
            // 检查传入参数
            if (string.IsNullOrWhiteSpace(locationnumber))
            {
                result.Code = (int)ResultCode.Fail;
                result.Message = "库位FNumber不能为空！";
                return result;
            }
            //获取相关信息 
            try
            {
                string sqlSelect = string.Format(@"/*dialect*/
              SELECT t.FID,t1.FNAME,
              CASE T2.FIGNOREINVENTORYTRACKNO WHEN 1 then 'True' ELSE 'False' END AS FIGNOREINVENTORYTRACKNO,t3.FUSE 
              FROM dbo.BAH_T_BD_LOCATION t
              left join dbo.BAH_T_BD_LOCATION_L t1 on t.FID = t1.FID
              INNER JOIN BAH_T_BD_LOCCONTROL T2 ON T.FID = T2.FID
              LEFT JOIN BAH_T_BD_LOCBASE T3 ON T.FID = T3.FID
              LEFT JOIN BAH_T_BD_AREABASE T4 ON T3.FAREAID = T4.FID
              where FDOCUMENTSTATUS = 'C' AND FFORBIDSTATUS = 'A'
              AND t.FNUMBER  = '{0}'AND T3.FAREAID LIKE '%{1}%' AND T4.FWHID LIKE '%{2}%'  
                 ;", locationnumber,areaid,whid);// or a.num is null
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
                        data.Add("FIGNOREINVENTORYTRACKNO", data_obj["FIGNOREINVENTORYTRACKNO"]);
                        data.Add("FUSE", data_obj["FUSE"]);
                        return_data.Add(data);

                    }
                    Finaldata.Add("LocationDetail", return_data);
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
