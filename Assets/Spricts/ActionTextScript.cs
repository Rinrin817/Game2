using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;
using TMPro;

public class ActionTextScript : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI myText;
    float timeCount;
    // Start is called before the first frame update
    void Start()
    {
        timeCount = 0;
        Destroy(gameObject, 15f);
    }

    // Update is called once per frame
    void Update()
    {
        timeCount += Time.deltaTime;
        Color currentColor = myText.color;
        if(timeCount >= 1f) currentColor.a -= 0.5f * Time.deltaTime;
        myText.color = currentColor;
    }
}
