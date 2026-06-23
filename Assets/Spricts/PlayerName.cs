using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerName : MonoBehaviour
{
    [SerializeField] TMP_InputField inputField;

    public string GetPlayerName()
    {
        return inputField.text;
    }
}
