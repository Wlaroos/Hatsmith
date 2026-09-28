using UnityEngine;

public class SetParticleParent : MonoBehaviour
{
    private void Start()
    {
        if (LevelGenerator2D.Instance != null)
        {
            transform.SetParent(LevelGenerator2D.Instance.ParticleParent);
        }
    }
}
