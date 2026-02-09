using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TiltBrush
{
    public class ToolManager : MonoBehaviour
    {
        private BaseTool[] m_Tools;

        void Start()
        {
            m_Tools = GetComponentsInChildren<BaseTool>(true);
            Debug.Log($"[CustomToolManager] Found {m_Tools.Length} tools");

            foreach (BaseTool tool in m_Tools)
            {
                Debug.Log($"Tool found: {tool.name}, Type: {tool.m_Type}");
                tool.Init();
                tool.gameObject.SetActive(false); // 初期は無効化
            }

            // MusicTool を強制的に有効化して確認（テスト用）
            foreach (BaseTool tool in m_Tools)
            {
                if (tool.m_Type == BaseTool.ToolType.MusicTool)
                {
                    tool.EnableTool(true);
                    Debug.Log("MusicTool enabled!");
                }
            }
        }
    }
}
