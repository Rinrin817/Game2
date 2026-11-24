
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalScript : MonoBehaviour
{
    [SerializeField] GameObject ManagerObj;
    [SerializeField] GameObject[] gameObjects;

    VariableManager variableManager;

    void Start()
    {
        variableManager = ManagerObj.GetComponent<VariableManager>();
    }

    void Update()
    {
        if (variableManager.isGoalOpen)
        {
            gameObjects[0].SetActive(false);
            gameObjects[1].SetActive(true);
            gameObjects[2].SetActive(true);
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (!variableManager.isGoalOpen || collision.gameObject.tag != "Player")
            return;

        PlayerController player = collision.gameObject.GetComponent<PlayerController>();

        if (player != null && player.roleNumber == 0)
        {
            player.RequestGoal();
            return;
        }

        EnemyPlayerController enemy = collision.gameObject.GetComponent<EnemyPlayerController>();

        if (enemy != null && enemy.roleNumber == 0)
        {
            collision.gameObject.SetActive(false);
        }
    }
}
