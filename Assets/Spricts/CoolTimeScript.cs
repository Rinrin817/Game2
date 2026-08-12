using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoolTimeScript : MonoBehaviour
{
    [SerializeField] Image myImage;
    bool isRed;
    bool isPink;
    bool isTimeLimit;

    private RectTransform rectTransform;
    private Vector2 basePosition;
    // Start is called before the first frame update
    void Start()
    {
        Reset();
    }

    // Update is called once per frame
    void Update()
    {
        if (isRed)
        {
            float shakeAmount = 5.0f;
            float shakeSpeed = 50.0f;
            float offsetX = Mathf.Sin(Time.time * shakeSpeed) * shakeAmount;
            float offsetY = Mathf.Cos(Time.time * shakeSpeed * 1.2f) * (shakeAmount * 0.5f);
            rectTransform.anchoredPosition = basePosition + new Vector2(offsetX, offsetY);
        }
        else if (isPink)
        {
            float shakeAmount = 1.0f;
            float shakeSpeed = 30.0f;
            float offsetX = Mathf.Sin(Time.time * shakeSpeed) * shakeAmount;
            float offsetY = Mathf.Cos(Time.time * shakeSpeed * 1.2f) * (shakeAmount * 0.5f);
            rectTransform.anchoredPosition = basePosition + new Vector2(offsetX, offsetY);
        }
        else
        {
            rectTransform.anchoredPosition = basePosition;
        }
    }

    public void ToRed()
    {
        myImage.color = Color.Lerp(Color.white, Color.red, 0.5f);
        isRed = true;
    }

    public void ToPink()
    {
        myImage.color = Color.Lerp(Color.white, Color.red, 0.3f);
        isPink = true;
    }

    public void Reset()
    {
        myImage.color = Color.white;
        isRed = false;
        isPink = false;
        rectTransform = myImage.GetComponent<RectTransform>();
        basePosition = rectTransform.anchoredPosition;
    }
}
