using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using Fusion;
using Fusion.Sockets;

public class VariableManager : NetworkBehaviour, INetworkRunnerCallbacks
{
    public List<GameObject> PlayerObj = new List<GameObject>();
    // [Networked] private int seed { get; set; }
    [SerializeField] Material[] materials;
    [SerializeField] GameObject[] daruma0Obj;
    [SerializeField] GameObject[] prisonObj;
    [SerializeField] GameObject[] roleObj;
    [SerializeField] Image[] imageObj;
    [SerializeField] GameObject lightObj;
    [SerializeField] GameObject playerPrefab;
    [SerializeField] TextMeshProUGUI upText;
    [SerializeField] Image roleExplain;
    [SerializeField] TextMeshProUGUI roleText;
    [SerializeField] TextMeshProUGUI buttonText;
    [SerializeField] Image CoolTimeImage;
    [SerializeField] GameObject playerNameCanvas;
    [SerializeField] TextMeshProUGUI CoolTimeText;
    [SerializeField] AudioClip missionStartAudio;
    [SerializeField] AudioClip openGoal;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioSource smallAudioSource;
    [SerializeField] AudioClip prisonBreakAudio;
    [SerializeField] private NetworkObject darumaPrefab;
    [SerializeField] private NetworkObject daruma1Prefab;
    [SerializeField] private GameObject mobileUIPanel;
    List<NetworkObject> spawnedDarumas = new List<NetworkObject>();
    public static int playerRole;
    public static int stageNumber;
    public string textString;
    public int[] gemList;
    public int playerCount;
    public bool prisonBreak;
    public static int isFinish;
    public bool isGoalOpen;
    public int missionNumber;
    public int missionSubNumber;
    public bool canMission;
    public bool clearMission;
    public float missionTimeCount;
    public float missionTimeCountLimit;
    public bool missionStart;
    GameObject[] prisonObjects;
    PlayerController[] playerControllers;
    PlayerController playerController;
    EnemyPlayerController[] enemyPlayerController;
    int[] roleArray;
    int random;
    bool previousPrisonBreak;
    bool isLeaving;
    bool audio = false;
    float canMissionTimeCount;
    float canMissionTimeLimit;
    float canMissionTimeLimit2;
    int prisonPlayerCount;
    int goalPlayerCount;
    bool hasSpawned = false;
    bool itemBool = false;
    int continuePlayerCount;
    PlayerController playerControllers2;
    NetworkRunner runner;
    List<int> roles = new List<int>();
    List<PlayerRef> players;
    int seed;
    int index;
    float spawnTimer;
    public int isSetName = 0;
    bool didCamera = false;
    bool gemBool = false;
    private bool hasSpawnedPlayer = false; // 二重実行防止用
    public bool isRoleChange;

    public override void Spawned()
    {
        upText.text = "";      
        Debug.Log($"spawn!! - InstanceID: {gameObject.GetInstanceID()}", gameObject);
        
        // ★自分の端末でロードが終わった瞬間から「1秒」のカウントダウンを開始
        spawnTimer = 1f; 
        hasSpawnedPlayer = false;
    }

