using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TiltBrush
{
    public class MusicTool : BaseTool
    {
        public GameObject m_MusicCanvas;
        public AudioSource m_AudioSource;
        public List<AudioClip> m_Tracks;
        private int currentTrackIndex = 0;

        public override void Init()
        {
            base.Init();
            m_Type = ToolType.MusicTool;

            if (m_MusicCanvas != null)
                m_MusicCanvas.SetActive(true);  // 常時表示

            PlayTrack();  // 自動再生
        }

        void PlayTrack()
        {
            if (m_Tracks != null && m_Tracks.Count > 0 && m_AudioSource != null)
            {
                m_AudioSource.clip = m_Tracks[currentTrackIndex];
                m_AudioSource.loop = true;
                m_AudioSource.Play();
            }
        }

        public override void EnableTool(bool bEnable)
        {
            base.EnableTool(bEnable);
            if (m_MusicCanvas != null)
                m_MusicCanvas.SetActive(bEnable);
        }

        public override void UpdateTool()
        {
            base.UpdateTool();
            // Optional: MusicTool固有の処理を追加
        }
    }
}




// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// namespace TiltBrush
// {
//     public class MusicTool : BaseTool
//     {
//         public GameObject m_MusicCanvas;     // InspectorでUIをアサイン
//         public AudioSource m_AudioSource;    // InspectorでAudioSourceをアサイン

//         public override void Init()
//         {
//             base.Init();
//             m_Type = ToolType.MusicTool;

//             if (m_MusicCanvas != null)
//             {
//                 m_MusicCanvas.SetActive(true);  // 常に表示
//             }
//         }

//         void start ()
//         {
//             m_MusicCanvas.SetActive(true);

//             AudioManager.m_Instance?.PlayClip(0);
//         }

//         public override void EnableTool(bool bEnable)
//         {
//             base.EnableTool(bEnable);

//             if (m_MusicCanvas != null)
//             {
//                 m_MusicCanvas.SetActive(true); // 非表示にしない
//             }
//         }

//         public override void UpdateTool()
//         {
//             base.UpdateTool();

//             // 🎯 左手コントローラーに追従させる処理
//             if (m_MusicCanvas != null)
//             {
//                 Transform leftHand = InputManager.m_Instance.GetController(InputManager.ControllerName.Brush);
//                 if (leftHand != null)
//                 {
//                     m_MusicCanvas.transform.position = leftHand.position + leftHand.forward * 0.2f + leftHand.up * 0.05f;
//                     m_MusicCanvas.transform.rotation = Quaternion.LookRotation(leftHand.forward, leftHand.up);
//                 }
//             }
//         }

//         // 🎵 UI側のボタンから呼ぶ：選択されたクリップを再生
//         public void PlayClip(AudioClip clip)
//         {
//             if (m_AudioSource != null && clip != null)
//             {
//                 m_AudioSource.Stop();
//                 m_AudioSource.clip = clip;
//                 m_AudioSource.loop = true;
//                 m_AudioSource.Play();
//             }
//         }
//     }
// }




// namespace TiltBrush
// {
//     public class MusicTool : BaseTool
//     {
//         public GameObject m_MusicCanvas;

//         public override void Init()
//         {
//             base.Init();
//             m_Type = ToolType.MusicTool;
//             if (m_MusicCanvas != null)
//             {
//                 m_MusicCanvas.SetActive(false);
//             }
//         }

//         public override void EnableTool(bool bEnable)
//         {
//             base.EnableTool(bEnable);
//             if (m_MusicCanvas != null)
//             {
//                 m_MusicCanvas.SetActive(bEnable);
//             }
//         }

//         public override void UpdateTool()
//         {
//             base.UpdateTool();
//             // Optional: MusicTool固有の処理を追加
//         }
//     }
// }
