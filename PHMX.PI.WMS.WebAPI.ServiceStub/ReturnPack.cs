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
    public class ReturnPack : AbstractWebApiBusinessService
    {
        public ReturnPack(KDServiceContext context) : base(context)
        {
        }
        /// <summary>
        /// 输入物料fID 返回绑定的包装信息
        /// </summary>

        /// <returns>返回服务结果。</returns>
        public ServiceResult ExecuteService(string materialFid)
        {
            var result = new ServiceResult<List<JSONObject>>();
            var ctx = this.KDContext.Session.AppContext;
            // 检查上下文对象
            if (this.IsContextExpired(result)) return result;
            // 检查传入参数
            if (string.IsNullOrWhiteSpace(materialFid))
            {
                result.Code = (int)ResultCode.Fail;
                result.Message = "物料主键不能为空！";
                return result;
            }
            //获取相关信息 
            try
            {
                string sqlSelect = string.Format(@"/*dialect*/
                 SELECT t2.FID, t2.FNUMBER, t3.FNAME
                 FROM dbo.BAH_T_BD_MATERIAL t
	             LEFT JOIN BAH_T_BD_MATPACKAGE t1 ON t.FID = t1.FID
	             LEFT JOIN BAH_T_BD_PACKAGE t2 ON t1.FPACKAGEID = t2.FID
	             LEFT JOIN BAH_T_BD_PACKAGE_L t3 ON t2.FID = t3.FID
                 WHERE t.FDOCUMENTSTATUS = 'C'
	             AND t.FFORBIDSTATUS = 'A'
	             AND t2.FDOCUMENTSTATUS = 'C'
	             AND t2.FFORBIDSTATUS = 'A'
	             AND t.FID = '{0}'
                 order by t2.FNUMBER


                   
                 ;", materialFid);// or a.num is null

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
                        data.Add("FNUMBER", data_obj["FNUMBER"]);
                        data.Add("FNAME", data_obj["FNAME"]);
                        return_data.Add(data);

                    }
                    Finaldata.Add("PackName", return_data);
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
