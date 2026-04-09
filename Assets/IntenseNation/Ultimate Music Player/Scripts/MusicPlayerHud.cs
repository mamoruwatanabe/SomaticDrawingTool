///////////////////////////////////////////////////////////////////
////   \\  //   ////                                           ////
////    \\//    ////     Created by Yousif Ragab               ////
////     ||     ////     ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬               ////
////     ||     ////                                           ////
///////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MusicPlayerHud : MonoBehaviour
{
	public TextMeshProUGUI SongName;        //Name of the song Text
	public TextMeshProUGUI ArtistName;      //Name of the artist Text
	public Image image;                     //Song cover Image
	public GameObject hud;                  //The main layout for the music player
	public Animator animator;               //The main Animator that includes all music player hud animations
	public Slider slider;                   //The song slider (To preview/change the song position)
	public bool AnimateOnStart = false;     //Should the animation start once the game starts?
	public string AnimationName = "Normal"; //Name of the animation that will play)
	public MusicPlayer musicPlayer;         //Reference for the MusicPlayer

	private bool firstTimeAnimating = true; //Is this the first time playing any animation? (Resets everytime the game is closed)
	private bool canChangeSlider = true;    //State for the slider if it's changeable or not

	//ChangeSong is called to play a specific song
	public void ChangeSong(Sprite Sprite, string songName, string artistName)
	{
		if (image)                       //Check if the image is assigned and not null
			image.sprite = Sprite;          //We need the Song Cover Sprite and assign it to the HUD Image

		if (SongName)                    //Check if the SongName is assigned and not null
			SongName.text = songName;       //Assign the Song Name to the HUD SongName Text 

		if (ArtistName)                  //Check if the ArtistName is assigned and not null
			ArtistName.text = artistName;   //As well as the Artist Name and assign it to the ArtistName HUD Text

		//if (slider)
		//	slider.maxValue = musicPlayer.audioSource.clip.length;

		DisplayNotification();         //The last thing is to display the notification containing these data
	}

    private void LateUpdate()
    {
		if (slider && canChangeSlider && musicPlayer.audioSource.isPlaying) //If there's a slider assigned and it's value can be changed and if there's a clip that's being played
			slider.value = musicPlayer.audioSource.time / musicPlayer.audioSource.clip.length; //Set the slider's value to the current song time, we divide by the song length to clamp it between 0 and 1
	}

	public void ChangeSliderState(bool state) //Change the Slider State
    {
		canChangeSlider = state; //Set the canChangeSlider to the new state
	}

    //DisplayNotification plays an animation and displays the selected song data
    void DisplayNotification()
	{
		//Should the notification be displayed once the game starts?
		if (!AnimateOnStart) //If NO then this happens
		{
			if (firstTimeAnimating) //If this the first time animating?
			{
				firstTimeAnimating = false; //If yes then set the firstTimeAnimating to No and don't continue checking the method
				return;
			}
		}

		if (AnimateOnStart || !firstTimeAnimating) //If YES or this is not the first time animating then do this
		{
			if (animator && AnimationName != "") //Check if there's an animator assigned and that the Animation Name is not empty
				animator.Play(AnimationName);  //Play the animation using the name of AnimationName string
		}
	}
}