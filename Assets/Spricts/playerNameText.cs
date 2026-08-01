using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class playerNameText : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI myText;

    void Start()
    {
        myText.text = PlayerData.PlayerName;
    }
}
