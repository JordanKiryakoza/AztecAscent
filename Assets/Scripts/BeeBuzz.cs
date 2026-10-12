using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BeeBuzz : MonoBehaviour
{
    public AudioClip buzz;

    void Start()
    {
        var src = GetComponent<AudioSource>();
        src.clip = buzz;   // load the sound into the speaker
        src.loop = true;   // repeat forever
        src.Play();        // start it
    }
}