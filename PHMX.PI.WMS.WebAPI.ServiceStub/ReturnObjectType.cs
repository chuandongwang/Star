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
    public class ReturnObjectType : AbstractWebApiBusinessService
    {
        public ReturnObjectType(KDServiceContext context) : base(context)
        {
        }
        public class ObjectInform
        {
            public string FID { get; set; }//
            public string FNAME { get; set; }  //
            public string FBillTypeNumber { get; set; }//
            public string FPHMXConvertRuleNumber { get; set; }  //
           
        }
        /// <summary>
        /// 返回目标单据、单据类型 目标规则信息
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
                List<ObjectInform> InformData = new List<ObjectInform>();
                ObjectInform Inform1 = new ObjectInform();
                Inform1.FID = "SP_OUTSTOCK";
                Inform1.FNAME = "简单生产退库单";
                Inform1.FBillTypeNumber = "FHTZ04_PHMX";
                Inform1.FPHMXConvertRuleNumber = "BAH_PI_OutNoticeToOUTSTOCK";
                InformData.Add(Inform1);
                ObjectInform Inform2 = new ObjectInform();
                Inform2.FID = "SP_PickMtrl";
                Inform2.FNAME = "简单生产领料单";
                Inform2.FBillTypeNumber = "FHTZ03_PHMX";
                Inform2.FPHMXConvertRuleNumber = "BAH_PI_OutNoticeToPickMtrl";
                InformData.Add(Inform2);
                List<JSONObject> return_data = new List<JSONObject>();
                foreach (var item in InformData)
                {
                    JSONObject data = new JSONObject();
                    data.Add("FID", item.FID);
                    data.Add("FNAME",item.FNAME);
                    data.Add("FBillTypeNumber", item.FBillTypeNumber);
                    data.Add("FPHMXConvertRuleNumber", item.FPHMXConvertRuleNumber);
                    return_data.Add(data);
                }
                JSONObject Finaldata = new JSONObject();
                Finaldata.Add("ObjectType", return_data);
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
