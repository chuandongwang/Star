using BAH.BOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PHMX.PI.WMS.Contracts
{
    /// <summary>
    /// 获取服务实例。
    /// </summary>
    public class PHMSWMSServiceFactory : AbstractServiceFactory
    {
        public static readonly PHMSWMSServiceFactory instance = new PHMSWMSServiceFactory();

        public static PHMSWMSServiceFactory Instance
        {
            get
            {
                return instance;
            }
        }

        public override void RegisterService()
        {
            this.MapServer.Add(typeof(IConnectorService), "PHMX.PI.WMS.App.Core.ConnectorService,PHMX.PI.WMS.App.Core");
            this.MapServer.Add(typeof(IInboundService), "PHMX.PI.WMS.App.Core.InboundService,PHMX.PI.WMS.App.Core");
        }
    }//end class
}
