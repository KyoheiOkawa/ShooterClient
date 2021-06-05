using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RequesJsons : MonoBehaviour
{
    public class MatchRequest
    {
        public string type = "match";
        public string name = "Alice";
    }

    public class UserObj
    {
        public string id;
        public string name;
        public string port;
        public string address;
    }

    public class InputObj
    {
        public int frame;
        public float axisX;
        public float axisY;
        public bool isFire;
        public float fireDirX;
        public float fireDirY;
        public float posX;
        public float posY;
        public float rotZ;

        public InputObj()
        {
            Reset();
        }

        public InputObj Clone()
        {
            InputObj ret = new InputObj();
            ret.frame = this.frame;
            ret.axisX = this.axisX;
            ret.axisY = this.axisY;
            ret.isFire = this.isFire;
            ret.fireDirX = this.fireDirX;
            ret.fireDirY = this.fireDirY;
            ret.posX = this.posX;
            ret.posY = this.posY;
            ret.rotZ = this.rotZ;

            return ret;
        }

        public void Reset()
        {
            frame = 0;
            axisX = 0;
            axisY = 0;
            isFire = false;
            fireDirX = 0;
            fireDirY = 0;
        }
    }

    public class PlayInputRequest
    {
        public string type = "input";
        public string own;
        public string rival;
        public int requireNextFrame;
        public List<string> inputObjects = new List<string>();

        public UserObj ownObj;
        public UserObj rivalObj;
        public List<InputObj> inputObjectsObj = new List<InputObj>();

        public string ToJson()
        {
            own = JsonUtility.ToJson(ownObj);
            rival = JsonUtility.ToJson(rivalObj);

            inputObjects.Clear();
            foreach(InputObj obj in inputObjectsObj)
            {
                inputObjects.Add(JsonUtility.ToJson(obj));
            }

            return JsonUtility.ToJson(this);
        }
    }

    public class RivalInputReturn
    {
        public string type = "rival-input";
        public int requireNextFrame;
        public List<string> inputObjects = new List<string>();
    }

    public class HitBulletRequest
    {
        public string type = "hit-bullet";
        public string bulletType;
        public int fireFrame;
        public string own;
        public string rival;

        public UserObj ownObj;
        public UserObj rivalObj;

        public HitBulletRequest Clone()
        {
            HitBulletRequest ret = new HitBulletRequest();
            ret.bulletType = this.bulletType;
            ret.fireFrame = this.fireFrame;
            ret.ownObj = this.ownObj;
            ret.rivalObj = this.rivalObj;

            return ret;
        }

        public string ToJson()
        {
            own = JsonUtility.ToJson(ownObj);
            rival = JsonUtility.ToJson(rivalObj);

            return JsonUtility.ToJson(this);
        }

        public void FromJson()
        {
            ownObj = JsonUtility.FromJson<UserObj>(own);
            rivalObj = JsonUtility.FromJson<UserObj>(rival);
        }

        public BulletType.Type GetBulletType()
        {
            return (BulletType.Type)Enum.Parse(typeof(BulletType.Type), bulletType);
        }
    }
}
