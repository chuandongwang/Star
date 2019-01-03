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

namespace PHMX.PI.WMS.WebAPI.ServiceStub.OutGoing
{
    public class ReturnOutNoticeByMaterial : AbstractWebApiBusinessService
    {
        public ReturnOutNoticeByMaterial(KDServiceContext context) : base(context)
        {
        }
        /// <summary>
        /// 输入发货通知编号，输出按物料组合的发货通知详情（要加三个展示数量）
        /// </summary>
        /// <param name="billno">发货通知编号</param>
        /// <returns>返回服务结果。</returns>
        public ServiceResult ExecuteService(string whid, string areaid, string ownerid, string billno)
        {
            DynamicObjectCollection dataObjectCollection;
            var result = new ServiceResult<List<JSONObject>>();
            List<JSONObject> Final_data = new List<JSONObject>();
            var ctx = this.KDContext.Session.AppContext;
            // 检查上下文对象
            if (this.IsContextExpired(result)) return result;
            // 检查传入参数
            if (string.IsNullOrWhiteSpace(billno))
            {
                result.Code = (int)ResultCode.Fail;
                result.Message = "发货通知编号不能为空！";
                return result;
            }
            //获取相关信息 
            try
            {
                string sqlSelect1 = string.Format(@"
                      SELECT DISTINCT t0.FID fid, t0.FBILLNO fbillno, t5.FNAME fbatchwhid_fname,t7.FNAME fbatchownerid_fname, t9.FNAME fcontactid_fname ,
 t0.FMODIFYDATE,t0.FDIRECTION FDirectionForNotice
 
                FROM  dbo.BAH_T_WMS_OUTNOTICE t0
                LEFT JOIN dbo.BAH_T_WMS_OUTNOTICEENTRY t ON t0.FID = t.FID
	            LEFT JOIN dbo.BAH_T_BD_LOCATION_L t1 ON t.FLOCID = t1.FID
	            LEFT JOIN dbo.BAH_T_WMS_OUTNOTICEENTRY_W t2 ON t.FENTRYID = t2.FENTRYID
	            LEFT JOIN dbo.BAH_T_BD_LOCATION t3 ON t.FLOCID = t3.FID
	            LEFT JOIN dbo.BAH_T_BD_LOCBASE T30 ON T3.FID = T30.FID
	            LEFT JOIN dbo.BAH_T_BD_WAREHOUSE t4 on t0.FWHID = t4.FID
	            INNER JOIN dbo.BAH_T_BD_WAREHOUSE_L t5 on t4.FID = t5.FID
	            LEFT JOIN dbo.BAH_V_BD_OWNER t6 ON t0.FOWNERID = t6.FID
	            INNER JOIN dbo.BAH_V_BD_OWNER_L T7 ON T6.FID = T7.FID
	            LEFT JOIN dbo.BAH_V_BD_CONTACT T8 ON T0.FCONTACTID = T8.FID
	            INNER JOIN dbo.BAH_V_BD_CONTACT_L T9 ON T8.FID = T9.FID
                LEFT JOIN dbo.BAH_T_BD_MATERIAL t10 ON t.FMATERIALID = t10.FID
	            INNER JOIN dbo.BAH_T_BD_MATWAREHOUSE T11 ON T10.FID = T11.FID
                LEFT JOIN  dbo.BAH_T_WMS_PICKUPENTRY_W T12 ON t0.FBILLNO = t12.FSOURCEBILLNO AND T2.FENTRYID = T12.FSOURCEID
	            LEFT JOIN  dbo.BAH_T_WMS_PICKUPENTRY_B T13 ON T12.FENTRYID = t13.FENTRYID
                WHERE  T0.FWHID LIKE '%{0}%' AND T0.FOWNERID LIKE '%{2}%' AND T30.FAREAID LIKE '%{1}%' and (t0.FBILLNO = '{3}' or t2.FSOURCEBILLNO = '{3}' or t0.FORDERNO = '{3}' )
                AND t0.FDOCUMENTSTATUS = 'C'  AND T12.FJOINSTATUS = 'A'
                AND ((t12.FHASJOINMQTY < t13.FTOMQTY AND T11.FENABLECAPACITY = 0)  OR (T11.FENABLECAPACITY = 1 AND ( t12.FHASJOINCTY < t13.FTOCTY)) )                       
                 ;", whid, areaid, ownerid, billno);// or a.num is null

                //库区为零的情况
                string sqlSelect2 = string.Format(@"/*dialect*/
                     SELECT DISTINCT t0.FID fid, t0.FBILLNO fbillno, t5.FNAME fbatchwhid_fname,t7.FNAME fbatchownerid_fname, t9.FNAME fcontactid_fname ,
 t0.FMODIFYDATE,t0.FDIRECTION FDirectionForNotice
                FROM  dbo.BAH_T_WMS_OUTNOTICE t0
                LEFT JOIN dbo.BAH_T_WMS_OUTNOTICEENTRY t ON t0.FID = t.FID
	            LEFT JOIN dbo.BAH_T_BD_LOCATION_L t1 ON t.FLOCID = t1.FID
	            LEFT JOIN dbo.BAH_T_WMS_OUTNOTICEENTRY_W t2 ON t.FENTRYID = t2.FENTRYID
	            LEFT JOIN dbo.BAH_T_BD_LOCATION t3 ON t.FLOCID = t3.FID
	            LEFT JOIN dbo.BAH_T_BD_LOCBASE T30 ON T3.FID = T30.FID
	            LEFT JOIN dbo.BAH_T_BD_WAREHOUSE t4 on t0.FWHID = t4.FID
	            INNER JOIN dbo.BAH_T_BD_WAREHOUSE_L t5 on t4.FID = t5.FID
	            LEFT JOIN dbo.BAH_V_BD_OWNER t6 ON t0.FOWNERID = t6.FID
	            INNER JOIN dbo.BAH_V_BD_OWNER_L T7 ON T6.FID = T7.FID
	            LEFT JOIN dbo.BAH_V_BD_CONTACT T8 ON T0.FCONTACTID = T8.FID
	            INNER JOIN dbo.BAH_V_BD_CONTACT_L T9 ON T8.FID = T9.FID
                LEFT JOIN dbo.BAH_T_BD_MATERIAL t10 ON t.FMATERIALID = t10.FID
	            INNER JOIN dbo.BAH_T_BD_MATWAREHOUSE T11 ON T10.FID = T11.FID
                LEFT JOIN  dbo.BAH_T_WMS_PICKUPENTRY_W T12 ON t0.FBILLNO = t12.FSOURCEBILLNO AND T2.FENTRYID = T12.FSOURCEID
	            LEFT JOIN  dbo.BAH_T_WMS_PICKUPENTRY_B T13 ON T12.FENTRYID = t13.FENTRYID
                WHERE  T0.FWHID LIKE '%{0}%' AND T0.FOWNERID LIKE '%{2}%' and (t0.FBILLNO = '{3}' or t2.FSOURCEBILLNO = '{3}' or t0.FORDERNO = '{3}' )
                AND t0.FDOCUMENTSTATUS = 'C' AND T12.FJOINSTATUS = 'A'
                AND ((t12.FHASJOINMQTY < t13.FTOMQTY AND T11.FENABLECAPACITY = 0)  OR (T11.FENABLECAPACITY = 1 AND ( t12.FHASJOINCTY < t13.FTOCTY)) )                
                 ;", whid, areaid, ownerid, billno);// or a.num is null
                //TODO:通过平台动态引擎获取数据
                if (areaid == "")
                {
                    dataObjectCollection = DBUtils.ExecuteDynamicObject(ctx, sqlSelect2, null, null);
                }
                else
                {
                    dataObjectCollection = DBUtils.ExecuteDynamicObject(ctx, sqlSelect1, null, null);
                }
                //TODO:通过平台动态引擎获取数据
                foreach (DynamicObject dataObject in dataObjectCollection)
                {
                    JSONObject return_data = new JSONObject();
                    return_data.Add("Fid", dataObject["Fid"]);
                    return_data.Add("FBillNo", billno);
                    return_data.Add("FOutNoticeBillNo", dataObject["fbillno"]);
                    if (dataObject["FDirectionForNotice"].ToString() == "General")
                    {
                        return_data.Add("FDirection", "普通");
                    }
                    else
                    {
                        return_data.Add("FDirection", "退回");
                    }
                    return_data.Add("FModifyDate", Convert.ToDateTime(dataObject["FMODIFYDATE"].ToString()).ToString("yyyy-MM-dd HH:mm:ss:fff"));
                    return_data.Add("FBatchWHName", dataObject["FBatchWHId_FName"]);
                    return_data.Add("FBatchOwnerIdName", dataObject["FBatchOwnerId_FName"]);
                    return_data.Add("FContactIdName", dataObject["FContactId_FName"]);
                    //客户编码和物料编码
                    string sqlSelect = string.Format(@"/*dialect*/
               SELECT distinct t.FENTRYID, t.FSEQ, t.FLocId, t6.FNUMBER AS FlocIdNumber, t1.FNAME AS FlocIdName
	            ,t2.FID AS FMaterialFID, t2.FNUMBER AS FMaterialNumber, t3.FNAME AS FMaterialName, t3.FSPECIFICATION, t.FPackageId,T7.FNAME AS FPackageName, t.FQty
	            , t.FUnitId,t4.FNAME AS FUnitName, t.FMQty,T.FMUNITID,t9.FNAME AS FMUnitName,  t.FLotNo,
	            CASE T8.FENABLELOT WHEN 1 then 'True' ELSE 'False' END AS FENABLELOT ,
	            CASE T8.FENABLEEXPIRY WHEN 1 then 'True' ELSE 'False' END AS FENABLEEXPIRY,
                CASE T8.FENABLECAPACITY WHEN 1 then 'True' ELSE 'False' END AS FENABLECAPACITY,
	            t8.FCAPACITYSCALE,t10.FCAPACITYUNIT,
	            
	             V1.FHASOUTBOUNDMQTY,V1.FNEEDOUTBOUNDMQTY,
	             t8.FEXPPERIOD,t8.FEXPUNIT,
	             
	            t.FAVGCTY,t.FCty,t.FTRACKNO,
	            
                V1.FHASOUTBOUNDCTY,V1.FNEEDOUTBOUNDCTY,
                t.FPRODUCEDATE,
                V1.FPHMXWGT
                FROM dbo.BAH_T_WMS_OUTNOTICEENTRY t
	            LEFT JOIN dbo.BAH_T_BD_LOCATION_L t1 ON t.FLOCID = t1.FID
	            LEFT JOIN dbo.BAH_T_BD_MATERIAL t2 ON t.FMATERIALID = t2.FID
	            LEFT JOIN dbo.BAH_T_BD_MATERIAL_L t3 ON t.FMATERIALID = t3.FID
	            LEFT JOIN dbo.BAH_V_BD_UNIT_L t4 ON t.FUNITID = t4.fid
	            LEFT JOIN dbo.BAH_T_WMS_OUTNOTICEENTRY_W t5 ON t.FENTRYID = t5.FENTRYID
	            LEFT JOIN dbo.BAH_T_BD_LOCATION t6 ON t.FLOCID = t6.FID
	            LEFT JOIN dbo.BAH_T_BD_PACKAGE_L T7 ON T.FPACKAGEID = T7.FID
	            INNER JOIN dbo.BAH_T_BD_MATWAREHOUSE T8 ON T2.FID = T8.FID
	            LEFT JOIN dbo.BAH_V_BD_UNIT_L t9 ON t.FMUNITID = t9.fid
	            LEFT JOIN dbo.BAH_T_BD_MATWAREHOUSE_L T10 ON T8.FENTRYID = T10.FENTRYID
	            LEFT JOIN  dbo.BAH_T_WMS_PICKUPENTRY_W T12 ON  T.FENTRYID = T12.FSOURCEID
	            LEFT JOIN  dbo.BAH_T_WMS_PICKUPENTRY_B T13 ON T12.FENTRYID = t13.FENTRYID
	            LEFT JOIN 
	            (
	            select T1.FSOURCEID,
                SUM(T1.FHASJOINMQTY) AS FHASOUTBOUNDMQTY,
                SUM(T1.FHASJOINCTY) AS FHASOUTBOUNDCTY,
                SUM(T.FTOMQTY - T1.FHASJOINMQTY) AS FNEEDOUTBOUNDMQTY,
                SUM(T.FTOCTY - T1.FHASJOINCTY) AS FNEEDOUTBOUNDCTY,
                SUM(T4.FPHMXWGT) AS FPHMXWGT
                from dbo.BAH_T_WMS_PICKUP t0
                LEFT JOIN dbo.BAH_T_WMS_PICKUPENTRY_B t ON T0.FID = T.FID 
                LEFT JOIN dbo.BAH_T_WMS_PICKUPENTRY_W t1 on t.FENTRYID = t1.FENTRYID
                LEFT JOIN dbo.BAH_T_WMS_PICKUPENTRY T2 ON T.FENTRYID = T2.FENTRYID
                LEFT JOIN dbo.BAH_T_BD_MATWAREHOUSE T3 ON T2.FMATERIALID = T3.FID
                LEFT JOIN dbo.BAH_T_WMS_PICKUPENTRY T4 ON T0.FID = T4.FID
                WHERE T0.FDOCUMENTSTATUS =  'C' AND T1.FJOINSTATUS = 'A' 
                AND ((t1.FHASJOINMQTY < t.FTOMQTY AND T3.FENABLECAPACITY = 0)  OR (T3.FENABLECAPACITY = 1 AND ( t1.FHASJOINCTY < t.FTOCTY)) )  
                GROUP BY T1.FSOURCEID  
               
	            ) V1 ON T.FENTRYID = V1.FSOURCEID
                WHERE  T12.FJOINSTATUS = 'A'
	            AND t.FID = '{0}'
                AND ((t12.FHASJOINMQTY < t13.FTOMQTY AND T8.FENABLECAPACITY = 0)  OR (T8.FENABLECAPACITY = 1 AND ( t12.FHASJOINCTY < t13.FTOCTY)) )                       
                order by t.FSEQ 
                 ;", dataObject["Fid"].ToString());// or a.num is null

                    DynamicObjectCollection mat_objc = DBUtils.ExecuteDynamicObject(ctx, sqlSelect, null, null);
                    if (mat_objc.Count == 0)
                    {
                        result.Code = (int)ResultCode.Fail;
                        result.Message = "未检索到对应信息！";
                    }
                    else
                    {
                        List<JSONObject> detail_list = new List<JSONObject>(); //明细信息
                        foreach (DynamicObject data in mat_objc)
                        {
                            JSONObject each_detail = new JSONObject();
                            IPackageService pkgService = null;
                            String FMQtyForShow;
                            String FHASOUTBOUNDMQTYForShow;
                            String FNeedOUTBOUNDMQTYForShow;
                            try
                            {
                                FormMetadata meta = MetaDataServiceHelper.Load(ctx, "BAH_BD_Package") as FormMetadata;
                                QueryBuilderParemeter queryParam = new QueryBuilderParemeter();
                                queryParam.FormId = "BAH_BD_Package";
                                queryParam.BusinessInfo = meta.BusinessInfo;
                                queryParam.FilterClauseWihtKey = " FID ='" + data["FPackageId"].ToString() + "' ";
                                var objs = BusinessDataServiceHelper.Load(ctx,
                                    meta.BusinessInfo.GetDynamicObjectType(),
                                    queryParam).FirstOrDefault();
                                pkgService = PIBDServiceFactory.Instance.GetService<IPackageService>(ctx);
                                var Marray = pkgService.Expand(ctx, objs, decimal.Parse(data["FMQty"].ToString()));
                                var Harray = Convert.ToDecimal(data["FHASOUTBOUNDMQTY"]) == 0 ? null : pkgService.Expand(ctx, objs, decimal.Parse(data["FHASOUTBOUNDMQTY"].ToString()));
                                var Narray = pkgService.Expand(ctx, objs, decimal.Parse(data["FNeedOUTBOUNDMQTY"].ToString()));
                                FMQtyForShow = string.Join("", Marray.Select(item => string.Concat(item.Qty.ToTrimEndZeroString(), item.Name.Value(ctx))).ToArray());
                                FHASOUTBOUNDMQTYForShow = Convert.ToDecimal(data["FHASOUTBOUNDMQTY"]) == 0 ? "0" + data["FMUnitName"] : string.Join("", Harray.Select(item => string.Concat(item.Qty.ToTrimEndZeroString(), item.Name.Value(ctx))).ToArray());
                                FNeedOUTBOUNDMQTYForShow = string.Join("", Narray.Select(item => string.Concat(item.Qty.ToTrimEndZeroString(), item.Name.Value(ctx))).ToArray());
                                if (Narray.Count() == 1)
                                {
                                    each_detail.Add("FNeedQTY", Narray.FirstOrDefault().Qty);
                                    each_detail.Add("FNeedUNITID", Narray.FirstOrDefault().Id);
                                    each_detail.Add("FNeedName", Narray.FirstOrDefault().Name.Value(2052));
                                }
                                else
                                {
                                    each_detail.Add("FNeedQTY", data["FNeedOUTBOUNDMQTY"]);
                                    each_detail.Add("FNeedUNITID", data["FMUNITID"]);
                                    each_detail.Add("FNeedName", data["FMUnitName"]);
                                }
                            }
                            finally
                            {
                                PIBDServiceFactory.Instance.CloseService(pkgService);
                            }

                            each_detail.Add("FENTRYID", data["FENTRYID"]);
                            each_detail.Add("FSEQ", data["FSEQ"]);
                            each_detail.Add("FLocId", data["FLocId"]);
                            each_detail.Add("FlocIdNumber", data["FlocIdNumber"]);
                            each_detail.Add("FlocIdName", data["FlocIdName"]);
                            each_detail.Add("FMaterialFID", data["FMaterialFID"]);
                            each_detail.Add("FMaterialNumber", data["FMaterialNumber"]);
                            each_detail.Add("FMaterialName", data["FMaterialName"]);
                            each_detail.Add("FSPECIFICATION", data["FSPECIFICATION"]);
                            each_detail.Add("FPackageId", data["FPackageId"]);
                            each_detail.Add("FPackageName", data["FPackageName"]);
                            each_detail.Add("FQty", data["FQty"]);
                            each_detail.Add("FUnitId", data["FUnitId"]);
                            each_detail.Add("FUnitName", data["FUnitName"]);
                            each_detail.Add("FMQty", data["FMQty"]);
                            each_detail.Add("FMUNITID", data["FMUNITID"]);
                            each_detail.Add("FMUnitName", data["FMUnitName"]);
                            each_detail.Add("FMQtyForShow", FMQtyForShow);
                            each_detail.Add("FHASOUTBOUNDMQTYForShow", FHASOUTBOUNDMQTYForShow);
                            each_detail.Add("FNeedOUTBOUNDMQTYForShow", FNeedOUTBOUNDMQTYForShow);
                            each_detail.Add("FNeedOUTBOUNDMQTY", data["FNEEDOUTBOUNDMQTY"]);
                            each_detail.Add("FLotNo", data["FLotNo"].ToString().Trim());
                            each_detail.Add("FENABLELOT", data["FENABLELOT"]);
                            each_detail.Add("FENABLEEXPIRY", data["FENABLEEXPIRY"]);
                            each_detail.Add("FENABLECAPACITY", data["FENABLECAPACITY"]);
                            each_detail.Add("FCAPACITYUNIT", data["FCAPACITYUNIT"]);
                            each_detail.Add("FCAPACITYSCALE", data["FCAPACITYSCALE"]);
                            each_detail.Add("FAVGCTY", data["FAVGCTY"]);
                            each_detail.Add("FCTY", data["FCTY"]);
                            each_detail.Add("FEXPPERIOD", data["FEXPPERIOD"]);
                            each_detail.Add("FEXPUNIT", data["FEXPUNIT"]);
                            each_detail.Add("FTRACKNO", data["FTRACKNO"]);
                            each_detail.Add("FHASOUTBOUNDCTY", data["FHASOUTBOUNDCTY"]);
                            each_detail.Add("FNeedOUTBOUNDCTY", data["FNeedOUTBOUNDCTY"]);
                            if (data["FProduceDate"].ToString() == "0001-01-01 00:00:00")
                            {
                                each_detail.Add("FPRODUCEDATE", "");
                            }
                            else
                            {
                                each_detail.Add("FPRODUCEDATE", Convert.ToDateTime(data["FPRODUCEDATE"].ToString()).ToString("yyyy-MM-dd HH:mm:ss:fff"));
                            }
                            each_detail.Add("FPHMXWGT", data["FPHMXWGT"]);
                            each_detail.Add("InventeryForAdvise", ReturnInventery(whid, areaid, ownerid, data["FMaterialFID"].ToString()));


                            detail_list.Add(each_detail);
                        }
                        return_data.Add("DetailList", detail_list);
                    }
                    //返回数据
                    Final_data.Add(return_data);
                }

                result.Code = (int)ResultCode.Success;
                result.Data = Final_data;
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
        public List<JSONObject> ReturnInventery(string whid, string areaid, string ownerid, string materialid)
        {
            var ctx = this.KDContext.Session.AppContext;
            List<JSONObject> InventeryList = new List<JSONObject>();
            string sqlSelect = string.Format(@"/*dialect*/
                     SELECT TOP 3 VV.* FROM (SELECT  DISTINCT V.* FROM (
 SELECT      TOP 100 PERCENT   FLOCID,t6.FNUMBER AS FLocNumber,t5.FNAME AS FLocName
                from dbo.BAH_V_WMS_INVENTORY t
	            LEFT JOIN dbo.BAH_T_BD_LOCATION_L t5 ON t.FLOCID = t5.FID
	            LEFT JOIN dbo.BAH_T_BD_LOCATION t6 ON t.FLOCID = t6.FID 
	            LEFT JOIN dbo.BAH_T_BD_LOCBASE T9 ON t.FLOCID = T9.FID
                LEFT JOIN dbo.BAH_T_BD_LOCCONTROL T10 ON T.FLOCID = T10.FID
	            where t.FMATERIALID LIKE '%{3}%' 
                and t.FOWNERID LIKE '%{2}%'
                and t.FWHID LIKE '%{0}%' 
                and  t.fareaid like'%{1}%' and  (t9.FUSE = 'Storage' or t9.FUSE = 'Pick')
                AND T10.FAllowPickAssign = 1                                                     
                order by t9.FUSE,t.FLOCID,t.FLOTNO  ) AS V)VV          
                 ;", whid, areaid, ownerid, materialid);
            DynamicObjectCollection Inventery_objc = DBUtils.ExecuteDynamicObject(ctx, sqlSelect, null, null);
            foreach (DynamicObject data in Inventery_objc)
            {
                JSONObject each_detail = new JSONObject();
                each_detail.Add("FLOCIDForAdvise", data["FLOCID"]);
                each_detail.Add("FLocNumberForAdvise", data["FLocNumber"]);
                each_detail.Add("FLocNameForAdvise", data["FLocName"]);
                //each_detail.Add("FMQTYForAdvise", data["FMQTY"]);
                //each_detail.Add("FMainUnitId", data["FMainUnitId"]);
                //each_detail.Add("FMainUnitName", data["FMainUnitName"]);
                //each_detail.Add("FAVGCTYForAdvise", data["FAVGCTY"]);
                //each_detail.Add("FCTYForAdvise", data["FCTY"]);
                InventeryList.Add(each_detail);
            }
            return InventeryList;
        }
    }
}
