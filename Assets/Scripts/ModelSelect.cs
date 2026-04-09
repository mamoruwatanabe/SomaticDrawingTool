using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelSelect : MonoBehaviour
{
    public GameObject[] models;

    private int currentIndex = 0;

    void Start()
    {
        // Make sure only the first model is active at start
        UpdateModels();
    }

    public void Change(int index)
    {
        // Check bounds
        if (index < 0 || index >= models.Length)
        {
            Debug.LogWarning("Index out of range!");
            return;
        }

        currentIndex = index;
        UpdateModels();
    }

    private void UpdateModels()
    {
        for (int i = 0; i < models.Length; i++)
        {
            if (models[i] != null)
            {
                models[i].SetActive(i == currentIndex);
            }
        }
    }
}
