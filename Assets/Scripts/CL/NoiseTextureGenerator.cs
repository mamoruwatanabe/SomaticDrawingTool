using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


public class NoiseTextureGenerator : MonoBehaviour
{
    public int size = 128;

    void Start()
    {
        Texture2D tex = new Texture2D(size, size);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float value = Random.value;
                tex.SetPixel(x, y, new Color(value, value, value));
            }
        }

        tex.Apply();

        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + "/GeneratedNoise.png", bytes);

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif

        Debug.Log("✅ ノイズ画像生成完了: Assets/GeneratedNoise.png");
    }
}
