using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartButton : MonoBehaviour
{
    public static int playerCountStatic;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void FiveInvoke()
    {
        StartButton.playerCountStatic = 5;
    }

    public void NineInvoke()
    {
        StartButton.playerCountStatic = 9;
    }

    public void ThirteenInvoke()
    {
        StartButton.playerCountStatic = 13;
    }
}
