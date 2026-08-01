using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultText : MonoBehaviour
{
    [SerializeField] GameObject resultImage;
    [SerializeField] Text roleText;
    [SerializeField] Text resultText;
    string role;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void ResetStatic()
    {
        VariableManager.isFinish = -1;
        VariableManager.playerRole = -1;
    }

    void Start()
    {
        if(VariableManager.isFinish != -1)
        {
            resultImage.SetActive(true);
            if(VariableManager.playerRole == 0) role = "Thief";
            if(VariableManager.playerRole == 1) role = "Pollice";
            if(VariableManager.playerRole == 2) role = "Daruma";
            
            roleText.text = "You were " + role;

            if(VariableManager.isFinish == 0)
            {
                resultText.text = "YOU WIN !!";
            }
            if(VariableManager.isFinish == 1)
            {
                resultText.text = "YOU LOSE...";
            }
            if(VariableManager.isFinish == 2)
            {
                resultText.text = "Close Win!";
            }

            VariableManager.isFinish = -1;
            VariableManager.playerRole = -1;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButton(0))
        {
            if(resultText != null) resultImage.SetActive(false);
        }
    }
}
