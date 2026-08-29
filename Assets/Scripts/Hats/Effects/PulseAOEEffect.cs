using UnityEngine;

[System.Serializable]
public class PulseAOEEffect : HatEffect
{
    public GameObject pulsePrefab;
    public float radius = 5f;
    public float damage = 10f;

    public override void Execute(HatInstance hat, GameObject target)
    {
        GameObject pulse = Object.Instantiate(pulsePrefab, hat.transform.position, Quaternion.identity);
        // Setup pulse scale and damage logic on the spawned object script
    }
}
