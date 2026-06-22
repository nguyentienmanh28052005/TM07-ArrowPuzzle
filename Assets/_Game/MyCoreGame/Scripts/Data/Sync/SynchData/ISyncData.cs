using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISyncData 
{
    public abstract void OnSyncData(UserData newData);
    public abstract void SendSyncData(UserData dataUser);

}
