using UnityEngine.Audio;
using System;
using UnityEngine;
//using Photon.Pun;

public class SoundManager : MonoBehaviour
{
    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)]
        public float volume;
        [Range(0.1f, 3f)]
        public float pitch;
        public bool loop;
        [Header("3D Settings")]
        public float minDistance = 15f;
        public float maxDistance = 25f;
        [HideInInspector]
        public AudioSource source;
    }

    public SoundGroup[] soundGroups;
    public GameObject soundInstance;
    public static SoundManager instance;
    //public PhotonView view;

    private void Awake()
    {
        instance = this;
//        if (instance == null)
//        {
//            instance = this;
//        }
//        else
//        {
//            Destroy(gameObject);
//        }

        //DontDestroyOnLoad(gameObject);

        // ^ Singleton stuff

        foreach(SoundGroup sg in soundGroups)
        {
            foreach (Sound s in sg.sounds)
            {
                s.source = gameObject.AddComponent<AudioSource>();
                s.source.clip = s.clip;
                s.source.volume = s.volume;
                s.source.pitch = s.pitch;
                s.source.loop = s.loop;
            }
        }
    }

    /// <summary>
    /// Plays a 2D sound
    /// </summary>
    /// <param name="name">Name of the sound</param>
    //[PunRPC]
    public void Play(string name)
    {
        SoundGroup sg = null;
        Sound s = null;
        foreach (SoundGroup group in soundGroups)
        {
            s = Array.Find(group.sounds, sound => sound.name == name);
            if (s != null)
            {
                sg = group;
                break;
            }
        }
        if (s == null || sg == null)
        {
            Debug.LogWarning("Sound: " + s.name + " was not found!");
            return;
        }
        s.source.volume = s.volume * sg.volumeMultiplier;

        s.source.Play();
    }

    /// <summary>
    /// Plays a 3D sound
    /// </summary>
    /// <param name="name">Name of the sound</param>
    /// <param name="position">Position of the sound</param>
    //[PunRPC]
    public void Play3D(string name, Vector3 position, bool global = true)
    {
        SoundGroup sg = null;
        Sound sound = null;
        bool foundSound = false;
        foreach (SoundGroup group in soundGroups)
        {
            sound = Array.Find(group.sounds, sound => sound.name == name);
            if (sound != null)
            {
                sg = group;
                foundSound = true;
                break;
            }
        }
        if (!foundSound)
        {
            Debug.LogWarning("Sound: " + name + " was not found!");
            return;
        }

        AudioSource source = Instantiate(soundInstance, position, Quaternion.identity).GetComponent<AudioSource>();
        source.clip = sound.clip;
        source.volume = sound.volume * sg.volumeMultiplier;
        source.pitch = sound.pitch;
        source.maxDistance = sound.maxDistance;
        source.minDistance = sound.minDistance;
        source.spatialBlend = 1f;

        if (global)
        {
            //view.RPC("Play3D", RpcTarget.Others, name, position, false);
        }
    }
}
