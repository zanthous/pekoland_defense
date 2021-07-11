using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotifyPlatform : MonoBehaviour
{
    PlatformController controller;
    private void Start()
    {
        controller = GetComponentInChildren<PlatformController>();
    }

    public void TriggerEvent()
    {
        controller.StartAnim();
    }
}
