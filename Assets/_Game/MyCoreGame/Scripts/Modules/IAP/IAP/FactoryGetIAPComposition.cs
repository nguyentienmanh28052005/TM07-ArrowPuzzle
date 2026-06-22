using master;
using System;
using System.Collections.Generic;

public class FactoryGetIAPComposition : FactoryBase<string, IIAPComponent>
{
    public FactoryGetIAPComposition(IAPManager iAPManager)
    {
        dic = new Dictionary<string, IIAPComponent>();
        var abilityTypes = MasterHelper.GetAllTypesThatImplement<IIAPComponent>();
        foreach (var type in abilityTypes)
        {
            var t = Activator.CreateInstance(type) as IIAPComponent;

            if (t != null && !dic.ContainsKey(t.GetType().Name))
            {
                t.Initialized(iAPManager);
                dic.Add(t.GetType().Name, t);
            }
        }
    }
}
