///////////////////////////////////////////////////////////////////
////   \\  //   ////                                           ////
////    \\//    ////     Created by Yousif Ragab               ////
////     ||     ////     ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬               ////
////     ||     ////                                           ////
///////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEngine.UI;

public class AdvancedMusicPlayer : MonoBehaviour
{
    public MusicPlayer musicPlayer; //Reference for the MusicPlayer script

    [Header("User Interface")]
    public Image PlayPauseImage; //Play or Pause Button's image
    public Sprite PlaySprite;    //Play sprite
    public Sprite PauseSprite;   //Pause sprite

    public Image ShuffleImage;   //Shuffle button's image

    [Header("Colors")]
    public Color ShuffleOffColor = Color.white; //The color to use when Shuffle is off
    public Color ShuffleOnColor = Color.green;  //The color to use when Shuffle is on

    private void Start()
    {
        UpdateShuffleButton();   //Update the Shuffle Button Design
        UpdatePlayPauseButton(); //Update the Play/Pause Button Design
    }

    public void PlayPause() //Called from Play/Pause button
    {
        musicPlayer.PausePlayMusic(); //Play or Pause music using the Music Player script
        UpdatePlayPauseButton(); //Update the design of the Play/Pause button
    }
    
    public void Shuffle() //Called from the Shuffle button
    {
        musicPlayer.ShuffleMusic(); //Enable or disable Shuffle using the Music Plyer script
        UpdateShuffleButton(); //Update the design of the Shuffle button
    }
    
    void UpdatePlayPauseButton()
    {
        if (musicPlayer.audioSource.isPlaying) //If the audio source is playing music
            PlayPauseImage.sprite = PauseSprite; //Then set the Play/Pause button's image to the Pause sprite
        else //If it's not playing music
            PlayPauseImage.sprite = PlaySprite; //Then set the Play/Pause button's image to the Play sprite
    }

    void UpdateShuffleButton()
    {
        if (musicPlayer.Shuffle) //If the Shuffle option is turned on
            ShuffleImage.color = ShuffleOnColor; //Then set the Shuffle button's color to the Shuffle On Color
        else //If it's turned on
            ShuffleImage.color = ShuffleOffColor; //Then set the Shuffle button's color to the Shuffle Off Color
    }
}