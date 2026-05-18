using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalSprict : MonoBehaviour
{
    [SerializeField] GameObject ManagerObj;
    [SerializeField] GameObject[] gameObjects;
    VariableManager variableManager;
    // Start is called before the first frame update
    void Start()
    {
        variableManager = ManagerObj.GetComponent<VariableManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if(variableManager.isGoalOpen)
        {
            gameObjects[0].SetActive(false);
            gameObjects[1].SetActive(true);
            gameObjects[2].SetActive(true);
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if(variableManager.isGoalOpen && collision.gameObject.tag == "Player")
        {
            Debug.Log("collision");
            if(collision.gameObject.GetComponent<PlayerController>() != null && collision.gameObject.GetComponent<PlayerController>().roleNumber == 0)
            {
                VariableManager.isFinish = 0;
            }
            if(collision.gameObject.GetComponent<EnemyPlayerController>() != null && collision.gameObject.GetComponent<EnemyPlayerController>().roleNumber == 0)
            {
                collision.gameObject.SetActive(false);
            }
        }
    }
}
