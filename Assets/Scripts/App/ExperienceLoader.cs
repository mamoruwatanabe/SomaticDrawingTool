using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ExperienceLoader : MonoBehaviour
{
    void Start()
    {
        Debug.Log("Selected drawing: " + AppState.SelectedDrawingID);

        string sketchPath = Path.Combine(
            Application.streamingAssetsPath,
            "Sketches",
            "Marco-sketch-v1.tilt"
        );

        LoadDrawing(sketchPath);
    }

    void LoadDrawing(string sketchPath)
    {
        Debug.Log("Loading sketch from: " + sketchPath);

        // NEXT STEP: call OpenBrush sketch loader here
    }
}
