using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultText : MonoBehaviour
{
    [SerializeField] GameObject resultImage;

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButton(0))
        {
            resultImage.SetActive(false);
        }
    }
}
