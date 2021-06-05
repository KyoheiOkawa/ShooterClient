using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Bullet : MonoBehaviour
{
    [SerializeField]
    uint wallHitLimit = 1;

    [SerializeField]
    float speed = 3.0f;

    [SerializeField]
    BulletType bulletType;

    uint wallHitCount = 0;

    Rigidbody2D rigid;

    bool isFire = false;

    Action fireAction = null;

    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();   
    }

    void FixedUpdate()
    {
        if(fireAction != null)
        {
            fireAction();
            fireAction = null;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Wall"))
        {
            wallHitCount++;
            if (wallHitCount > wallHitLimit)
                Destroy(this.gameObject);
        }
    }

    public void Fire(Vector2 dir,int fireFrame)
    {
        if (isFire)
            return;

        bulletType.fireFrame = fireFrame;

        fireAction = () =>
        {
            dir.Normalize();
            rigid.velocity = dir * speed;
        };

        isFire = true;
    }
}
