using master;
using System;
using System.Collections.Generic;


public class FactoryProcessDataServer : FactoryBase<string, IProcessDataServer>
{
    public Dictionary<string, IProcessDataServer> DictionaryProcess => dic;
    public FactoryProcessDataServer()
    {
        dic = new Dictionary<string, IProcessDataServer>();
        var abilityTypes = MasterHelper.GetAllTypesThatImplement<IProcessDataServer>();
        foreach (var type in abilityTypes)
        {
            var t = Activator.CreateInstance(type) as IProcessDataServer;
            var actionName = t.GetActionName();
            if (t != null && !dic.ContainsKey(actionName))
            {
                dic.Add(actionName, t);
            }
        }
    }
}
