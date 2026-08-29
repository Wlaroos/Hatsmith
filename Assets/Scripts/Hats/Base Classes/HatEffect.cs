using UnityEngine;
[System.Serializable]
public abstract class HatEffect
{
    public abstract void Execute(HatInstance hat, GameObject target = null);
}
