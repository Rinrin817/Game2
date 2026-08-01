using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class SettingScript : MonoBehaviour
{
    [SerializeField] GameObject startFusionObj;
    [SerializeField] Canvas thisCanvas;
    StartFusion startFusion;
    [SerializeField] Slider Xslider;
    [SerializeField] Slider Yslider;
    [SerializeField] Image SettingImage;
    [SerializeField] Button SEImage;
    [SerializeField] Button BGMImage;
    [SerializeField] Image XSensivitiyImage;
    [SerializeField] Image YSensivitiyImage;
    
    void Awake()
    {
        DontDestroyOnLoad(thisCanvas);
    }

    void Start()
    {
        startFusion = startFusionObj.GetComponent<StartFusion>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenSettings()
    {
        if(SettingImage.gameObject.activeSelf)
        {
            SettingImage.gameObject.SetActive(false);
            SEImage.gameObject.SetActive(false);
            BGMImage.gameObject.SetActive(false);
            XSensivitiyImage.gameObject.SetActive(false);
            YSensivitiyImage.gameObject.SetActive(false);
        }
        else
        {
            SettingImage.gameObject.SetActive(true);
            SEImage.gameObject.SetActive(true);
            BGMImage.gameObject.SetActive(true);
            XSensivitiyImage.gameObject.SetActive(true);
            YSensivitiyImage.gameObject.SetActive(true);   
        }
    }

    public void isSESetting()
    {
        if(startFusion.isSE) startFusion.isSE = false;
        else startFusion.isSE = true;
    }

    public void isBGMSetting()
    {
        if(startFusion.isBGM) startFusion.isBGM = false;
        else startFusion.isBGM = true;
    }

    public void SetXesnsitivity()
    {
        startFusion.XsensivitySetting = (Xslider.value + 0.1f) * 300;
    }

    public void SetYesnsitivity()
    {
        startFusion.YsensivitySetting = (Yslider.value + 0.1f) * 150;
    }
}
