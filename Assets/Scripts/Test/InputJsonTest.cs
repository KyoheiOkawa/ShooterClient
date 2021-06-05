using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputJsonTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        RequesJsons.PlayInputRequest data = new RequesJsons.PlayInputRequest();

        RequesJsons.UserObj own = new RequesJsons.UserObj();
        own.id = "3344151";
        own.name = "alice";
        own.port = "35333";
        own.address = "localhost";

        RequesJsons.UserObj rival = new RequesJsons.UserObj();
        rival.id = "3544151";
        rival.name = "bob";
        rival.port = "35333";
        rival.address = "localhost";

        RequesJsons.InputObj input = new RequesJsons.InputObj();
        input.frame = 0;
        input.axisX = 0.5f;
        input.axisY = 0.5f;
        input.isFire = false;

        data.ownObj = own;
        data.rivalObj = rival;
        data.inputObjectsObj.Add(input);
        data.inputObjectsObj.Add(input);
        data.inputObjectsObj.Add(input);


        Debug.Log(data.ToJson());

        JsonNode node = JsonNode.Parse(data.ToJson());
        foreach(var obj in node["inputObjects"])
        {
            RequesJsons.InputObj iobj = JsonUtility.FromJson<RequesJsons.InputObj>(obj.Get<string>());
            Debug.Log(JsonUtility.ToJson(iobj));
        }
    }

    /* 以下生成されるJSON
    {
        "type":"input",
        "own":"{\"id\":\"3344151\",\"name\":\"alice\",\"port\":\"35333\",\"address\":\"localhost\"}",
        "rival":"{\"id\":\"3544151\",\"name\":\"bob\",\"port\":\"35333\",\"address\":\"localhost\"}",
        "inputObjects":
        [
            "{\"frame\":0,\"axisX\":0.5,\"axisY\":0.5,\"isFire\":false}",
            "{\"frame\":0,\"axisX\":0.5,\"axisY\":0.5,\"isFire\":false}",
            "{\"frame\":0,\"axisX\":0.5,\"axisY\":0.5,\"isFire\":false}"
        ]
    }
    */
}
