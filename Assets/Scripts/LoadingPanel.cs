using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingPanel : MonoBehaviour
{
    [SerializeField]
    Text loadingText;

    [SerializeField]
    float changeTextSec = 0.25f;

    List<string> loadingStrs = new List<string>();

    void Start()
    {
        loadingStrs.Add("通信中   ");
        loadingStrs.Add("通信中.  ");
        loadingStrs.Add("通信中.. ");
        loadingStrs.Add("通信中...");

        loadingText.text = loadingStrs[0];

        StartCoroutine(ChangeLoadingTextRoutine());
    }

    IEnumerator ChangeLoadingTextRoutine()
    {
        int strIndex = 0;

        while(true)
        {
            loadingText.text = loadingStrs[strIndex];
            strIndex++;
            if (strIndex >= loadingStrs.Count)
                strIndex = 0;

            yield return new WaitForSeconds(changeTextSec);
        }
    }
}
