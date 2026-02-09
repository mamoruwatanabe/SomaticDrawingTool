using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class IntroUI : MonoBehaviour
{
    // Set this to your Select scene name
    [SerializeField] private string nextSceneName = "02_Select";

    public void OnStartPressed()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
