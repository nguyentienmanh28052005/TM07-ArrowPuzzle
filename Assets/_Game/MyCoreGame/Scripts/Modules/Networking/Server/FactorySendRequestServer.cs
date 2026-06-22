using master;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactorySendRequestServer : FactoryBase<string, ISendRequestServer>
{
    public Dictionary<string, ISendRequestServer> Dictionary => dic;
    public FactorySendRequestServer()
    {
        dic = new Dictionary<string, ISendRequestServer>();
        var abilityTypes = MasterHelper.GetAllTypesThatImplement<ISendRequestServer>();
        foreach (var type in abilityTypes)
        {
            var t = Activator.CreateInstance(type) as ISendRequestServer;
            var actionName = type.Name;
            if (t != null && !dic.ContainsKey(actionName))
            {
                dic.Add(actionName, t);
            }
        }
    }
}
