using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletType : MonoBehaviour
{
    public enum Type
    {
        RED,
        BLUE
    }

    [SerializeField]
    BulletType.Type type;

    public BulletType.Type TYPE
    {
        get
        {
            return type;
        }
    }

    public int fireFrame;
}
