using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    MobileInput mobileInput;

    [SerializeField]
    Player player;

    void Start()
    {
        mobileInput.StickAxisEvent += OnAxis;
        mobileInput.FireEvent += OnFire;
    }

    void Update()
    {
        
    }

    void OnDestroy()
    {
        mobileInput.StickAxisEvent -= OnAxis;
        mobileInput.FireEvent -= OnFire;
    }

    void OnAxis(Vector2 axis)
    {
        player.Move(axis);
    }

    void OnFire()
    {
        player.Fire(player.transform.up,0);
    }
}
