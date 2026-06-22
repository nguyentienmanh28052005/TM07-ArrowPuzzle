using master;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IProcessDataServer
{
    public string GetActionName();
    public void Process(DataServerReceived data);   
}
