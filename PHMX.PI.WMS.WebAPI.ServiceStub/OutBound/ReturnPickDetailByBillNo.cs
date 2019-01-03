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
using BAH.PI.BD.Contracts;

namespace PHMX.PI.WMS.WebAPI.ServiceStub.OutBound
{
    public class ReturnPickDetailByBillNo : AbstractWebApiBusinessService
    {
        public ReturnPickDetailByBillNo(KDServiceContext context) : base(context)
        {
        }
        /// <summary>
        /// 输入拣货通知编号，输出拣货通知详情
        /// </summary>
        /// <param name="billno">拣货通知编号</param>
        /// <returns>返回服务结果。</returns>
        public ServiceResult ExecuteService(string whid, string areaid, string ownerid, string billno, string trackno)
        {

            DynamicObjectCollection dataObjectCollection;
            List<JSONObject> return_data = new List<JSONObject>();
            var result = new ServiceResult<List<JSONObject>>();
            var ctx = this.KDContext.Session.AppContext;
            // 检查上下文对象
            if (this.IsContextExpired(result)) return result;
            // 检查传入参数
            //if (string.IsNullOrWhiteSpace(billno))
            //{
            //    result.Code = (int)ResultCode.Fail;
            //    result.Message = "收货明细编号不能为空！";
            //    return result;
            //}
            //获取相关信息 
            try
            {
                //TODO:通过平台动态引擎获取数据
                string sqlSelect1 = string.Format(@"/*dialect*/
                                                    SELECT distinct t0.FID fid, t0.FBILLNO fbillno 
                                                    FROM dbo.BAH_T_WMS_PICKUP T0
                                                    LEFT JOIN dbo.BAH_T_WMS_PICKUPENTRY_B T ON T0.FID = T.FID
                                                    LEFT JOIN dbo.BAH_T_BD_LOCATION_L t1 ON t.FTOLOCID = t1.FID
                                                    LEFT JOIN dbo.BAH_T_WMS_PICKUPENTRY_W T2 ON T.FENTRYID = T2.FENTRYID
                                                    LEFT JOIN dbo.BAH_T_BD_LOCATION t3 ON t.FTOLOCID = t3.FID
                                                    LEFT JOIN dbo.BAH_T_BD_LOCBASE T30 ON T3.FID = T30.FID
                                                    LEFT JOIN dbo.BAH_T_BD_WAREHOUSE t4 on t0.FTOWHID = t4.FID
                                                    INNER JOIN dbo.BAH_T_BD_WAREHOUSE_L t5 on t4.FID = t5.FID
                                                    LEFT JOIN dbo.BAH_V_BD_OWNER t6 ON t0.FTOOWNERID = t6.FID
                                                    INNER JOIN dbo.BAH_V_BD_OWNER_L T7 ON T6.FID = T7.FID
                                                    LEFT JOIN dbo.BAH_V_BD_CONTACT T8 ON T0.FCONTACTID = T8.FID
                                                    INNER JOIN dbo.BAH_V_BD_CONTACT_L T9 ON T8.FID = T9.FID
                                                    LEFT JOIN dbo.BAH_T_WMS_OUTNOTICEENTRY T10 ON T2.FSOURCEID = T10.FENTRYID
                                                    LEFT JOIN dbo.BAH_T_WMS_PICKUPENTRY T100 ON T0.FID=T100.FID
                                                    INNER JOIN dbo.BAH_T_BD_MATWAREHOUSE T11 ON T100.FMATERIALID = T11.FID
                                                    LEFT JOIN dbo.BAH_T_WMS_OUTNOTICEENTRY_W T12 ON T10.FENTRYID = T12.FENTRYID
                                                    WHERE t0.FDOCUMENTSTATUS = 'C' AND t2.FJOINSTATUS = 'A' 
                                                    AND ((T2.FJoinMQty < T.FTOMQTY AND T11.FENABLECAPACITY = 0) OR (T11.FENABLECAPACITY = 1 AND (T2.FJOINCTY < T.FTOCTY OR T2.FJOINCTY = 0))) 


                                                    AND T0.FTOWHID LIKE '%{0}%' AND T0.FTOOWNERID LIKE '%{2}%' AND T30.FAREAID LIKE '%{1}%'  and (t0.FBILLNO = '{3}' or t2.FSOURCEBILLNO  = '{3}' or T.FTOTRACKNO = '{3}' or T12.FSOURCEBILLNO ='{3}' or t3.FNUMBER = '{3}' )
                                                    AND T30.FUSE ='Transition'
                                                                                                            
                 ;", whid, areaid, ownerid, billno, trackno);// or a.num is null
                                                             //库区为零的情况
                string sqlSelect2 = string.Format(@"/*dialect*/
                                                    SELECT distinct t0.FID fid, t0.FBILLNO fbillno 
                                                    FROM dbo.BAH_T_WMS_PICKUP T0
                                                    LEFT JOIN dbo.BAH_T_WMS_PICKUPENTRY_B T ON T0.FID = T.FID
                                                    LEFT JOIN dbo.BAH_T_BD_LOCATION_L t1 ON t.FTOLOCID = t1.FID
                                                    LEFT JOIN dbo.BAH_T_WMS_PICKUPENTRY_W T2 ON T.FENTRYID = T2.FENTRYID
                                                    LEFT JOIN dbo.BAH_T_BD_LOCATION t3 ON t.FTOLOCID = t3.FID
                                                    LEFT JOIN dbo.BAH_T_BD_LOCBASE T30 ON T3.FID = T30.FID
                                                    LEFT JOIN dbo.BAH_T_BD_WAREHOUSE t4 on t0.FTOWHID = t4.FID
                                                    INNER JOIN dbo.BAH_T_BD_WAREHOUSE_L t5 on t4.FID = t5.FID
                                                    LEFT JOIN dbo.BAH_V_BD_OWNER t6 ON t0.FTOOWNERID = t6.FID
                                                    INNER JOIN dbo.BAH_V_BD_OWNER_L T7 ON T6.FID = T7.FID
                                                    LEFT JOIN dbo.BAH_V_BD_CONTACT T8 ON T0.FCONTACTID = T8.FID
                                                    INNER JOIN dbo.BAH_V_BD_CONTACT_L T9 ON T8.FID = T9.FID
                                                    LEFT JOIN dbo.BAH_T_WMS_OUTNOTICEENTRY T10 ON T2.FSOURCEID = T10.FENTRYID
                                                    LEFT JOIN dbo.BAH_T_WMS_PICKUPENTRY T100 ON T0.FID=T100.FID
                                                    INNER JOIN dbo.BAH_T_BD_MATWAREHOUSE T11 ON T100.FMATERIALID = T11.FID
                                                    LEFT JOIN dbo.BAH_T_WMS_OUTNOTICEENTRY_W T12 ON T10.FENTRYID = T12.FENTRYID
                                                    WHERE t0.FDOCUMENTSTATUS = 'C' AND t2.FJOINSTATUS = 'A' 
                                                   
                                                    AND ((T2.FJoinMQty < T.FTOMQTY AND T11.FENABLECAPACITY = 0) OR (T11.FENABLECAPACITY = 1 AND (T2.FJOINCTY < T.FTOCTY OR T2.FJOINCTY = 0))) 


                                                    AND T0.FTOWHID LIKE '%{0}%' AND T0.FTOOWNERID LIKE '%{2}%'  and (t0.FBILLNO = '{3}' or t2.FSOURCEBILLNO  = '{3}' or T.FTOTRACKNO = '{3}' or T12.FSOURCEBILLNO ='{3}' or t3.FNUMBER = '{3}' )
                                                    AND T30.FUSE ='Transition'
                                                                                                           
                 ;", whid, areaid, ownerid, billno, trackno);// or a.num is null
                //TODO:通过平台动态引擎获取数据
                if (areaid == "")
                {
                    dataObjectCollection = DBUtils.ExecuteDynamicObject(ctx, sqlSelect2, null, null);
                }
                else
                {
                    dataObjectCollection = DBUtils.ExecuteDynamicObject(ctx, sqlSelect1, null, null);
                }

                foreach (DynamicObject dataObject in dataObjectCollection)
                {

                    string sqlSelect = string.Format(@"/*dialect*/
               
              SELECT T11.FBILLNO,t.FID,t.FENTRYID, T0.FSEQ, t.FTOLOCID, t6.FNUMBER AS FlocIdNumber, t1.FNAME AS FlocIdName
	            ,t2.FID AS FMaterialFID, t2.FNUMBER AS FMaterialNumber, t3.FNAME AS FMaterialName, t3.FSPECIFICATION, t.FTOPACKAGEID,T7.FNAME AS FPackageName, t.FTOQTY
	            , t.FTOUNITID,  t4.FNAME AS FUnitName,t.FTOMQTY,T.FTOMUNITID,t9.FNAME AS FMUnitName, t0.FLotNo,t.FTOTRACKNO,t0.FPRODUCEDATE,
	            t5.FHasJoinMQty,(t.FTOMQTY - t5.FHasJoinMQty) AS FNeedOUTBOUNDMQTY,
	            CASE T8.FENABLELOT WHEN 1 then 'True' ELSE 'False' END AS FENABLELOT ,
	            CASE T8.FENABLEEXPIRY WHEN 1 then 'True' ELSE 'False' END AS FENABLEEXPIRY,
	            CASE T8.FENABLECAPACITY WHEN 1 then 'True' ELSE 'False' END AS FENABLECAPACITY,
	            t8.FCAPACITYSCALE,t10.FCAPACITYUNIT,
	            t.FTOAVGCTY,t.FTOCTY,
                t5.FHasJoinCty,(t.FTOCTY - t5.FHasJoinCty) AS FNeedCTY,
                t0.FEXPIRYDATE,
                t5.FSOURCEBILLNO,T13.FNAME AS FCONTACTNAME,
                T0.FPHMXWGT
                FROM dbo.BAH_T_WMS_PICKUPENTRY_B t
                LEFT JOIN dbo.BAH_T_WMS_PICKUPENTRY T0 ON T.FENTRYID = T0.FENTRYID
	            LEFT JOIN dbo.BAH_T_BD_LOCATION_L t1 ON t.FTOLOCID = t1.FID
	            LEFT JOIN dbo.BAH_T_BD_MATERIAL t2 ON T0.FMATERIALID = t2.FID
	            LEFT JOIN dbo.BAH_T_BD_MATERIAL_L t3 ON T0.FMATERIALID = t3.FID
	            LEFT JOIN dbo.BAH_V_BD_UNIT_L t4 ON t.FTOUNITID = t4.fid
	            LEFT JOIN dbo.BAH_T_WMS_PICKUPENTRY_W t5 ON t.FENTRYID = t5.FENTRYID
	            LEFT JOIN dbo.BAH_T_BD_LOCATION t6 ON t.FTOLOCID = t6.FID
                LEFT JOIN dbo.BAH_T_BD_PACKAGE_L T7 ON T.FTOPACKAGEID = T7.FID
                LEFT JOIN dbo.BAH_V_BD_UNIT_L t9 ON t.FTOMUNITID = t9.fid
                INNER JOIN dbo.BAH_T_BD_MATWAREHOUSE T8 ON T0.FMATERIALID = T8.FID
                LEFT JOIN dbo.BAH_T_BD_MATWAREHOUSE_L T10 ON T8.FENTRYID = T10.FENTRYID
                INNER JOIN dbo.BAH_T_WMS_PICKUP T11 ON T.FID = T11.FID
                LEFT JOIN dbo.BAH_T_BD_LOCBASE T30 ON T6.FID = T30.FID
                LEFT JOIN dbo.BAH_T_WMS_OUTNOTICE T12 ON T5.FSOURCEBILLNO = T12.FBILLNO
                LEFT JOIN dbo.BAH_V_BD_CONTACT_L T13 ON T12.FCONTACTID = T13.FID 
                WHERE  t5.FJOINSTATUS = 'A'
	            AND t.FID = '{0}'
                AND ((T5.FJoinMQty < T.FTOMQTY AND T8.FENABLECAPACITY = 0) OR (T8.FENABLECAPACITY = 1 AND (T5.FJOINCTY < T.FTOCTY OR T5.FJOINCTY = 0)))
                AND T30.FUSE ='Transition'
                order by t.FTOTRACKNO
                 ;", dataObject["Fid"].ToString());// or a.num is null

                    DynamicObjectCollection mat_objc = DBUtils.ExecuteDynamicObject(ctx, sqlSelect, null, null);


                    //List<JSONObject> detail_list = new List<JSONObject>(); //明细信息
                    foreach (DynamicObject data in mat_objc)
                    {
                        JSONObject each_detail = new JSONObject();
                        IPackageService pkgService = null;
                        String FMQtyForShow;
                        String FHASMQTYForShow;
                        String FNeedMQTYForShow;
                        try
                        {
                            FormMetadata meta = MetaDataServiceHelper.Load(ctx, "BAH_BD_Package") as FormMetadata;
                            QueryBuilderParemeter queryParam = new QueryBuilderParemeter();
                            queryParam.FormId = "BAH_BD_Package";
                            queryParam.BusinessInfo = meta.BusinessInfo;
                            queryParam.FilterClauseWihtKey = " FID ='" + data["FTOPackageId"].ToString() + "' ";
                            var objs = BusinessDataServiceHelper.Load(ctx,
                                meta.BusinessInfo.GetDynamicObjectType(),
                                queryParam).FirstOrDefault();

                            pkgService = PIBDServiceFactory.Instance.GetService<IPackageService>(ctx);
                            var Marray = pkgService.Expand(ctx, objs, decimal.Parse(data["FTOMQty"].ToString()));
                            var Harray = Convert.ToDecimal(data["FHasJoinMQty"]) == 0 ? null : pkgService.Expand(ctx, objs, decimal.Parse(data["FHasJoinMQty"].ToString()));
                            var Narray = pkgService.Expand(ctx, objs, decimal.Parse(data["FNeedOUTBOUNDMQTY"].ToString()));
                            if (Narray.Count() == 1)
                            {
                                each_detail.Add("FNeedQTY", Narray.FirstOrDefault().Qty);
                                each_detail.Add("FNeedUNITID", Narray.FirstOrDefault().Id);
                                each_detail.Add("FNeedName", Narray.FirstOrDefault().Name.Value(2052));
                            }
                            else
                            {
                                each_detail.Add("FNeedQTY", data["FNeedOUTBOUNDMQTY"]);
                                each_detail.Add("FNeedUNITID", data["FTOMUNITID"]);
                                each_detail.Add("FNeedName", data["FMUnitName"]);
                            }
                            FMQtyForShow = string.Join("", Marray.Select(item => string.Concat(item.Qty.ToTrimEndZeroString(), item.Name.Value(ctx))).ToArray());
                            FHASMQTYForShow = Convert.ToDecimal(data["FHasJoinMQty"]) == 0 ? "0" + data["FMUnitName"] : string.Join("", Harray.Select(item => string.Concat(item.Qty.ToTrimEndZeroString(), item.Name.Value(ctx))).ToArray());
                            FNeedMQTYForShow = string.Join("", Narray.Select(item => string.Concat(item.Qty.ToTrimEndZeroString(), item.Name.Value(ctx))).ToArray());
                        }
                        finally
                        {
                            PIBDServiceFactory.Instance.CloseService(pkgService);
                        }
                        each_detail.Add("FBILLNO", data["FBILLNO"]);
                        each_detail.Add("FID", data["FID"]);
                        each_detail.Add("FENTRYID", data["FENTRYID"]);
                        each_detail.Add("FSEQ", data["FSEQ"]);
                        each_detail.Add("FLocId", data["FTOLocId"]);
                        each_detail.Add("FlocIdNumber", data["FlocIdNumber"]);
                        each_detail.Add("FlocIdName", data["FlocIdName"]);
                        each_detail.Add("FMaterialFID", data["FMaterialFID"]);
                        each_detail.Add("FMaterialNumber", data["FMaterialNumber"]);
                        each_detail.Add("FMaterialName", data["FMaterialName"]);
                        each_detail.Add("FSPECIFICATION", data["FSPECIFICATION"]);
                        each_detail.Add("FPackageId", data["FTOPackageId"]);
                        each_detail.Add("FPackageName", data["FPackageName"]);
                        each_detail.Add("FQty", data["FTOQty"]);
                        each_detail.Add("FUnitId", data["FTOUnitId"]);
                        each_detail.Add("FUnitName", data["FUnitName"]);
                        each_detail.Add("FMQty", data["FTOMQty"]);
                        each_detail.Add("FMUNITID", data["FTOMUNITID"]);
                        each_detail.Add("FMUnitName", data["FMUnitName"]);
                        each_detail.Add("FMQtyForShow", FMQtyForShow);
                        each_detail.Add("FHASMQTYForShow", FHASMQTYForShow);
                        each_detail.Add("FNeedMQTYForShow", FNeedMQTYForShow);
                        each_detail.Add("FNeedMQTY", data["FNeedOUTBOUNDMQTY"]);
                        each_detail.Add("FLotNo", data["FLotNo"]);
                        each_detail.Add("FTRACKNO", data["FTOTRACKNO"]);
                        if (data["FProduceDate"].ToString() == "0001-01-01 00:00:00")
                        {
                            each_detail.Add("FPRODUCEDATE", "");
                        }
                        else
                        {
                            each_detail.Add("FPRODUCEDATE", Convert.ToDateTime(data["FPRODUCEDATE"].ToString()).ToString("yyyy-MM-dd "));
                        }
                        each_detail.Add("FENABLELOT", data["FENABLELOT"]);
                        each_detail.Add("FENABLEEXPIRY", data["FENABLEEXPIRY"]);
                        each_detail.Add("FENABLECAPACITY", data["FENABLECAPACITY"]);
                        each_detail.Add("FCAPACITYUNIT", data["FCAPACITYUNIT"]);
                        each_detail.Add("FCAPACITYSCALE", data["FCAPACITYSCALE"]);
                        each_detail.Add("FAVGCTY", data["FTOAVGCTY"]);
                        each_detail.Add("FCTY", data["FTOCTY"]);
                        each_detail.Add("FHasJoinCty", data["FHasJoinCty"]);
                        each_detail.Add("FNeedCTY", data["FNeedCTY"]);
                        if (data["FEXPIRYDATE"].ToString() == "0001-01-01 00:00:00")
                        {
                            each_detail.Add("FEXPIRYDATE", "");
                        }
                        else
                        {
                            each_detail.Add("FEXPIRYDATE", Convert.ToDateTime(data["FEXPIRYDATE"].ToString()).ToString("yyyy-MM-dd "));
                        }
                        each_detail.Add("FSOURCEBILLNO", data["FSOURCEBILLNO"]);
                        each_detail.Add("FCONTACTNAME", data["FCONTACTNAME"]);
                        each_detail.Add("FPHMXWGT", data["FPHMXWGT"]);
                        return_data.Add(each_detail);
                    }
                }
                //客户编码和物料编码
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
            if (result.Message == "未将对象引用设置到对象的实例。")
            {
                result.Message = "未检索到对应信息！";
            }
            return result;
        }
    }
}
