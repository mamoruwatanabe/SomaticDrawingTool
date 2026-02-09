using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TiltBrush
{
    public class CubeTool : BaseTool
    {
        public GrabWidget cubePrefab;

        public override void UpdateTool()
        {
            base.UpdateTool();

            if (InputManager.m_Instance.GetCommandDown(InputManager.SketchCommands.Activate))
            {
                Transform brushXf = InputManager.m_Instance.GetBrushControllerAttachPoint();
                float scale = brushXf.localScale.x;
                TrTransform spawnXf = TrTransform.TRS(brushXf.position, brushXf.rotation, scale);

                SketchMemoryScript.m_Instance.PerformAndRecordCommand(
                    new CreateWidgetCommand(cubePrefab, spawnXf)
                );
            }
        }
    }
}


// v1
// namespace TiltBrush
// {
//     public class CubeTool : MonoBehaviour
//     {
//         public GameObject cubePrefab;  // GrabWidgetがついているPrefabを割り当て

//         void Update()
//         {
//             if (InputManager.m_Instance.GetCommandDown(InputManager.SketchCommands.Activate))
//             {
//                 // ブラシコントローラの位置・回転を取得
//                 Transform brushXf = InputManager.m_Instance.GetBrushControllerAttachPoint();

//                 // プレハブのGrabWidgetコンポーネントを取得
//                 GrabWidget prefabWidget = cubePrefab.GetComponent<GrabWidget>();
//                 if (prefabWidget == null)
//                 {
//                     Debug.LogError("Cube prefab must have GrabWidget component.");
//                     return;
//                 }

//                 // TrTransformを作成（位置、回転、スケール）
//                 float scale = brushXf.localScale.x;  // 等方スケール用
//                 TrTransform spawnXf = TrTransform.TRS(brushXf.position, brushXf.rotation, scale);

//                 // Instantiateしてコマンドで登録
//                 SketchMemoryScript.m_Instance.PerformAndRecordCommand(
//                     new CreateWidgetCommand(prefabWidget, spawnXf)
//                 );
//             }
//         }
//     }
// }
