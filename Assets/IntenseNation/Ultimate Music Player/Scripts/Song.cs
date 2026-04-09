///////////////////////////////////////////////////////////////////
////   \\  //   ////                                           ////
////    \\//    ////     Created by Yousif Ragab               ////
////     ||     ////     ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬               ////
////     ||     ////                                           ////
///////////////////////////////////////////////////////////////////

using UnityEngine;

[System.Serializable]
public class Song
{
    public string SongName;         //Name of the song
    public string ArtistName;      //Name of the artist
	public Sprite SongImage;      //Song cover
    public AudioClip audioClip;  //Song Audio Clip
    public bool Loop;           //Should the song loop?

    [HideInInspector]
    public AudioSource audioSource; //AudioSource that plays the song, it's hidden because
                                    //it's changed automatically from MusicPlayer.cs
}
