using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Orm.DataEntity;

namespace PHMX.PI.WMS.Business.PlugIn
{
    [System.ComponentModel.Description("收货通知、发货通知保存后自动刷新当前界面")]

    public class RefreshStatusAfterAudit : AbstractBillPlugIn
    {
        public override void AfterBarItemClick(AfterBarItemClickEventArgs e)
        {
            base.AfterBarItemClick(e);
            /*因为上游单据下推收货通知、发货通知，生成的单据仅处于创建状态，未能自动提交及审核，后来在下游单据的保存操作列表中配了服务插件，实现了自动提交和审核;
            手工新增的收货、发货通知，因配置了服务插件，所以在点击保存后，也自动提交和审核了，而当前界面在保存后，审核状态等未做更新，导致操作人员不清楚当前单据状态，
            提交及审核按钮状态为激活状态，显示状态不对，所以增加了这个表单插件，保存后，自动刷新当前单据
            */
            if (e.BarItemKey == "tbSplitSave" || e.BarItemKey == "tbSave")
            {
                this.View.Refresh();               
                //this.View.Model.SetValue("FRemark", ((DynamicObject)this.View.Model.GetValue("FBillTypeId"))["Name"].ToString()) ;               

                if (((DynamicObject)this.View.Model.GetValue("FBillTypeId"))["Name"].ToString() != "简单生产入库-收货")
                    return;
                else
                {
                    //Entity entity = this.View.BusinessInfo.GetEntity("FEntity");
                    int iRowCount = this.View.Model.GetEntryRowCount("FEntity");

                    String EnableCapacity;

                    for (int iRowIndex = 0; iRowIndex < iRowCount; iRowIndex++)
                    {
                        DynamicObject FMaterialId = this.View.Model.GetValue("FMaterialId", iRowIndex) as DynamicObject;

                        DynamicObjectCollection WarehouseSub = FMaterialId["WarehouseSub"] as DynamicObjectCollection;

                        EnableCapacity = WarehouseSub.FirstOrDefault()["EnableCapacity"].ToString();

                        //生成条码信息 %单据编号%物料%跟踪号%数量%
                        String sCodeInfo = "%" + this.View.Model.GetValue("FBillNo", iRowIndex).ToString() + "%" + ((DynamicObject)this.View.Model.GetValue("FMaterialId", iRowIndex))["Number"].ToString() + " %" + this.View.Model.GetValue("FTrackNo", iRowIndex).ToString() + "%";

                        if (EnableCapacity == "False")
                        {
                            //sCodeInfo += this.View.Model.GetValue("FMQty", iRowIndex).ToString() + "%";
                            sCodeInfo += string.Format("{0:######}", double.Parse(this.View.Model.GetValue("FMQty", iRowIndex).ToString())) + "%";
                        }
                        else
                        {
                            //sCodeInfo += this.View.Model.GetValue("FCty", iRowIndex).ToString() + "%"; 
                            sCodeInfo += string.Format("{0:######}", double.Parse(this.View.Model.GetValue("FCty", iRowIndex).ToString())) + "%";
                        }


                        this.View.Model.SetValue("FPHMXCodeInfo", sCodeInfo, iRowIndex);

                    }
                    this.View.UpdateView();
                    this.View.InvokeFormOperation(FormOperationEnum.Save);
                }
            }
        }
    }
}
