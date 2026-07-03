using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    Coroutine _shakeRoutine = null;
    Vector3 _originalPos = new Vector3(0,0,0);

    public static CameraShake Instance { get; private set; }

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public void Shake(float duration, float magnitude)
    {
        if(_shakeRoutine != null)
        {
            transform.localPosition = _originalPos;
            StopCoroutine(_shakeRoutine);
        }
        StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        _originalPos = transform.localPosition;

        float elapsedTime = 0;

        while(elapsedTime < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(x, y, _originalPos.z);

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        transform.localPosition = _originalPos;
    }
}

