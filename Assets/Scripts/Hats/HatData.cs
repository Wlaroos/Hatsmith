using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewHatData", menuName = "Hat Data")]
public class HatData : ScriptableObject
{
    public string hatName;
    public GameObject hatPrefab;
    public float stackHeightOffset = 0.3f; // Height added to the stack when equipped

    [SerializeReference]
    public List<HatEffectRule> rules = new List<HatEffectRule>();
}

[Serializable]
public class HatEffectRule
{
    [SerializeReference] public HatTrigger trigger;
    [SerializeReference] public HatEffect effect;
}

