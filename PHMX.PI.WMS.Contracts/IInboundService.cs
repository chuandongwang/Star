﻿using PHMX.PI.WMS.Core.Connector;
using Kingdee.BOS;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Rpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace PHMX.PI.WMS.Contracts
{
    /// <summary>
    /// 收货服务接口。
    /// </summary>
    [RpcServiceError, ServiceContract]
    public interface IInboundService
    {

    }
}
