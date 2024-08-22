using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRegisterUI 
{
    public void Open();
    public void Close();

    public void Register();
    public string GetID();
}
