using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoseSetter : MonoBehaviour
{
    public Transform leftUpperArm;

    void Start()
    {
        // 左腕を前方に45度上げる
        leftUpperArm.localRotation = Quaternion.Euler(45, 0, 0);
    }
}
