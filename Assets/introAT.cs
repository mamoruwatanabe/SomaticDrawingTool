using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class IntroAT : MonoBehaviour
{
    // Set this to your Select scene name
    [SerializeField] private string nextSceneName = "AudioTactitle";

    public void OnStartPressed()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
