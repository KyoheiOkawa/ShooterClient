using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class MessagePanel : MonoBehaviour
{
    [SerializeField]
    Text text;

    public string Message
    {
        get
        {
            return text.text;
        }
        set
        {
            text.text = value;
        }
    }

    float tapWaitTime = 0.0f;

    public float TapWaitTime
    {
        get
        {
            return TapWaitTime;
        }
        set
        {
            tapWaitTime = value;
        }
    }

    float tapWaitCount = 0.0f;

    Action action = null;

    public Action TapAction
    {
        get
        {
            return action;
        }
        set
        {
            action = value;
        }
    }

    void Start()
    {
        
    }

    void Update()
    {
        tapWaitCount += Time.unscaledDeltaTime;
        if (tapWaitCount < tapWaitTime)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            if(action != null)
                action();
            Destroy(this.gameObject);
        }
    }
}
