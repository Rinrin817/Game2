using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerName : MonoBehaviour
{
    [SerializeField] TMP_InputField inputField;

    public string GetPlayerName()
    {
        #if UNITY_EDITOR
        int random = Random.Range(00, 99);
        return "製作者〆" + random.ToString();
        #endif
        return inputField.text;
    }
}
