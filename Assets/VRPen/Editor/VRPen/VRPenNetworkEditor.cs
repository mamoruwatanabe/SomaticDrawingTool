using UnityEditor;
using UnityEngine;
using VRPen.VRPenPro;

namespace VRPenNamespace.Editors.VRPenPro
{
    [CustomEditor(typeof(VRPenNetwork))]
    public class VRPenNetworkEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            VRPenNetwork _target = (VRPenNetwork)target;
            
            if(_target.Menu == null) return;
      
            foreach (var line in _target.Menu.Lines)
            {
                if (line.NotSelectable)
                {
                    GUILayout.Label(line.Text);
                }
                else
                {
                    if (GUILayout.Button(line.Text))
                    {
                        line.Action();
                        return;
                    }
                }
            }

            if (!_target.MenuIsOnMain())
            {
                if(GUILayout.Button("Back")) _target.MenuClick();
            }
        }
    }
}