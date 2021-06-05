using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class Player : MonoBehaviour
{
    public event PlayerNetworkController.HitBulletDelegate hitBulletEvent;

    [SerializeField]
    float maxSpeed = 10.0f;

    [SerializeField]
    GameObject bulletPrefab;

    [SerializeField]
    int maxHP = 100;

    [SerializeField]
    int bulletDamage = 5;

    [SerializeField]
    BulletType.Type hitBulletType;

    [SerializeField]
    GameObject messagePanelPrefab;

    GameObject msgPanel;

    Action winloseAction;

    static bool isFinishedGame = false;

    public int MaxHP
    {
        get
        {
            return maxHP;
        }
    }

    int hp;

    public int HP
    {
        get
        {
            return HP;
        }
    }

    public float NormalizedHP
    {
        get
        {
            return (float)hp / (float)maxHP;
        }
    }

    Rigidbody2D rigid;

    bool isAxis = false;

    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        hp = maxHP;
        isFinishedGame = false;
    }

    void Update()
    {
        if(winloseAction != null && !isFinishedGame)
        {
            winloseAction();
            winloseAction = null;
            isFinishedGame = true;
        }
    }

    void FixedUpdate()
    {
        if (isFinishedGame)
            return;

        MoveUpdate();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Bullet"))
        {
            var bulletType = collision.gameObject.GetComponent<BulletType>();
            if (hitBulletType == bulletType.TYPE)
            {
                if(hitBulletEvent != null)
                {
                    hitBulletEvent(bulletType.TYPE, bulletType.fireFrame);
                }
                Destroy(collision.transform.parent.gameObject);
            }
        }
    }

    public void Damage()
    {
        hp -= bulletDamage;

        if (hp <= 0.0f)
        {
            if (msgPanel == null)
            {
                winloseAction = () =>
                {
                    var canvas = GameObject.Find("Canvas");
                    msgPanel = Instantiate(messagePanelPrefab, canvas.transform);
                    msgPanel.GetComponent<MessagePanel>().TapAction = () =>
                    {
                        SceneManager.LoadScene("Menu");
                    };
                    msgPanel.GetComponent<MessagePanel>().TapWaitTime = 2.0f;

                    switch (hitBulletType)
                    {
                        case BulletType.Type.RED:
                            {
                                msgPanel.GetComponent<MessagePanel>().Message = "YOU WIN!!";
                                break;
                            }
                        case BulletType.Type.BLUE:
                            {
                                msgPanel.GetComponent<MessagePanel>().Message = "YOU LOSE...";
                                break;
                            }
                    }
                };
            }
        }
    }

    public void Move(Vector2 dir)
    {
        if (dir.magnitude < 0.1f)
            return;

        isAxis = true;

        rigid.velocity = dir * maxSpeed;

        float angle = Vector2.Angle(Vector2.up, dir.normalized);
        if (dir.x > 0)
            angle *= -1.0f;
        rigid.SetRotation(angle);
    }

    void MoveUpdate()
    {
        if(isAxis)
        {
            isAxis = false;
        }
        else
        {
            rigid.velocity = Vector2.zero;
        }
    }

    public void Fire(Vector2 dir,int fireFrame)
    {
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        bullet.GetComponent<Bullet>().Fire(dir,fireFrame);
    }

    public void FixPositionRot(Vector2 pos,float rotZ)
    {
        rigid.position = pos;
        rigid.rotation = rotZ;
    }
}
