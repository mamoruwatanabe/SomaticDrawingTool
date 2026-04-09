///////////////////////////////////////////////////////////////////
////   \\  //   ////                                           ////
////    \\//    ////     Created by Yousif Ragab               ////
////     ||     ////     ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬               ////
////     ||     ////                                           ////
///////////////////////////////////////////////////////////////////

using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public Song[] Songs;                        //All the songs to play
    public static MusicPlayer musicPlayer;      //static Music Player (ensures only one prefab is in scene)

    //These strings are used to hold data about song and artist, used by MusicPlayerHud
    [HideInInspector]
    public string SongName;                     //Name of the song (Hidden because it's automatically changed)
    [HideInInspector]
    public string ArtistName;
    //End of the cached data

    public string ArtistPrefix = "Produced By"; //This is added before the artist name is shown in the notification

    public AudioSource audioSource;            //The Audio Source required to play songs
    private int CurrentSongNumber;              //The number of the currently playing song
    private int LatestSongNumber;               //The number of the previously played song

    public bool Shuffle;                        //Option to shuffle on/off
    public bool EnableButtons;                  //Should the user be able to control the music player from shortcut buttons?
    public bool EnableNotifications = true;     //Should the code send a notification about the song?
    public MusicPlayerHud musicPlayerHud;       //Required to send the notification

    private bool isPlaying = true;              //Local bool that holds the state of music playing/paused

    private void Awake()
    {
        //This ensures only one prefab is in scene
        if (musicPlayer == null) //Is there's no musicPlayer in the scene
        {
            musicPlayer = this;  //Then assign this prefab to the musicPlayer
        }
        else                     //If there's already a musicPlayer in this scene (along this one)
        {
            Destroy(gameObject); //Then we have to destroy this gameobject since there's already a musicPlayer
        }


        DontDestroyOnLoad(gameObject); //We use this method so that the music player is not destroyed when we change scenes
                                       //The music shouldn't stop even if we change scenes

        LatestSongNumber = -1;         //Set the number of the last played song to -1, so that the code will set it automatically afterwards

        if (audioSource == null)       //Check if the audio source is not assigned
        {
            if (GetComponent<AudioSource>() == null)  //Double check that we don't have an AudioSource in the gameobject
            {
                audioSource = gameObject.AddComponent<AudioSource>(); //If so then add a new AudioSource and assign it to audioSource
            }
            else //We could already have an AudioSource in gameobject but not assigned
            {
                audioSource = GetComponent<AudioSource>(); //If so then assign it
            }
        }

        if (!musicPlayerHud && EnableNotifications) //If there's no musicPlayerHud assigned AND we have enabled the notification
            musicPlayerHud = GetComponent<MusicPlayerHud>(); //Then assign the MusicPlayerHud in gameobject to musicPlayerHud

        //At this point we have everything set up and ready to play any song

        ChangeSong(); //Finally play a song
    }

    private void Update()
    {
        if (EnableButtons) //If the EnableButtons is enabled (if not then ignore this part of code)
        {
            if (Input.GetButtonDown("ShuffleMusic")) //If the ShuffleMusic Button is pressed
            {
                ShuffleMusic(); //Call the Shuffle function
            }

            if (Input.GetButtonDown("PausePlayMusic")) //If the Play/Pause Button is pressed
            {
                PausePlayMusic(); //Call the Pause/Play function
            }

            if (Input.GetButtonDown("NextSong")) //If the NextSong Button is pressed
            {
                NextSong(); //Call the Next Song function
            }

            if (Input.GetButtonDown("PreviousSong")) //If the PreviousSong Button is pressed
            {
                PreviousSong(); //Call the Previous Song function
            }
        }
    }

    public void ChangeSongPosition(float position)
    {
        audioSource.time = Mathf.Lerp(0, audioSource.clip.length, position);
    }

    public void ShuffleMusic()
    {
        Shuffle = !Shuffle; //If Shuffle is enabled then disable it, and if it's enabled then disable it
    }
    
    public void PausePlayMusic()
    {
        if (audioSource.isPlaying) //Is the AudioSource is playing something?
        {
            audioSource.Pause(); //If so then Pause the song
        }
        else //Is the AudioSource paused?
        {
            audioSource.UnPause();//If so then Resume the song
        }

        isPlaying = !isPlaying; //Change the isPlaying state
    }

    public void NextSong()
    {
        if (!Shuffle) //Check if shuffle is disabled
        {
            if (CurrentSongNumber < Songs.Length) //Check if the Selected Song Number is less than the maximum songs number available in the Songs list
            {
                CurrentSongNumber++; //Increase the Selected Song Number by 1 to load the next song
            }
            else //If the Current Song Number is greater than the maximum songs number
            {
                CurrentSongNumber = 0; //Then reset it to 0(We listened to the playlist so we should listen to it again)
            }
        }
        ChangeSong(); //Finally change the song to the one we chose
    }

    public void PreviousSong()
    {
        if (!Shuffle) //Check if shuffle is disabled
        {
            if (CurrentSongNumber > 0) //Check if the Selected Song Number is greater than 0 which is the minimum song number
            {
                CurrentSongNumber--; //Decrease the Selected Song Number by 1 to load the previous song
            }
            else //If the Current Song Number is less than 0
            {
                CurrentSongNumber = Songs.Length - 1; //Then the previous song should be the last song in the playlist
            }
        }
        ChangeSong(); //Finally change the song to the one we chose
    }

    private void LateUpdate()
    {
        //Here we check that there's always a song playing
        if (!audioSource.isPlaying && isPlaying) //If there's no song is playing and isPlaying is enable (meaning there should be a song playing)
        {
            if (!Shuffle) //And if we don't have shuffle on (Since ChangeSong handles shuffle anyways)
            {
                CurrentSongNumber++; //Then load the next song in the playlist
            }
            ChangeSong(); //Finally we call the ChangeSong to play the new song
        }
    }

    //This is where the magic happens, this method handles changing and playing songs
    public void ChangeSong()
    {
        if (Shuffle) //If shuffle is enabled
        {
            CurrentSongNumber = Random.Range(0, Songs.Length); //To shuffly between songs we choose a random song number between 0 and the songs number from the Songs list
        }

        if (!Shuffle) //If shuffle is disabled
        {
            //By default we move forward in songs so we load the next song as soon as the one playing finishes
            if (CurrentSongNumber >= Songs.Length) //But if we finish the playlist by increasing the CurrentSongNumber more than the maximum songs number
            {
                CurrentSongNumber = 0; //Then we reset it to the first song which has the number of 0
            }
        }

        if (Songs.Length > 1) //Check if the songs available in the list is greater than one (So that we won't cause an overflow)
        {
            if (CurrentSongNumber == LatestSongNumber) //If the current song (which is randomly generated) is the same as the song we have just played
            {
                ChangeSong(); //Then we let the system choose another song, since we don't want to listen to the same song twice in a row
                return;
            }
        }

        if (CurrentSongNumber != LatestSongNumber || Songs.Length <= 1) //But if the the current song is a different from the one we have been listening to OR there's only one song available
        {
            PlaySong(); //Then go ahead and play this song
            LatestSongNumber = CurrentSongNumber; //Also we need to set the Latest Song Number to the new one playing so that we can check the next song
        }
    }

    //After setting everything we should play the song in AudioSource as well as notifying the user about the new song (If Enabled)
    public void PlaySong()
    {
        audioSource.time = 0; //Reset the AudioSource time to 0
        audioSource.clip = Songs[CurrentSongNumber].audioClip; //Assign the Selected Song by getting the Song by passing the CurrentSongNumber to Songs list and getting the audioClip
        audioSource.loop = Songs[CurrentSongNumber].Loop; //Assign the  selected song but this time by getting the check for whether the song should loop or not
        audioSource.Play(); //Finally Play the song in the Audio Source
        //By now the song should be playing and we have everything taken care of
        //But we should notify the user
        if (musicPlayerHud && EnableNotifications) //Should there be notifications? Or Is the MusicPLayerHud was assigned? (By disabling it this will be useful for background music or Simple Music Player)
        {
            SongName = Songs[CurrentSongNumber].SongName; //Assign the song name to the SongName string (loaded by MusicPlayerHud)
            ArtistName = ArtistPrefix + Songs[CurrentSongNumber].ArtistName; //Add ArtistPrefix (i.e Produced By) to the SongName string then Assign the artist name to it (loaded by MusicPlayerHud)
            
            //Last thing is telling the MusicPlayerHud to send the notification with the properties we set here
            //                        We pass the Song Cover Image      / Of course the Song Name          / and also the Artist Name
            musicPlayerHud.ChangeSong(Songs[CurrentSongNumber].SongImage, Songs[CurrentSongNumber].SongName, Songs[CurrentSongNumber].ArtistName);
        }
    }
}