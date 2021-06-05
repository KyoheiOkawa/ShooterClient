using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoblleInputTest : MonoBehaviour
{
    [SerializeField]
    MobileInput mobileInput;

    // Start is called before the first frame update
    void Start()
    {
        mobileInput.StickAxisEvent += Axis;
        mobileInput.FireEvent += Fire;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void Axis(Vector2 axis)
    {
        Debug.Log("axis:" + axis.ToString());
    }

    void Fire()
    {
        Debug.Log("Fire");
    }
}
