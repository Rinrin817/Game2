using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

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
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip openAudio;
    [SerializeField] AudioClip closeAudio;
    public int finishResultTemp;
    
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
        if(Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            if(SettingImage.gameObject.activeSelf)
            {
                SettingImage.gameObject.SetActive(false);
                SEImage.gameObject.SetActive(false);
                BGMImage.gameObject.SetActive(false);
                XSensivitiyImage.gameObject.SetActive(false);
                YSensivitiyImage.gameObject.SetActive(false);
                audioSource.PlayOneShot(closeAudio, 0.5f);
                if(!startFusion.isClickCamera)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;   
                }
            }
        }
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
        {
            if (EventSystem.current.currentSelectedGameObject.GetComponent<UnityEngine.UI.InputField>() != null ||
                EventSystem.current.currentSelectedGameObject.GetComponent<TMPro.TMP_InputField>() != null)
            {
                return;
            }
        }
        if(SceneManager.GetActiveScene().name == "Home")
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;   
        }
        if(SceneManager.GetActiveScene().name == "Battle")
        {
            if(!SettingImage.gameObject.activeSelf && !startFusion.isClickCamera)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;  
            }
        }
        if(Input.GetKeyDown(KeyCode.J)) OpenSettings();
    }

    public void OpenSettings()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if(SettingImage.gameObject.activeSelf)
        {
            SettingImage.gameObject.SetActive(false);
            SEImage.gameObject.SetActive(false);
            BGMImage.gameObject.SetActive(false);
            XSensivitiyImage.gameObject.SetActive(false);
            YSensivitiyImage.gameObject.SetActive(false);
            audioSource.PlayOneShot(closeAudio, 0.5f);
            if(!startFusion.isClickCamera && SceneManager.GetActiveScene().name == "Battle")
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;   
            }
        }
        else
        {
            SettingImage.gameObject.SetActive(true);
            SEImage.gameObject.SetActive(true);
            BGMImage.gameObject.SetActive(true);
            XSensivitiyImage.gameObject.SetActive(true);
            YSensivitiyImage.gameObject.SetActive(true);   
            audioSource.PlayOneShot(openAudio, 0.5f);
        }
    }

    public void isCameraSetting()
    {
        if(startFusion.isClickCamera) startFusion.isClickCamera = false;
        else startFusion.isClickCamera = true;
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
        startFusion.XsensivitySetting = (Xslider.value + 0.5f) * 300;
    }

    public void SetYesnsitivity()
    {
        startFusion.YsensivitySetting = (Yslider.value + 0.5f) * 150;
    }
}
