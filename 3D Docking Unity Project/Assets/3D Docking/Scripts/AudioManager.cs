using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{

    public static AudioManager Instance;
    const float kLowPitch = 0.5f;
    const float kMidPitch = 0.75f;
    const float kHighPitch = 1f;

    /// <summary>
    /// 0-2 task, 3 all done, 4 fail
    /// </summary>
    public AudioClip[] sounds;
    AudioSource source;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public void PlaySound(int i)
    {
        if (i >= sounds.Length)
            return;

        if (i == 0)
            source.pitch = kLowPitch;
        else if (i == 1)
            source.pitch = kMidPitch;
        else
            source.pitch = kHighPitch;

        source.PlayOneShot(sounds[i]);

    }


    // Start is called before the first frame update
    void Start()
    {
        source = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
