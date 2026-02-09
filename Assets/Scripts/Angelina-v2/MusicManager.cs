using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MusicManager : MonoBehaviour
{
    public AudioSource audioSource;
    public List<AudioClip> audioClips;
    public TextMeshProUGUI trackNameText;

    public void PlayTrack(int index)
    {
        if (index >= 0 && index < audioClips.Count)
        {
            audioSource.clip = audioClips[index];
            audioSource.Play();
            if (trackNameText != null)
            {
                trackNameText.text = "Now Playing: " + audioClips[index].name;
            }
        }
    }

    public void StopTrack()
    {
        audioSource.Stop();
        if (trackNameText != null)
        {
            trackNameText.text = "Stopped";
        }
    }
}