    void SpawnPlayer()
    {
        gemBool = false;
        Debug.Log("spawnPlayer!!");
        NetworkRunner runner = FindFirstObjectByType<NetworkRunner>();
        if (runner == null) 
        {
            Debug.Log("runner == null");
            return;
        }

        if (runner.GetPlayerObject(runner.LocalPlayer) != null)
        {
            Debug.Log("runner.GetPlayerObject(runner.LocalPlayer) != null");
            return;
        }

        index = runner.LocalPlayer.PlayerId;
        Vector3 targetPosition = Vector3.zero;
        if(index == 0)
        {
            targetPosition = new Vector3(0, 15, 0);
        }
        if(index == 1)
        {
            targetPosition = new Vector3(0, 15, 5);
        }
        if(index == 2)
        {
            targetPosition = new Vector3(0, 15, -5);
        }
        if(index == 3)
        {
            targetPosition = new Vector3(0, 15, 10);
        }
        if(index == 4)
        {
            targetPosition = new Vector3(0, 15, -10);
        }
        if(index == 5)
        {
            targetPosition = new Vector3(5, 15, 0);
        }
        if(index == 6)
        {
            targetPosition = new Vector3(5, 15, 5);
        }
        if(index == 7)
        {
            targetPosition = new Vector3(5, 15, -5);
        }
        if(index == 8)
        {
            targetPosition = new Vector3(5, 15, 10);
        }
        if(index == 9)
        {
            targetPosition = new Vector3(5, 15, -10);
        }
        if(index == 10)
        {
            targetPosition = new Vector3(-5, 15, -5);
        }
        if(index == 11)
        {
            targetPosition = new Vector3(-5, 15, 10);
        }
        if(index == 12)
        {
            targetPosition = new Vector3(-5, 15, -10);
        }
        var obj = runner.Spawn(
            playerPrefab.GetComponent<NetworkObject>(),
            targetPosition,
            Quaternion.identity,
            runner.LocalPlayer
        );
        runner.SetPlayerObject(runner.LocalPlayer, obj);
        
        playerCount = runner.ActivePlayers.Count();
        Debug.Log(playerCount.ToString());

        if (runner.IsSharedModeMasterClient)
        {
            var myPlayer = obj.GetComponent<PlayerController>();
            if (myPlayer != null)
            {
                myPlayer.syncedSeed = Random.Range(1, 100000);
            }
        }

        StartCoroutine(WaitAndAssign(obj.GetComponent<NetworkObject>(), runner));
    }

    IEnumerator WaitAndAssign(NetworkObject netObj, NetworkRunner runner)
    {
        while (netObj != null && !netObj.HasStateAuthority)
        {
            yield return null;
        }
        
        // 1. 全員のプレイヤー生成が完全に完了するのを待つ
        while (FindObjectsByType<PlayerController>(FindObjectsSortMode.None).Length != playerCount)
        {
            yield return null;
        }

        if (runner == null) yield return null;
        if (runner.gameObject.GetComponent<StartFusion>() == null) yield return null;

        int modePlayerCount = runner.gameObject.GetComponent<StartFusion>().modePlayerCount;

        if (modePlayerCount == 1)
        {
            roles = new List<int> { 0 };
            canMissionTimeLimit = 20f;
            for(int i = 0; i < roleObj.Length; i ++)
            {
                roleObj[i].SetActive(true);
            }
        }
        if (modePlayerCount == 2)
        {
            roles = new List<int> { 0, 2 };
            canMissionTimeLimit = 20f;
            for(int i = 0; i < roleObj.Length; i ++)
            {
                roleObj[i].SetActive(true);
            }
        }
        if (modePlayerCount == 5)
        {
            roles = new List<int> { 0, 0, 0, 1, 2 };
            canMissionTimeLimit = 20f;
        }
        if (modePlayerCount == 9)
        {
            roles = new List<int> { 0, 0, 0, 0, 0, 0, 1, 1, 2 };
            canMissionTimeLimit = 16f;
        }
        if (modePlayerCount == 13)
        {
            roles = new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 2 };
            canMissionTimeLimit = 12f;
        }
        canMissionTimeLimit2 = canMissionTimeLimit;

