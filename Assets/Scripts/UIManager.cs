using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    Text rivalNameText;

    [SerializeField]
    Image rivalHPBar;

    [SerializeField]
    Image myHPBar;

    [SerializeField]
    Player player;

    [SerializeField]
    Player rivalPlayer;

    void Start()
    {
        rivalNameText.text = PlayerPrefs.GetString("RivalName");
    }

    void Update()
    {
        rivalHPBar.fillAmount = rivalPlayer.NormalizedHP;
        myHPBar.fillAmount = player.NormalizedHP;
    }
}
