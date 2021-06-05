using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MobileInput : MonoBehaviour
{
    public delegate void StickAxisDelegate(Vector2 axis);
    public event StickAxisDelegate StickAxisEvent;

    public delegate void FireDelegate();
    public event FireDelegate FireEvent;

    [SerializeField]
    Image stickImage;

    [SerializeField]
    Canvas canvas;

    [SerializeField]
    float stickRange = 100.0f;

    [SerializeField]
    float fireIntervalSec = 0.1f;

    float downStartTime = 0.0f;

    Vector2 rectScale;

    Image ownImage;

    //スティックを一定以上倒していたらファイヤーイベント呼ばない
    bool canFire = false;

    void Start()
    {
        Vector2 canvasRectSize = canvas.GetComponent<RectTransform>().sizeDelta;
        rectScale.x = canvasRectSize.x / Screen.width;
        rectScale.y = canvasRectSize.y / Screen.height;

        ownImage = GetComponent<Image>();
        ownImage.enabled = false;
        stickImage.enabled = false;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GetComponent<RectTransform>().anchoredPosition = GetMouseCanvasPosition();
            downStartTime = Time.time;
            canFire = true;
        }
        else if(Input.GetMouseButton(0))
        {
            stickImage.enabled = true;
            ownImage.enabled = true;

            StickAxis();
        }
        else if(Input.GetMouseButtonUp(0))
        {
            stickImage.enabled = false;
            ownImage.enabled = false;

            FireJudge();
        }
    }

    Vector2 GetMouseCanvasPosition()
    {
        Vector2 mousePos = Input.mousePosition;
        mousePos.Scale(rectScale);
        return mousePos;
    }

    void FireJudge()
    {
        float timeDelta = Time.time - downStartTime;
        if (timeDelta < fireIntervalSec)
        {
            if (FireEvent != null && canFire)
                FireEvent();
        }
    }

    void StickAxis()
    {
        Vector2 mousePos = GetMouseCanvasPosition();
        RectTransform rectTransform = GetComponent<RectTransform>();

        Vector2 diff = mousePos - rectTransform.anchoredPosition;
        if (diff.magnitude > stickRange)
        {
            diff = diff.normalized * stickRange;
        }
        stickImage.rectTransform.anchoredPosition = diff;

        if (StickAxisEvent != null)
        {
            Vector2 axis = diff.normalized * (diff.magnitude / stickRange);
            if (axis.magnitude > 0.1f)
            {
                canFire = false;
                StickAxisEvent(axis);
            }
        }
    }
}
