using System.Collections.Generic;
namespace master
{
    public abstract class FactoryBase<TEnum, TInterface>
    {
        protected Dictionary<TEnum, TInterface> dic;
        public TInterface GetObj(TEnum type)
        {
            if (dic.ContainsKey(type))
            {
                return dic[type];
            }
            return default;
        }
    }
}

