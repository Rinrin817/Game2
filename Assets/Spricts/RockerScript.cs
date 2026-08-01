using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockerScript : MonoBehaviour
{
    [SerializeField] GameObject[] ItemObjs;
    int nowItemID;
    // Start is called before the first frame update
    public void SetItem(int buttonID)
    {
        ItemObjs[buttonID].SetActive(true);
    }
}
