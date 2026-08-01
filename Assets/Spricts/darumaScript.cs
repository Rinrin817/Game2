using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class darumaScript : MonoBehaviour
{
    List<GameObject> PlayerObj = new List<GameObject>();
    GameObject ManagerObj;
    List<Transform> targets = new List<Transform>();
    [SerializeField] float maxDistance = 80f;
    GameObject lightObj;
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
        ManagerObj = GameObject.Find("Manager");
        lightObj = GameObject.Find("Directional Light");
        
        // ターゲット(目の位置)のキャッシュはStartで1度だけ行う
        Transform[] allTransforms = FindObjectsByType<Transform>(FindObjectsSortMode.None);
        targets.Clear();
        foreach (var t in allTransforms)
        {
            if (t.gameObject.name == "darumaTransform")
            {
                targets.Add(t);
            }
        }
        
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

        // Start時点で一度プレイヤーを取得しておく
        UpdatePlayerList();
    }

    // プレイヤーリストの更新をメソッド化
    void UpdatePlayerList()
    {
        PlayerController[] controllers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        PlayerObj.Clear();
        foreach (var controller in controllers)
        {
            if (controller != null)
            {
                PlayerObj.Add(controller.gameObject);
            }
        }
    }

    void Update()
    {
        // 毎フレーム Find するのではなく、人数が不一致の時だけリストを更新するようにして
        // インデックス(i)の順序ズレと処理負荷を防止
        if (PlayerObj.Count != variableManager.playerCount)
        {
            UpdatePlayerList();
            if (PlayerObj.Count != variableManager.playerCount) return;
        }

        timeCount += Time.deltaTime;

        // --- 視界判定・移動不可制御 ---
        for (int i = 0; i < variableManager.playerCount; i++)
        {
            // インデックスアウト防止
            if (i >= PlayerObj.Count || i >= targets.Count) continue;
            if (PlayerObj[i] == null || targets[i] == null) continue;

            if (darumaSee && CanSeeTarget(targets[i])) 
            {
                var pc = PlayerObj[i].GetComponent<PlayerController>();
                var epc = PlayerObj[i].GetComponent<EnemyPlayerController>();

                if (pc != null) pc.canMove = false;
                if (epc != null) epc.canMove = false;

                if (i == 0) variableManager.textString = "Cannot Move!";
            }
        }

        // --- 演出・回転の制御 ---
        if (timeCount >= timeLimit && !middleRotate)
        {
            lightObj.GetComponent<Light>().color = Color.Lerp(Color.black, Color.red, 0.5f);
            lightObj.GetComponent<Light>().intensity = 5f;
        }

        if (timeCount >= timeLimit)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * speed);

            if (!middleRotate)
            {
                if (!audio)
                {
                    audioSource.PlayOneShot(darumaAudio, 0.5f);
                    audio = true;   
                }

                // 【修正箇所】だるまがプレイヤー側にある程度回転し終えてからdarumaSeeをTrueにする
                // (振り向いている最中に一瞬でアウトになるのを防ぐため)
                if (Quaternion.Angle(transform.rotation, finishRotation) < 15f)
                {
                    darumaSee = true;
                }

                // 最初のフレームでrandomの時間を確定させる（毎フレームRandom.Rangeが走るのを防止）
                if (random == 0)
                {
                    random = Random.Range(2f, 5f);
                }
                targetRotation = finishRotation;

                // 監視時間が終了したら、前を向き直すフェーズへ
                if (timeCount >= timeLimit + random)
                {
                    middleRotate = true;
                    targetRotation = firstRotation;
                    lightObj.GetComponent<Light>().color = Color.Lerp(Color.white, Color.red, 0.3f);
                    lightObj.GetComponent<Light>().intensity = 1f;
                }
            }
            else
            {
                // 正面を向き直している間は見ない
                darumaSee = false;

                // 【修正箇所】時間固定(timeLimit + 5f)だと、randomが5秒近かった場合に戻る時間が足りなくなるため、
                // 前を向き直すための時間（+2秒など）を設けて判定します
                if (timeCount >= timeLimit + random + 2f)
                {
                    finishRotate = true;

                    for (int i = 0; i < variableManager.playerCount; i++)
                    {
                        if (i >= PlayerObj.Count) continue;
                        if (PlayerObj[i] == null) continue;

                        var pc = PlayerObj[i].GetComponent<PlayerController>();
                        var epc = PlayerObj[i].GetComponent<EnemyPlayerController>();

                        if (pc != null) pc.canMove = true;
                        if (epc != null) epc.canMove = true;
                    }
                }
            }

            if (finishRotate)
            {
                timeCount = 0;
                timeLimit = Random.Range(2f, 6f);
                speed = Random.Range(5f, 10f);
                random = 0; // randomをリセット
                finishRotate = false;
                middleRotate = false;
                audio = false;
                variableManager.textString = " ";

                for (int i = 0; i < variableManager.playerCount; i++)
                {
                    if (i >= PlayerObj.Count) continue;
                    if (PlayerObj[i] == null) continue;

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

        if (dist > maxDistance) return false;

        Vector3 dirN = dir.normalized;

        if (Vector3.Dot(transform.forward, dirN) < 0f)
            return false;

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