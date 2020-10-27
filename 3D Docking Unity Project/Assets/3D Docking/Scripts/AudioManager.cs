using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{

    public static AudioManager Instance;
    //public float lowPitch = 0.6f;
    //public float midPitch = 0.8f;
    //public float highPitch = 1f;

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

        //if (i == 0)
        //    source.pitch = lowPitch;
        //else if (i == 1)
        //    source.pitch = midPitch;
        //else
        //    source.pitch = highPitch;

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
