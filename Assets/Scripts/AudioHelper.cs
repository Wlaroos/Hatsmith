using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AudioHelper
{
    public static AudioSource PlayClip3D(AudioClip clip, float volume, Vector3 position, float minDistance, float maxDistance)
    {
        GameObject audioObject = new GameObject("3DAudio");
        AudioSource audioSource = audioObject.AddComponent<AudioSource>();

        audioSource.clip = clip;
        audioSource.volume = volume;

        audioObject.transform.position = position;
        audioSource.spatialBlend = 1;

        minDistance = Mathf.Clamp(minDistance, 1, maxDistance);
        audioSource.minDistance = minDistance;
        audioSource.maxDistance = maxDistance;

        audioSource.Play();

        Object.Destroy(audioObject, clip.length);

        return audioSource;
    }

    public static AudioSource PlayClip2D(AudioClip clip, float volume)
    {
        GameObject audioObject = new GameObject("2DAudio");
        AudioSource audioSource = audioObject.AddComponent<AudioSource>();

        audioSource.clip = clip;
        audioSource.volume = volume;

        if (audioSource.clip.name == "Exploration_01")
        {
            audioSource.loop = true;
        }

        audioSource.Play();

        if (audioSource.clip.name != "Exploration_01")
        {
            Object.Destroy(audioObject, clip.length);
        }

        return audioSource;
    }
}
