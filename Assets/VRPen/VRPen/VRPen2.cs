using System;
using System.IO;
using System.Linq;
using UnityEngine;
using VRPenNamespace;

namespace VRPenNamespace
{
    public partial class VRPen
    {
        public string SaveAsFile()
        {
            Scribble      = ScriptableObject.CreateInstance<VRPenScribble>();
            Scribble.name = DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss");

            if (!string.IsNullOrWhiteSpace(gameObject.scene.name))
            {
                Scribble.name = gameObject.scene.name + Scribble.name;
            }

            SaveCurrentScribble();

            var text = JsonUtility.ToJson(Scribble);
            var name = Scribble.name + ".vrpen.json";
            var path = Application.persistentDataPath + "/" + name;
            File.WriteAllText(path, text);
            Debug.Log(path);
            return name;
        }

        public string[] GetSavedFiles()
        {
            var files = Directory.GetFiles(Application.persistentDataPath, "*.vrpen.json")
                                 .Select(p => Path.GetFileName(p))
                                 .ToArray();
            return files;
        }

        public void LoadFile(string path)
        {
            Scribble      = ScriptableObject.CreateInstance<VRPenScribble>();
            Scribble.name = DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss");

            if (!string.IsNullOrWhiteSpace(gameObject.scene.name))
            {
                Scribble.name = gameObject.scene.name + Scribble.name;
            }

            var text = File.ReadAllText(Application.persistentDataPath + "/" + path);
            JsonUtility.FromJsonOverwrite(text, Scribble);

            Load();
        }
    }
}