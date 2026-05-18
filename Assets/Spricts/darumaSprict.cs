using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class darumaSprict : MonoBehaviour
{
    [SerializeField] GameObject[] PlayerObj;
    [SerializeField] GameObject ManagerObj;
    [SerializeField] Transform[] targets;
    [SerializeField] float maxDistance = 80f;
    [SerializeField] GameObject lightObj;
    [SerializeField] AudioClip darumaAudio;

    VariableManager variableManager;
    PlayerController playerController;
    EnemyPlayerController enemyPlayerController;
    AudioSource audioSource;

    float timeCount;
    float timeLimit;
    float speed;
    float random;
    Quaternion firstRotation;
    Quaternion finishRotation;
    bool finishRotate;
    public bool middleRotate;
    public bool darumaSee;
    bool audio;

    Quaternion targetRotation;

    void Start()
    {
        audio = false;
        audioSource = GetComponent<AudioSource>();
        darumaSee = false;
        variableManager = ManagerObj.GetComponent<VariableManager>();
        timeLimit = Random.Range(1f, 3f);
        speed = Random.Range(5f, 10f);
        middleRotate = false;
        finishRotate = false;

        firstRotation = transform.rotation;
        finishRotation = firstRotation * Quaternion.Euler(0, 180, 0);
        targetRotation = finishRotation;

        lightObj.GetComponent<Light>().color = Color.Lerp(Color.white, Color.red, 0.3f);
        lightObj.GetComponent<Light>().intensity = 1f;
    }

    void Update()
    {
        timeCount += Time.deltaTime;
        for (int i = 0; i < variableManager.playerCount; i++)
        {
            if (PlayerObj[i] == null || targets[i] == null) continue;

            if (darumaSee && CanSeeTarget(targets[i])) // ★darumaSee中だけ判定
            {
                var pc = PlayerObj[i].GetComponent<PlayerController>();
                var epc = PlayerObj[i].GetComponent<EnemyPlayerController>();

                if (pc != null) pc.canMove = false;
                if (epc != null) epc.canMove = false;

                if (i == 0) variableManager.textString = "Cannot Move!";
            }
        }
        if (timeCount >= timeLimit && !middleRotate)
        {
            lightObj.GetComponent<Light>().color = Color.Lerp(Color.black, Color.red, 0.5f);
            lightObj.GetComponent<Light>().intensity = 5f;
        }

        if(timeCount >= timeLimit)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * speed);

            if(!middleRotate)
            {
                if(!audio)
                {
                    audioSource.PlayOneShot(darumaAudio, 0.5f);
                    audio = true;   
                }
                darumaSee = true;
                random = Random.Range(2f, 5f);
                targetRotation = finishRotation;

                if(timeCount >= timeLimit + random)
                {
                    middleRotate = true;
                    targetRotation = firstRotation;
                    lightObj.GetComponent<Light>().color = Color.Lerp(Color.white, Color.red, 0.3f);
                    lightObj.GetComponent<Light>().intensity = 1f;
                }
            }
            else
            {
                darumaSee = false;
                if(timeCount >= timeLimit + 5f)
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
                timeLimit = Random.Range(2f, 6f);
                speed = Random.Range(5f, 10f);
                finishRotate = false;
                middleRotate = false;
                audio = false;
                variableManager.textString = " ";

                for (int i = 0; i < variableManager.playerCount; i++)
                {
                    var pc = PlayerObj[i].GetComponent<PlayerController>();
                    var epc = PlayerObj[i].GetComponent<EnemyPlayerController>();

                    if (pc != null) pc.canMove = true;
                    if (epc != null) epc.canMove = true;
                }
            }
        }
    }

    bool CanSeeTarget(Transform target)
    {
        if (target == null) return false;

        Vector3 eye = transform.position + Vector3.up * 13f;
        Vector3 targetPos = target.position + Vector3.up * 1.0f;

        Vector3 dir = targetPos - eye;
        float dist = dir.magnitude;

        // 距離
        if (dist > maxDistance) return false;

        Vector3 dirN = dir.normalized;

        // ★前半球判定（これが本質）
        if (Vector3.Dot(transform.forward, dirN) < 0f)
            return false;

        // ★遮蔽チェック（1本だけ）
        RaycastHit[] hits = Physics.RaycastAll(eye, dirN, dist);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var hit in hits)
        {
            if (hit.transform.IsChildOf(target))
                return true;

            if (hit.transform.CompareTag("Transparent") || hit.transform.CompareTag("daruma"))
                continue;

            break;
        }

        return false;
    }
}