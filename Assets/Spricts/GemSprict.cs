using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GemSprict : MonoBehaviour
{
    [SerializeField] GameObject effectObj;
    [SerializeField] GameObject PlayerObj;
    public int gemNumber;
    public bool effect;
    float timeLimit = 1f;
    float timeCount;
    // Start is called before the first frame update
    void Start()
    {
        timeCount = 0;
        effect = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(effect)
        {
            effectObj.SetActive(true);
            GetComponent<MeshRenderer>().enabled = false;
            foreach (var objCol in GetComponents<Collider>())
                {
                    foreach (var playerCol in PlayerObj.GetComponents<Collider>())
                    {
                        Physics.IgnoreCollision(objCol, playerCol);
                    }
                }
            timeCount += Time.deltaTime;
        }
        if(timeCount >= timeLimit && effect)
        {
            gameObject.SetActive(false);
        }
    }
}
