using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class playerNameText : MonoBehaviour
{
    [SerializeField] Text myText;

    void Start()
    {
        myText.text = PlayerData.PlayerName;
    }
}
