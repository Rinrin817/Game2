using System.Dynamic;
using UnityEngine;
using UnityEngine.UI;

public class ImageScript : MonoBehaviour
{
    [SerializeField] private Image myImage;
    [SerializeField] Color currentColor;
    private float timer = 0f;

    void Start()
    {
        myImage.color =  new Color(currentColor.r, currentColor.g, currentColor.b, 0);
        Invoke("ActiveFalse", 1f);
    }

    void OnEnable()
    {
        timer = 0f;
        myImage.color = currentColor;
        Invoke("ActiveFalse", 1f);
    }

    void Update()
    {
        timer += Time.deltaTime;
        myImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, Mathf.Lerp(0.5f, 0f, timer / 0.5f));
    }

    void ActiveFalse()
    {
        gameObject.SetActive(false);
    }
}