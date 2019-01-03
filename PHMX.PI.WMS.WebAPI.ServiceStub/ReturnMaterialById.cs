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
    public class ReturnMaterialById : AbstractWebApiBusinessService
    {
        public ReturnMaterialById(KDServiceContext context) : base(context)
        {
        }
        /// <summary>
        /// 输入物料fID 返回物料信息
        /// </summary>

        /// <returns>返回服务结果。</returns>
        public ServiceResult ExecuteService(string materialFNumber)
        {
            var result = new ServiceResult<List<JSONObject>>();
            var ctx = this.KDContext.Session.AppContext;
            // 检查上下文对象
            if (this.IsContextExpired(result)) return result;
            // 检查传入参数
            if (string.IsNullOrWhiteSpace(materialFNumber))
            {
                result.Code = (int)ResultCode.Fail;
                result.Message = "物料主键不能为空！";
                return result;
            }
            //获取相关信息 
            try
            {
                string sqlSelect = string.Format(@"/*dialect*/
                 SELECT t2.FID AS FMaterialFID, t2.FNUMBER AS FMaterialNumber, t3.FNAME AS FMaterialName, t3.FSPECIFICATION
	            ,CASE T8.FENABLELOT WHEN 1 then 'True' ELSE 'False' END AS FENABLELOT ,
	            CASE T8.FENABLEEXPIRY WHEN 1 then 'True' ELSE 'False' END AS FENABLEEXPIRY,
                CASE T8.FENABLECAPACITY WHEN 1 then 'True' ELSE 'False' END AS FENABLECAPACITY,
	            t8.FCAPACITYSCALE,t10.FCAPACITYUNIT, t8.FEXPPERIOD,t8.FEXPUNIT,
	            T11.FPACKAGEID,t12.FNAME AS FPackageName,T13.FMAINUNITID,T14.FNAME AS FMainUnitName
                FROM dbo.BAH_T_BD_MATERIAL t2 
	            LEFT JOIN dbo.BAH_T_BD_MATERIAL_L t3 ON t2.FID = t3.FID
	            INNER JOIN dbo.BAH_T_BD_MATWAREHOUSE T8 ON T2.FID = T8.FID
	            LEFT JOIN dbo.BAH_T_BD_MATWAREHOUSE_L T10 ON T8.FENTRYID = T10.FENTRYID
	            LEFT JOIN BAH_T_BD_MATPACKAGE T11 ON T2.FID = T11.FID
	            LEFT JOIN dbo.BAH_T_BD_PACKAGE_L T12 ON T11.FPACKAGEID = T12.FID
	            LEFT JOIN dbo.BAH_T_BD_PACKAGE T13 ON T11.FPACKAGEID = T13.FID
	            LEFT JOIN dbo.BAH_T_BD_PKGUOM_L T14 ON T13.FMAINUNITID = t14.FENTRYID
                WHERE t2.FNUMBER = '{0}'


                   
                 ;", materialFNumber);// or a.num is null

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

                        data.Add("FMaterialFID", data_obj["FMaterialFID"]);
                        data.Add("FMaterialNumber", data_obj["FMaterialNumber"]);
                        data.Add("FMaterialName", data_obj["FMaterialName"]);
                        data.Add("FSPECIFICATION", data_obj["FSPECIFICATION"]);
                        data.Add("FENABLELOT", data_obj["FENABLELOT"]);
                        data.Add("FENABLEEXPIRY", data_obj["FENABLEEXPIRY"]);
                        data.Add("FENABLECAPACITY", data_obj["FENABLECAPACITY"]);
                        data.Add("FCAPACITYSCALE", data_obj["FCAPACITYSCALE"]);
                        data.Add("FCAPACITYUNIT", data_obj["FCAPACITYUNIT"]);
                        data.Add("FEXPPERIOD", data_obj["FEXPPERIOD"]);
                        data.Add("FEXPUNIT", data_obj["FEXPUNIT"]);
                        data.Add("FPACKAGEID", data_obj["FPACKAGEID"]);
                        data.Add("FPackageName", data_obj["FPackageName"]);
                        data.Add("FMAINUNITID", data_obj["FMAINUNITID"]);
                        data.Add("FMainUnitName", data_obj["FMainUnitName"]);
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
