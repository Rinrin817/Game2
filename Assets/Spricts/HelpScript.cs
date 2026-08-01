using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class HelpScript : MonoBehaviour
{
    [SerializeField] GameObject[] ruleObj;
    int count = 0;
    int count2 = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            ruleObj[count].SetActive(false);
            count ++;
            if(count == ruleObj.Length) count = 0;
        }
        count2 = 0;
        for(int i = 0; i < ruleObj.Length; i ++)
        {
            if(!ruleObj[i].activeSelf) count2 ++ ;
        }
        if(count2 == 3) count = 0;
    }

    public void OpenRule()
    {
        for(int i = 0; i < ruleObj.Length; i ++)
        {
            ruleObj[i].SetActive(true);
        }
    }
}
