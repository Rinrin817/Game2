using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class HelpScript : MonoBehaviour
{
    [SerializeField] GameObject[] ruleObj;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip openAudio;
    [SerializeField] AudioClip closeAudio;
    [SerializeField] TextMeshProUGUI countText;
    int count = 0;
    int count2 = 0;
    int count3 = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
        {
            if (EventSystem.current.currentSelectedGameObject.GetComponent<UnityEngine.UI.InputField>() != null ||
                EventSystem.current.currentSelectedGameObject.GetComponent<TMPro.TMP_InputField>() != null)
            {
                return;
            }
        }
        if(Input.GetKeyDown(KeyCode.H)) OpenRule();
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
        if(count2 == 3)
        {
            count = 0;
            countText.gameObject.SetActive(false);
        }
        countText.text = (count2 + 1).ToString() + "/3";
    }

    public void OpenRule()
    {
        for(int i = 0; i < ruleObj.Length; i ++)
        {
            ruleObj[i].SetActive(true);
        }
        countText.gameObject.SetActive(true);
        audioSource.PlayOneShot(openAudio, 0.5f);
    }
}