        playerControllers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None).OrderBy(x => x.GetComponent<NetworkObject>().Id).ToArray();

        while (seed == 0)
        {
            foreach (var player in playerControllers)
            {
                if (player.syncedSeed != 0)
                {
                    seed = player.syncedSeed;
                    break;
                }
            }
            yield return null;
        }

        // 2. 全員共通のシード値で役職リストをシャッフル
        Random.InitState(seed);
        roles = roles.OrderBy(x => Random.value).ToList();

        if(playerCount == 1 || playerCount == 2 || playerCount == 5 || playerCount == 9 || playerCount == 13)
        {
            roleArray = new int[playerCount];
            for(int i = 0; i < playerCount; i++)
            {
                int finalRole = 0;
                if(roles.Count == playerCount) finalRole = roles[i];
                roleArray[i] = finalRole;

                // 名簿の「i番目」のプレイヤーが、自分自身（InputAuthority持ち）か直接判定する
                if(playerControllers[i].HasInputAuthority)
                {
                    // ★ここで後々のために自分のコントローラーを代入して保持する
                    playerController = playerControllers[i];
                    playerNameCanvas = playerController.gameObject.transform.GetChild(0).gameObject;
                    index = i;

                    // 自分の役職を代入
                    playerController.roleNumber = finalRole;
                    playerController.canMove = false;
                    
                    Debug.Log($"自分（index: {index}）に正しい役職 {finalRole} を配布し、変数に保存しました");
                }
            }
        }
        setTextString();
    }

    public void RequestSetActiveItem()
    {
        NetworkRunner runner = FindFirstObjectByType<NetworkRunner>();
        if (runner == null) return;

        var startFusion = runner.GetComponent<StartFusion>();
        var state = startFusion != null ? startFusion.State : null;

        // 送信するアイテムIDを決定
        int currentItemIndex = (state != null && state.Object != null && state.Object.IsValid) ? startFusion.nowItemID : 0;

        // 「自分のインデックス」と「自分が今持っているアイテムID」を一緒にRPCで送信
        RPC_SetActiveItem(index, currentItemIndex);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_SetActiveItem(int targetIndex, int itemIndex)
    {
        // 送られてきた itemIndex を使って特定のプレイヤーの表示を更新
        SetActiveItemForPlayer(targetIndex, itemIndex);
    }

    public void SetActiveItemForPlayer(int targetIndex, int itemIndex)
    {
        if (playerControllers == null || targetIndex < 0 || targetIndex >= playerControllers.Length) return;
        var targetPlayer = playerControllers[targetIndex];
        if (targetPlayer == null) return;

        GameObject skinObj = FindChildWithTag(targetPlayer.gameObject, "Skin");
        if (skinObj == null)
        {
            Debug.Log("skinObj = null");
            return;   
        }

        Transform itemsTransform = skinObj.transform.Find("Items");
        if (itemsTransform != null)
        {
            itemsTransform.gameObject.SetActive(true);
            string targetItemName = "Item" + itemIndex; // RPCで届いたitemIndexを使用

            foreach (Transform child in itemsTransform)
            {
                child.gameObject.SetActive(child.gameObject.name == targetItemName);
            }
        }
    }

    private GameObject FindChildWithTag(GameObject parent, string tag)
    {
        Transform[] allChildren = parent.GetComponentsInChildren<Transform>(true);

        foreach (Transform child in allChildren)
        {
            if (child.CompareTag(tag))
            {
                return child.gameObject;
            }
        }

        return null;
    }

    void SetNameText()
    {
        if (PlayerObj.Count != playerCount) return;

        NetworkPlayerInfo[] rawPlayers = FindObjectsByType<NetworkPlayerInfo>(FindObjectsSortMode.None);

        for (int i = 0; i < playerCount; i++)
        {
            if (imageObj[i].TryGetComponent(out CanvasGroup canvasGroup))
                canvasGroup.alpha = 1f;

            if (i >= PlayerObj.Count)
                continue;

            var currentController = PlayerObj[i].GetComponent<PlayerController>();
            if (currentController == null)
                continue;

            var matchedPlayerInfo = rawPlayers.FirstOrDefault(p =>
                p != null &&
                p.Object != null &&
                p.Object.IsValid &&
                !p.IsLeaving &&
                p.OwnerRef == currentController.Object.InputAuthority);

            if (matchedPlayerInfo == null)
                continue;

            // --- 1. アイコンと特殊状態のアクティブ切り替え ---
            foreach (Transform child in imageObj[i].transform)
            {
                if (child.name == "Aikon")
                    child.gameObject.SetActive(currentController.roleNumber == 0);
                else if (child.name == "Aikon (1)")
                    child.gameObject.SetActive(currentController.roleNumber == 1);
                else if (child.name == "Aikon (2)")
                    child.gameObject.SetActive(currentController.roleNumber == 2);
                else if (child.name == "Goal")
                    child.gameObject.SetActive(currentController.roleNumber == 3);
                else if (child.name == "InPrison")
                    child.gameObject.SetActive(currentController.stateNumber == 1);
            }

            // --- 2. 名前テキストの更新（非表示の親の影響を受けないよう全テキストを一括更新） ---
            TextMeshProUGUI[] nameTexts = imageObj[i].GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var textComp in nameTexts)
            {
                if (textComp.gameObject.name == "NameText")
                {
                    textComp.text = matchedPlayerInfo.PlayerName.ToString();
                }
            }
        }
    }

    void Awake()
    {
        /*
        for(int i = 0; i < playerCount; i ++)
        {
            runner.Spawn(
                playerPrefab.GetComponent<NetworkObject>(),
                Vector3.zero,
                Quaternion.identity,
                player
            );   
        }
        */
        DynamicGI.UpdateEnvironment();
        stageNumber = 1;
        Transform targetPrisonObj = prisonObj[stageNumber].transform;
        prisonObjects = new GameObject[targetPrisonObj.childCount];
        for (int i = 0; i < targetPrisonObj.childCount; i++)
        {
            prisonObjects[i] = targetPrisonObj.GetChild(i).gameObject;
        }
        if(StartButton.playerCountStatic != 5 && StartButton.playerCountStatic != 9 && StartButton.playerCountStatic != 13)
        {
            playerCount = 5;
        }
        else
        {
            playerCount = StartButton.playerCountStatic;
        }

        prisonBreak = true;
        previousPrisonBreak = true;
        isFinish = -1;
        missionNumber = -1;
        missionSubNumber = -1;
        canMission = true;
        missionStart = false;
        canMissionTimeCount = 0;
        textString = " ";
        CoolTimeImage.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        audio = false;

        /*
        #if UNITY_EDITOR
            mobileUIPanel.SetActive(true);
            return; // ここで処理を終了して、下の判定には進まない
        #endif
        */
        if (Application.isMobilePlatform)
        {
            mobileUIPanel.SetActive(true);  // スマホのブラウザなら表示
        }
        else
        {
            mobileUIPanel.SetActive(false); // PCのブラウザなら非表示
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (spawnTimer > 0f)
        {
            // Unity標準の時間経過（Time.deltaTime）を使用するので、Runnerは不要です
            spawnTimer -= Time.deltaTime;

            // 1秒が経過し、まだスポーンしていないなら実行
            if (spawnTimer <= 0f && !hasSpawnedPlayer)
            {
                hasSpawnedPlayer = true; // 二重実行防止
                SpawnPlayer();
            }
        }
        if(playerController == null) return;
        if (!playerController.isActiveAndEnabled) return;

        NetworkRunner runner = FindFirstObjectByType<NetworkRunner>();
        if (runner == null) return;
        if (runner.gameObject.GetComponent<StartFusion>() == null) return;

        if(playerController.roleNumber == 2)
        {
            playerNameCanvas.SetActive(false);
        }
        else
        {
            playerNameCanvas.SetActive(true);
        }

        if(!itemBool && FindChildWithTag(playerController.gameObject, "Skin") != null)
        {
            itemBool = true;
            RequestSetActiveItem();
        }

        if(Input.GetMouseButton(0))
        {
            didCamera = true;
            Invoke("setTextString", 2f);
        }

        if(isRoleChange)
        {
            isRoleChange = false;
            if(!didCamera)
            {
                textString = "WASDで移動\n左クリック＆ドラッグでカメラ回転";
            }
            else
            {
                setTextString();
            }
        }

        if(!playerController.canMove)
        {
            if(playerController.roleNumber == 0)
            roleText.text = "あなたは泥棒です\n1. 金庫を破壊して宝石を盗もう\n2. 捕まった仲間を牢屋から助け出そう\n3. 捕まらずにゴールから脱出しよう";
            if(playerController.roleNumber == 1)
            roleText.text = "あなたは警察です\n1. 泥棒を追いかけて捕まえよう\n2. 金庫や牢屋を見張って見つけ出そう\n3. ゴールからの脱出を阻止しよう";
            if(playerController.roleNumber == 2)
            roleText.text = "あなたはだるまさんです\n1. 神視点でみんなの動きを観察できる\n2. クリアできなさそうなミッションを発動しよう\n3. 制限時間内に誰もクリアできなかったら勝利";
        }
        if(roleExplain.gameObject.activeSelf)
        {
            int timeCount = Mathf.CeilToInt(runner.gameObject.GetComponent<StartFusion>().roleExplainTime);
            buttonText.text = "待機中(" + timeCount + "秒後に開始)";
            if(timeCount == 0) 
            {
                roleExplain.gameObject.SetActive(false);
                playerController.canMove = true;
            }
        }
        PlayerController[] controllers =
        FindObjectsByType<PlayerController>(FindObjectsSortMode.None)
        .OrderBy(c => c.Object.Id)
        .ToArray();

        PlayerObj.Clear();

        foreach (var controller in controllers)
        {
            PlayerObj.Add(controller.gameObject);
        }
        if (PlayerObj.Count != playerCount)
        {
            playerCount = runner.ActivePlayers.Count();
            return;   
        }

        isSetName ++;
        if(isSetName % 10 == 0) 
        {
            SetNameText();
        }

        upText.text = textString;
        if(missionNumber != -1) missionTimeCount += Time.deltaTime;
        canMissionTimeCount += Time.deltaTime;
        if(previousPrisonBreak != prisonBreak)
        {
            previousPrisonBreak = prisonBreak;
            if(prisonBreak)
            {
                smallAudioSource.PlayOneShot(prisonBreakAudio);
                for(int i = 1; i < 5; i ++)
                {
                    prisonObjects[i].GetComponent<Renderer>().material = materials[3];
                }
                prisonObjects[0].GetComponent<Renderer>().material = materials[2];
                prisonObjects[5].GetComponent<Renderer>().material = materials[2];
                for(int i = 0; i < 6; i ++)
                {
                    foreach (Collider c in prisonObjects[i].GetComponents<Collider>())
                    {
                        c.enabled = false;
                    }
                }
            }
            else
            {
                for(int i = 1; i < 5; i ++)
                {
                    prisonObjects[i].GetComponent<Renderer>().material = materials[1];
                }
                prisonObjects[0].GetComponent<Renderer>().material = materials[0];
                prisonObjects[5].GetComponent<Renderer>().material = materials[0];
                for(int i = 0; i < 6; i ++)
                {
                    foreach (Collider c in prisonObjects[i].GetComponents<Collider>())
                    {
                        c.enabled = true;
                    }
                }
            }   
        }

        float clampedTime = Mathf.Min(missionTimeCount, missionTimeCountLimit);
        float missionProgress = (missionTimeCountLimit > 0f) ? (clampedTime / missionTimeCountLimit) : 0f;
        float coolLimit = (canMissionTimeLimit > 0f) ? canMissionTimeLimit : 1f;
        float coolRatio = Mathf.Clamp01(canMissionTimeCount / coolLimit);

        if (playerController.roleNumber == 2)
        {
            CoolTimeImage.gameObject.SetActive(true);

            if (missionNumber != -1)
            {
                CoolTimeText.text = "ミッション\n制限時間";
                CoolTimeImage.transform.rotation = Quaternion.Euler(0f, 0f, -missionProgress * 360f);
            }
            else if (!canMission)
            {
                CoolTimeText.text = "ミッション\nクールタイム";
                CoolTimeImage.transform.rotation = Quaternion.Euler(0f, 0f, coolRatio * 360f);
                if(coolRatio == 1) canMission = true;
            }
            else
            {
                CoolTimeText.text = "ミッション\n発動可能！";
                CoolTimeImage.transform.rotation = Quaternion.identity;
            }
        }
        else
        {
            if (missionNumber != -1)
            {
                CoolTimeImage.gameObject.SetActive(true);
                CoolTimeText.text = "ミッション\n制限時間";
                CoolTimeImage.transform.rotation = Quaternion.Euler(0f, 0f, -missionProgress * 360f);
            }
            else
            {
                CoolTimeImage.gameObject.SetActive(false);
                CoolTimeImage.transform.rotation = Quaternion.identity;
            }
        }
        
        if(missionNumber != -1)
        {
            if(missionStart)
            {
                audioSource.PlayOneShot(missionStartAudio);
                clearMission = false;
                missionStart = false;
                missionTimeCount = 0;
                if(missionNumber == 0)
                {
                    if(playerCount == 1) missionTimeCountLimit = 30;
                    if(playerCount == 2) missionTimeCountLimit = 30;
                    if(playerCount == 5) missionTimeCountLimit = 25;
                    if(playerCount == 9) missionTimeCountLimit = 20;
                    if(playerCount == 13) missionTimeCountLimit = 15;
                    if (playerController.roleNumber == 2)
                    {
                        RPC_StartMission(0, missionSubNumber);
                    }
                }
                if(missionNumber == 1)
                {
                    if(playerCount == 1) missionTimeCountLimit = 50;
                    if(playerCount == 2) missionTimeCountLimit = 50;
                    if(playerCount == 5) missionTimeCountLimit = 40;
                    if(playerCount == 9) missionTimeCountLimit = 30;
                    if(playerCount == 13) missionTimeCountLimit = 20;
                    if (playerController.roleNumber == 2)
                    {
                        RPC_StartMission(1, 0);
                    }
                }
                if(missionNumber == 2)
                {
                    if(playerCount == 1) missionTimeCountLimit = 30;
                    if(playerCount == 2) missionTimeCountLimit = 50;
                    if(playerCount == 5) missionTimeCountLimit = 30;
                    if(playerCount == 9) missionTimeCountLimit = 22;
                    if(playerCount == 13) missionTimeCountLimit = 15;
                    if (playerController.roleNumber == 2)
                    {
                        RPC_StartMission(2, missionSubNumber);
                    }
                }
            }
            if(missionTimeCount >= missionTimeCountLimit)
            {
                if(playerController.roleNumber == 2)
                {
                    isFinish = 0;   
                }
                else
                {
                    isFinish = 1;
                }
            }
            if(clearMission)
            {
                RPC_ClearMission();
                RPC_SyncLight();
                missionNumber = -1;
                missionSubNumber = -1;
                canMissionTimeCount = 0;
                canMission = false;
                clearMission = false;
                setTextString();
                for (int i = 0; i < playerCount; i++)
                {
                    playerControllers2 = PlayerObj[i].GetComponent<PlayerController>();
                    if (playerControllers2 != null) playerControllers2.canMove = true;
                }
                playerControllers2 = PlayerObj[0].GetComponent<PlayerController>();
            }
        }
        if(!audio && isGoalOpen)
        {
            audioSource.PlayOneShot(openGoal);
            Debug.Log("openGoal");
            audio = true;
        }
        prisonPlayerCount = 0;
        goalPlayerCount = 0;
        continuePlayerCount = 0;

        for(int i = 0; i < playerCount; i ++)
        {
            playerControllers2 = PlayerObj[i].GetComponent<PlayerController>();
            if (playerControllers2 != null)
            {
                if(playerControllers2.roleNumber == 0 && playerControllers2.stateNumber == 1)
                {
                    prisonPlayerCount ++;
                }
                else if(PlayerObj[i].activeSelf == false)
                {
                    goalPlayerCount ++;
                }
                else
                {
                    continuePlayerCount ++;
                }
            }
        }
        if((playerCount == 5 && continuePlayerCount == 2) || (playerCount == 9 && continuePlayerCount == 3) || (playerCount == 13 && continuePlayerCount == 4))
        {
            if(playerController.roleNumber == 1)
            {
                isFinish = 2;
                runner.gameObject.GetComponent<StartFusion>().gemCount += 5;
                runner.gameObject.GetComponent<StartFusion>().SaveData();
                LeaveRoom();
                return;
            }
            else
            {
                isFinish = 1;
                runner.gameObject.GetComponent<StartFusion>().gemCount += 5;
                runner.gameObject.GetComponent<StartFusion>().SaveData();
                LeaveRoom();
                return;
            }
        }
        if((playerCount == 5 && prisonPlayerCount == 3) || (playerCount == 9 && prisonPlayerCount == 6) || (playerCount == 13 && prisonPlayerCount == 9))
        {
            if(playerController.roleNumber == 1)
            {
                isFinish = 0;
                runner.gameObject.GetComponent<StartFusion>().gemCount += 5;
                runner.gameObject.GetComponent<StartFusion>().SaveData();
                LeaveRoom();
                return;
            }
            else
            {
                isFinish = 1;
                runner.gameObject.GetComponent<StartFusion>().gemCount += 5;
                runner.gameObject.GetComponent<StartFusion>().SaveData();
                LeaveRoom();
                return;
            }
        }
        if((playerCount == 5 && goalPlayerCount == 3) || (playerCount == 9 && goalPlayerCount == 6) || (playerCount == 13 && goalPlayerCount == 9))
        {
            if(playerController.roleNumber != 0)
            {
                isFinish = 1;
                runner.gameObject.GetComponent<StartFusion>().gemCount += 5;
                runner.gameObject.GetComponent<StartFusion>().SaveData();
                LeaveRoom();
                return;
            }
        }
        if(isFinish != -1)
        {
            playerController.roleNumber = 3;
            if(!gemBool)
            {
                runner.gameObject.GetComponent<StartFusion>().gemCount += 5;
                gemBool = true;
            }
            runner.gameObject.GetComponent<StartFusion>().SaveData();
        }
    }

    async void LeaveRoom()
    {
        if (isLeaving) return;
        isLeaving = true;

        runner.gameObject.GetComponent<StartFusion>().gameStarted = false;
        runner.gameObject.GetComponent<StartFusion>().State = null;

        if (runner != null && runner.IsRunning)
        {
            await runner.Shutdown();
        }

        SceneManager.LoadScene(1);
    }

    void setTextString()
    {
        if(playerController.roleNumber == 0)
        if(playerController.stateNumber == 1) textString = "泥棒：仲間に助けてもらうまで待とう";
        else if(missionNumber != -1)
        {
            if(missionNumber == 0) textString = "泥棒：巨大なだるまが出現！タッチしに行こう";
            if(missionNumber == 1) textString = "泥棒：小さなだるまが出現！タッチしに行こう";
            if(missionNumber == 2) textString = "泥棒：誰かが呪われた！呪いを解除しに行こう";
        }
        else if(isGoalOpen) textString = "泥棒：光っているゴールから脱出しよう！";
        else textString = "泥棒：金庫を破壊して宝石を盗もう";

        if(playerController.roleNumber == 1)
        if(missionNumber != -1)
        {
            if(missionNumber == 0) textString = "警察：巨大なだるまが出現！タッチしに行こう";
            if(missionNumber == 1) textString = "警察：小さなだるまが出現！タッチしに行こう";
            if(missionNumber == 2) textString = "警察：誰かが呪われた！呪いを解除しに行こう";
        }
        else if(isGoalOpen) textString = "警察：ゴールからの脱出を阻止しよう";
        else textString = "警察：泥棒を追いかけてタッチで捕まえよう";

        if(playerController.roleNumber == 2)
        if(canMission) textString = "だるまさん：右クリック＆1～3キーで\nミッションを発動してみよう";
        else if(missionNumber != -1) textString = "だるまさん：クールタイムが終わるまで待とう";
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_StartMission(int RPCmissionNumber, int missionSubNumber)
    {
        missionTimeCount = 0;
        missionNumber = RPCmissionNumber;
        Debug.Log(missionNumber.ToString());
        // すでに古いだるまが残っていれば削除
        foreach (var d in spawnedDarumas)
        {
            if (d != null) Runner.Despawn(d);
        }
        spawnedDarumas.Clear();

        if (missionNumber == 0)
        {
            if(playerCount == 1) missionTimeCountLimit = 30;
            if(playerCount == 2) missionTimeCountLimit = 30;
            if(playerCount == 5) missionTimeCountLimit = 25;
            if(playerCount == 9) missionTimeCountLimit = 20;
            if(playerCount == 13) missionTimeCountLimit = 15;
            Vector3 spawnPos = Vector3.zero;
            Quaternion spawnRot = Quaternion.identity;

            // 東西南北の座標と向きを決定
            if (missionSubNumber == 0)
            {
                spawnPos = new Vector3(120, 20f, 0);
                spawnRot = Quaternion.Euler(0, 90, 0);
            }
            else if (missionSubNumber == 1)
            {
                spawnPos = new Vector3(-120, 20f, 0);
                spawnRot = Quaternion.Euler(0, -90, 0);
            }
            else if (missionSubNumber == 2)
            {
                spawnPos = new Vector3(0, 20f, 100);
                spawnRot = Quaternion.Euler(0, 180, 0);
            }
            else if (missionSubNumber == 3)
            {
                spawnPos = new Vector3(0, 20f, -100);
                spawnRot = Quaternion.Euler(0, -180, 0);
            }

            // 💡マスターの権限でネットワーク上にだるまを生成（これで全員の画面に同期して出現します）

            NetworkObject obj = Runner.Spawn(darumaPrefab, spawnPos, spawnRot);
            spawnedDarumas.Add(obj);
        }
        if (missionNumber == 1)
        {
            if(playerCount == 1) missionTimeCountLimit = 50;
            if(playerCount == 2) missionTimeCountLimit = 50;
            if(playerCount == 5) missionTimeCountLimit = 40;
            if(playerCount == 9) missionTimeCountLimit = 30;
            if(playerCount == 13) missionTimeCountLimit = 20;
            Vector3 spawnPos = Vector3.up * 50f;
            Quaternion spawnRot = Quaternion.identity;
            NetworkObject obj = Runner.Spawn(daruma1Prefab, spawnPos, spawnRot);
            spawnedDarumas.Add(obj);
        }
        if (missionNumber == 2)
        {
            if(playerCount == 1) missionTimeCountLimit = 30;
            if(playerCount == 2) missionTimeCountLimit = 50;
            if(playerCount == 5) missionTimeCountLimit = 30;
            if(playerCount == 9) missionTimeCountLimit = 22;
            if(playerCount == 13) missionTimeCountLimit = 15;
            playerControllers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None).OrderBy(x => x.GetComponent<NetworkObject>().Id).ToArray();
            playerControllers[missionSubNumber].isMarked = true;
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_ClearMission()
    {
        missionNumber = -1;
        clearMission = true;
        foreach (var d in spawnedDarumas)
        {
            if (d != null) Runner.Despawn(d);
        }
        spawnedDarumas.Clear();
        playerControllers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None).OrderBy(x => x.GetComponent<NetworkObject>().Id).ToArray();
        for(int i = 0; i < playerControllers.Length; i ++) playerControllers[i].isMarked = true;
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_SyncLight()
    {
        if (lightObj.GetComponent<Light>() != null)
        {
            lightObj.GetComponent<Light>().color = Color.white;
            lightObj.GetComponent<Light>().intensity = 1f;
        }
        Invoke("ResetLight", 1f);
    }

    void ResetLight()
    {
        if (lightObj.GetComponent<Light>() != null)
        {
            lightObj.GetComponent<Light>().color = Color.white;
            lightObj.GetComponent<Light>().intensity = 1f;
        }
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        isSetName = 0;
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, System.Collections.Generic.List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, System.Collections.Generic.Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, System.ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
}
