using UnityEngine;

public class SetParticleParent : MonoBehaviour
{
    private void OnEnable()
    {
        GameManager.Instance.RoomChangeEvent += OnRoomChange;
    }

    private void OnDisable()
    {
        GameManager.Instance.RoomChangeEvent -= OnRoomChange;
    }

    private void OnRoomChange()
    {
        Destroy(gameObject);
    }
}
