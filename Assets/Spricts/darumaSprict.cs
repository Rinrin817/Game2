using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class darumaSprict : MonoBehaviour
{
    [SerializeField] GameObject[] PlayerObj;
    [SerializeField] GameObject ManagerObj;
    [SerializeField] Transform[] targets;
    [SerializeField] float maxDistance = 80f;

    VariableManager variableManager;
    PlayerController playerController;
    EnemyPlayerController enemyPlayerController;

    float timeCount;
    float timeLimit;
    float speed;
    float random;

    bool finishRotate;
    public bool middleRotate;

    Quaternion targetRotation;

    void Start()
    {
        variableManager = ManagerObj.GetComponent<VariableManager>();
        timeLimit = Random.Range(1f, 3f);
        speed = Random.Range(5f, 10f);
        middleRotate = false;
        finishRotate = false;

        targetRotation = Quaternion.Euler(0, 180, 0);
    }

    void Update()
    {
        timeCount += Time.deltaTime;

        // --- 見ているときだけ検知 ---
        if (timeCount >= timeLimit && !middleRotate)
        {
            for (int i = 0; i < variableManager.playerCount; i++)
            {
                if (PlayerObj[i] == null || targets[i] == null) continue;
                if (CanSeeTarget(targets[i]))
                {
                    Debug.Log("標的を発見！");

                    playerController = PlayerObj[i].GetComponent<PlayerController>();
                    enemyPlayerController = PlayerObj[i].GetComponent<EnemyPlayerController>();

                    var pc = PlayerObj[i].GetComponent<PlayerController>();
                    var epc = PlayerObj[i].GetComponent<EnemyPlayerController>();

                    if (pc != null) pc.canMove = false;
                    if (epc != null) epc.canMove = false;
                    if(i == 0)
                    {
                        variableManager.textString = "Cannot Move!";   
                    }
                }
            }
        }

        // --- 回転処理 ---
        if(timeCount >= timeLimit)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * speed);

            if(!middleRotate)
            {
                targetRotation = Quaternion.Euler(0, 180, 0);
                random = Random.Range(0.3f, 8f);

                if(timeCount >= timeLimit + random)
                {
                    middleRotate = true;
                    targetRotation = Quaternion.Euler(0, 0, 0);
                }
            }
            else
            {
                if(timeCount >= timeLimit + 5.1f)
                {
                    finishRotate = true;
                    for (int i = 0; i < variableManager.playerCount; i++)
                    {
                        var pc = PlayerObj[i].GetComponent<PlayerController>();
                        var epc = PlayerObj[i].GetComponent<EnemyPlayerController>();

                        if (pc != null) pc.canMove = true;
                        if (epc != null) epc.canMove = true;
                    }
                }
            }

            if(finishRotate)
            {
                timeCount = 0;
                timeLimit = Random.Range(1f, 3f);
                speed = Random.Range(5f, 10f);
                finishRotate = false;
                middleRotate = false;
                variableManager.textString = " ";
                for (int i = 0; i < variableManager.playerCount; i++)
                {
                    playerController = PlayerObj[i].GetComponent<PlayerController>();
                    enemyPlayerController = PlayerObj[i].GetComponent<EnemyPlayerController>();

                    if (playerController != null) playerController.canMove = true;
                    if (enemyPlayerController != null) enemyPlayerController.canMove = true;
                }
            }
        }
    }

    bool CanSeeTarget(Transform target)
    {
        if (target == null) return false;

        Vector3 dir = target.position - transform.position;
        float distance = dir.magnitude;

        if (distance > maxDistance) return false;

        RaycastHit[] hits = Physics.RaycastAll(transform.position, dir.normalized, distance);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        Debug.Log("=== 判定開始 ===");

        foreach (var hit in hits)
        {
            if (hit.transform == target)
                return true;

            if (hit.transform.CompareTag("Transparent") || hit.transform.CompareTag("daruma"))
                continue;

            break;
        }

        return false;
    }
}