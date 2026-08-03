using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.Audio;

public class StartFusion : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] NetworkRunner runner;
    [SerializeField] GameObject skinObj;
    [SerializeField] GameObject cameraObj;
    [SerializeField] GameObject gachaCameraObj;
    [SerializeField] PlayerName playerName;
    [SerializeField] NetworkPlayerInfo playerInfoPrefab;
    [SerializeField] TMP_InputField passwordInputField;
    [SerializeField] TextMeshProUGUI modeText;
    [SerializeField] TextMeshProUGUI explainText;
    [SerializeField] TextMeshProUGUI[] gemCountText;
    [SerializeField] TextMeshProUGUI[] itemText;
    [SerializeField] Image[] itemImage;
    [SerializeField] GameObject[] thiefSkinObjs;
    [SerializeField] GameObject[] polliceSkinObjs;
    [SerializeField] NetworkObject networkGameStatePrefab;
    [SerializeField] AudioSource homeBGMSource;
    [SerializeField] AudioSource battleBGMSource;
    [SerializeField] AudioClip homeButtonAudio;
    [SerializeField] AudioClip openGacha;
    [SerializeField] AudioClip openGacha2;
    [SerializeField] AudioClip openGacha3;
    [SerializeField] AudioClip openGacha4;
    [SerializeField] AudioClip openAudio;
    [SerializeField] AudioClip closeAudio;
    [SerializeField] AudioClip typingAudio;
    [SerializeField] Image[] ONOFFImage;
    [SerializeField] Image NoticeImage;
    [SerializeField] Canvas[] CanvasObjs;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] GameObject[] ItemObjs;
    [SerializeField] GameObject[] ItemButton;
    [SerializeField] Transform buttonContainer;
    public int nowItemID;
    Button Stage1Button;
    Button Stage2Button;
    GameObject StageChangeImage;
    [SerializeField] AudioSource audioSource;
    Text roomText;
    public int modePlayerCount;
    int lastGachaCount = 0;
    public List<int> Items = new List<int>();
    public bool isSE;
    public bool isBGM;
    public float XsensivitySetting;
    public float YsensivitySetting;
    public int gemCount;
    [SerializeField] TextMeshProUGUI startText;
    bool isLeaving = false;
    float lastPropertyUpdateTime;
    bool gachaFinish;
    public NetworkGameState State { get; set; }
    public int stageNumber;
    Transform childTransform;
    GameObject parentObj;

    public int thiefSkinNumber
    {
        get => State.thiefSkinNumber;
        set => State.thiefSkinNumber = value;
    }

    public int polliceSkinNumber
    {
        get => State.polliceSkinNumber;
        set => State.polliceSkinNumber = value;
    }

    public int skinObjNumber
    {
        get => State.skinObjNumber;
        set => State.skinObjNumber = value;
    }

    public float roleExplainTime
    {
        get => State.roleExplainTime;
        set => State.roleExplainTime = value;
    }

    public NetworkString<_16> PlayerName
    {
        get => State.PlayerName;
        set => State.PlayerName = value;
    }
    public int modeNumber;
    string modeString;
    public bool gameStarted;

    void Awake()
    {
        stageNumber = 1;
        DontDestroyOnLoad(gameObject);
        runner.AddCallbacks(this);
        homeBGMSource.Play();
        SetupItemButtons();
        LoadData();
        for(int i = 0; i < 38; i ++)
        {
            TextMeshProUGUI itemText = ItemButton[i].gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            itemText.text = ItemResult(i);
        }
    }
    
    public async void StartGame()
    {
        isLeaving = false;
        startText.text = "参加中...";
        PlayerData.PlayerName = playerName.GetPlayerName();

        if (runner.IsRunning && runner.SessionInfo.IsValid)
        {
            Debug.Log("すでにルームに参加中のため、StartGameをスキップします。");
            return; 
        }

        if (!runner.IsRunning)
        {
            if(modeNumber == 0) modeString = "1Play_";
            if(modeNumber == 1) modeString = "5Battle_";
            if(modeNumber == 2) modeString = "9Battle_";
            if(modeNumber == 3) modeString = "13Battle_";
            if(modeNumber == 4) modeString = "2Play_";

            string roomName = modeString + "DefaultRoom";
            if(passwordInputField.text != "") roomName = modeString + passwordInputField.text;

            var args = new StartGameArgs()
            {
                GameMode = GameMode.Shared,
                SessionName = roomName,
                SceneManager = runner.GetComponent<NetworkSceneManagerDefault>(),
                PlayerCount = 20,

                SessionProperties = new Dictionary<string, SessionProperty>()
                {
                    { "GameStartedProp", 0 }, // 初期値 0 で事前登録
                    { "RoleTimerProp", 0 }     // 初期値 0 で事前登録
                }
            };
            var result = await runner.StartGame(args);
            Debug.Log(result.Ok);
            Debug.Log(result.ShutdownReason);
            Debug.Log(result.ErrorMessage);
        }

        if (runner.IsSharedModeMasterClient)
        {
            State = runner
                .Spawn(networkGameStatePrefab, Vector3.zero, Quaternion.identity)
                .GetComponent<NetworkGameState>();
            Debug.Log(State != null);

            runner.LoadScene(SceneRef.FromIndex(2));
        }
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("Join");
        if (roomText != null) roomText.text = runner.ActivePlayers.Count() + "/" + modePlayerCount.ToString();
        if (player == runner.LocalPlayer)
        {
            runner.Spawn(playerInfoPrefab, Vector3.zero, Quaternion.identity, player);
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (roomText != null) roomText.text = runner.ActivePlayers.Count() + "/" + modePlayerCount.ToString();
    }

    void Update()
    {
        if (isLeaving) return;
        float volume = isSE ? 0f : -80f;
        audioMixer.SetFloat("SEVolume", volume);
        volume = isBGM ? 0f : -80f;
        audioMixer.SetFloat("BGMVolume", volume);
        ONOFFImage[0].gameObject.SetActive(isSE);
        ONOFFImage[1].gameObject.SetActive(!isSE);
        ONOFFImage[2].gameObject.SetActive(isBGM);
        ONOFFImage[3].gameObject.SetActive(!isBGM);
        if(!gameStarted)
        {
            float mouseX = Input.GetAxis("Mouse X") * -300f * Time.deltaTime;
            if(Input.GetMouseButton(0) && skinObj != null) skinObj.transform.rotation *=  Quaternion.Euler(0, mouseX, 0);
        }
        if(Input.GetKeyDown(KeyCode.A) && Input.GetKey(KeyCode.B) && Input.GetKey(KeyCode.Space))
        {
            for(int i = 0; i < 38; i ++)
            {
                Items.Add(i);
                if(!ItemButton[i].activeSelf) ItemButton[i].SetActive(true);
                SaveData();
            }
        }
        if (State == null && runner != null && runner.IsRunning)
        {
            // シーン内に引っ越してきた NetworkGameState を探す
            NetworkGameState foundState = FindFirstObjectByType<NetworkGameState>();
            
            // Unityの古いバージョンを使っている場合は、以下を試してください
            // NetworkGameState foundState = FindObjectOfType<NetworkGameState>();

            Debug.Log("foundState = " + (foundState != null).ToString());
            if (foundState != null)
            {
                State = foundState;
                Debug.Log("シーン遷移後に NetworkGameState を無事に見つけ出しました！");
            }
        }
        if (runner != null && runner.IsRunning)
        {
            if (!runner.IsSharedModeMasterClient)
            {
                if (runner.SessionInfo.Properties.TryGetValue("GameStartedProp", out var startedProp))
                {
                    gameStarted = ((int)startedProp == 1);
                }
                if (runner.SessionInfo.Properties.TryGetValue("RoleTimerProp", out var timerProp))
                {
                    if (State != null && State.Object != null && State.Object.IsValid)
                    {
                        roleExplainTime = (float)timerProp;
                    }
                }
            }
        }

        if (gameStarted && State != null && State.Object != null && State.Object.IsValid)
        {
            if (runner != null && runner.IsRunning && runner.IsSharedModeMasterClient)
            {
                if(roleExplainTime > -0.1f)
                {
                    roleExplainTime -= Time.deltaTime;
                    if (Time.time - lastPropertyUpdateTime > 0.5f)
                    {
                        lastPropertyUpdateTime = Time.time;

                        var props = new Dictionary<string, SessionProperty>();
                        props["GameStartedProp"] = 1;
                        props["RoleTimerProp"] = (SessionProperty)roleExplainTime;
                        runner.SessionInfo.UpdateCustomProperties(props);
                    }
                }
            }

            if(modeNumber == 0)
            {
                if(GameObject.Find("RoomText") != null) GameObject.Find("RoomText").GetComponent<Text>().text = "1/1";
            }
            //return;
        }
        if(GameObject.Find("StageChange") != null)
        {
            GameObject.Find("StageChange").GetComponent<Button>().onClick.RemoveAllListeners();
            GameObject.Find("StageChange").GetComponent<Button>().onClick.AddListener(ChangeStage);
        }
        if(GameObject.Find("BaseCanvas") != null && Stage1Button == null)
        {
            Debug.Log("try");
            parentObj = GameObject.Find("BaseCanvas");
            childTransform = parentObj.transform.Find("StageChangeImage");
            StageChangeImage = childTransform.gameObject;
            childTransform = StageChangeImage.transform.Find("Stage1");
            Stage1Button = childTransform.gameObject.GetComponent<Button>();
            Stage1Button.onClick.RemoveAllListeners();
            Stage1Button.onClick.AddListener(ChangeToStage1);
            childTransform = StageChangeImage.transform.Find("Stage2");
            Stage2Button = childTransform.gameObject.GetComponent<Button>();
            Stage2Button.onClick.RemoveAllListeners();
            Stage2Button.onClick.AddListener(ChangeToStage2);
        }

        // Debug.Log(State != null && State.Object != null && State.Object.IsValid);
        /*
        if (!gameStarted && runner != null && runner.ActivePlayers != null && runner.IsSharedModeMasterClient && runner.ActivePlayers.Count() >= modePlayerCount && State != null && State.Object != null && State.Object.IsValid)
        {
            Debug.Log("start");
            gameStarted = true;
            
            var props = new Dictionary<string, SessionProperty>();
            props["GameStartedProp"] = 1;
            props["RoleTimerProp"] = (SessionProperty)roleExplainTime; // 修正
            runner.SessionInfo.UpdateCustomProperties(props);

            Invoke("AllPlayerStartGame", 2f);
        }
        */
        if(cameraObj != null) cameraObj.transform.rotation *= Quaternion.Euler(0f, 0.05f, 0f);
        if (modeText != null && explainText != null)
        {
            for(int i = 0; i < gemCountText.Length; i ++) gemCountText[i].text = gemCount.ToString();
            if(modeNumber == 0)
            {
                modeText.text = "1人プレイ";
                explainText.text = "泥棒：1 警察：0 だるまさん：0\n練習用モード\nステージをよく見たい方におすすめ";
                modePlayerCount = 1;
            }
            if(modeNumber == 1)
            {
                modeText.text = "5人バトル";
                explainText.text = "泥棒：3 警察：1 だるまさん：1\nカジュアルなモード\n初心者の方におすすめ";
                modePlayerCount = 5;
            }
            if(modeNumber == 2)
            {
                modeText.text = "9人バトル";
                explainText.text = "泥棒：6 警察：2 だるまさん：1\nチームワークが大事なモード\n慣れてきた方におすすめ";
                modePlayerCount = 9;
            }
            if(modeNumber == 3)
            {
                modeText.text = "13人バトル";
                explainText.text = "泥棒：9 警察：3 だるまさん：1\n人がたくさんいるモード\nわちゃわちゃ好きにおすすめ";
                modePlayerCount = 13;
            }
            if(modeNumber == 4)
            {
                modeText.text = "2人プレイ";
                explainText.text = "泥棒：1 警察：0 だるまさん：1\nデバッグ用モード\n開発者におすすめ";
                modePlayerCount = 2;
            }   
        }

        if (runner == null || !runner.IsRunning || runner.SessionInfo == null) return;

        // ★ マスターだけが人数を確認し、1回だけ開始処理へと進む
        if (!gameStarted && runner.IsSharedModeMasterClient)
        {
            if (runner.ActivePlayers.Count() >= modePlayerCount && State != null && State.Object != null && State.Object.IsValid)
            {
                gameStarted = true; // 次のフレームからこの if 文に入らないように即座にロック

                var props = new Dictionary<string, SessionProperty>();
                props["GameStartedProp"] = 1;
                props["RoleTimerProp"] = (SessionProperty)roleExplainTime;
                runner.SessionInfo.UpdateCustomProperties(props);

                Debug.Log("Master: 条件達成。2秒後にAllPlayerStartGameを実行します。");
                
                // 連打を防ぐため、Invokeで一呼吸置いてから実行します
                Invoke("AllPlayerStartGame", 5f);
            }
        }

        /*
        if (!gameStarted && runner != null && runner.ActivePlayers != null && !runner.IsSharedModeMasterClient && runner.ActivePlayers.Count() >= modePlayerCount && State != null && State.Object != null && State.Object.IsValid)
        {
            gameStarted = true;
            Invoke("AllPlayerStartGame", 2f);
        }
        */

        if(roomText == null && SceneManager.GetActiveScene().name == "Match")
        {
            roomText = GameObject.Find("RoomText").GetComponent<Text>();
        }
    }

    void SetupItemButtons()
    {
        audioSource.PlayOneShot(homeButtonAudio, 0.5f);
        Button[] buttons = buttonContainer.GetComponentsInChildren<Button>(true);

        for (int i = 0; i < buttons.Length; i++)
        {
            ItemButton[i] = buttons[i].gameObject;
            int buttonID = i;
            // ① OnClickイベントをスクリプトから自動登録
            buttons[i].onClick.RemoveAllListeners(); // 重複防止
            buttons[i].onClick.AddListener(() => SetItem(buttonID));
            // ② ボタンの子要素にあるテキストを取得して文字を変更
            TextMeshProUGUI text = buttons[i].GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = buttonID.ToString();
            }
        }
    }

    void AllPlayerStartGame()
    {
        DecideStageByMajority();

        runner.LoadScene(SceneRef.FromIndex(1));
        homeBGMSource.Stop();
        battleBGMSource.Play();
    }

    public void DecideStageByMajority()
    {
        NetworkPlayerInfo[] allPlayers = FindObjectsByType<NetworkPlayerInfo>(FindObjectsSortMode.None);
        if (allPlayers.Length == 0) return;

        if (!allPlayers[0].Runner.IsSharedModeMasterClient)
        {
            Debug.LogWarning("マスタークライアントのみがステージを決定できます。");
            return;
        }
        Dictionary<int, int> stageVotes = new Dictionary<int, int>();
        foreach (var player in allPlayers)
        {
            int vote = player.desiredStage;

            if (stageVotes.ContainsKey(vote))
            {
                stageVotes[vote]++;
            }
            else
            {
                stageVotes[vote] = 1;
            }
        }
        int winningStage = stageVotes.OrderByDescending(x => x.Value).First().Key;
        NetworkPlayerInfo myPlayer = allPlayers.FirstOrDefault(p => p.HasStateAuthority);
        
        if (myPlayer != null)
        {
            myPlayer.RPC_SyncStageNumber(winningStage);
        }
        else
        {
            Debug.LogError("自分の NetworkPlayerInfo が見つかりませんでした。");
        }
    }

    public void ChangeMode(bool isRight)
    {
        audioSource.PlayOneShot(homeButtonAudio, 0.5f);
        if(isRight)
        {
            if(modeNumber == 4) modeNumber = 0;
            else modeNumber ++;
        }
        else
        {
            if(modeNumber == 0) modeNumber = 4;
            else modeNumber --;
        }
    }

    public void ChangeThiefSkin()
    {
        audioSource.PlayOneShot(homeButtonAudio, 0.5f);
        if(thiefSkinNumber == 1) thiefSkinNumber = 0;
        else thiefSkinNumber ++;
        if (skinObj != null)
        {
            skinObj.SetActive(false);
        }
        skinObj = thiefSkinObjs[thiefSkinNumber];
        skinObjNumber = thiefSkinNumber;
        skinObj.SetActive(true);
    }

    public void Gacha()
    {
        NoticeImage.gameObject.SetActive(false);
        CanvasObjs[2].gameObject.SetActive(false);
        CanvasObjs[3].gameObject.SetActive(false);
        if(gachaCameraObj.gameObject.activeSelf)
        {
            cameraObj.gameObject.SetActive(true);
            gachaCameraObj.gameObject.SetActive(false);
            CanvasObjs[0].gameObject.SetActive(true);
            CanvasObjs[1].gameObject.SetActive(false);
            audioSource.PlayOneShot(closeAudio, 0.5f);
        }
        else
        {
            cameraObj.gameObject.SetActive(false);
            gachaCameraObj.SetActive(true);
            CanvasObjs[0].gameObject.SetActive(false);
            CanvasObjs[1].gameObject.SetActive(true);
            audioSource.PlayOneShot(openAudio, 0.5f);
        }
    }

    public void TypingCharacter(TMP_InputField inputField)
    {
        if (inputField != null && !string.IsNullOrWhiteSpace(inputField.text))
        {
            audioSource.pitch = Random.Range(0.7f, 1.3f);
            audioSource.PlayOneShot(typingAudio);
            audioSource.pitch = 1f;
        }
    }

    int GachaResult()
    {
        int itemNumber = Random.Range(0, 10000);

        // コモン 15個 (0～14 / 各 3% [300] / 計 45%)
        if (itemNumber < 300) return 0;
        if (itemNumber < 600) return 1;
        if (itemNumber < 900) return 2;
        if (itemNumber < 1200) return 3;
        if (itemNumber < 1500) return 4;
        if (itemNumber < 1800) return 5;
        if (itemNumber < 2100) return 6;
        if (itemNumber < 2400) return 7;
        if (itemNumber < 2700) return 8;
        if (itemNumber < 3000) return 9;
        if (itemNumber < 3300) return 10;
        if (itemNumber < 3600) return 11;
        if (itemNumber < 3900) return 12;
        if (itemNumber < 4200) return 13;
        if (itemNumber < 4500) return 14;

        // レア 8個 (15～22 / 各 3.9% [390] / 計 31.2%)
        if (itemNumber < 4890) return 15;
        if (itemNumber < 5280) return 16;
        if (itemNumber < 5670) return 17;
        if (itemNumber < 6060) return 18;
        if (itemNumber < 6450) return 19;
        if (itemNumber < 6840) return 20;
        if (itemNumber < 7230) return 21;
        if (itemNumber < 7620) return 22;

        // スーパーレア 5個 (23～27 / 各 4.15% [415] / 計 20.75%)
        if (itemNumber < 8035) return 23;
        if (itemNumber < 8450) return 24;
        if (itemNumber < 8865) return 25;
        if (itemNumber < 9280) return 26;
        if (itemNumber < 9695) return 27;

        // ウルトラレア 5個 (28～32 / 各 0.6% [60] / 計 3%)
        if (itemNumber < 9755) return 28;
        if (itemNumber < 9815) return 29;
        if (itemNumber < 9875) return 30;
        if (itemNumber < 9935) return 31;
        if (itemNumber < 9995) return 32;

        // レジェンド 5個 (33～37 / 各 0.01% [1] / 計 0.05%)
        if (itemNumber < 9996) return 33;
        if (itemNumber < 9997) return 34;
        if (itemNumber < 9998) return 35;
        if (itemNumber < 9999) return 36;
        if (itemNumber < 10000) return 37;

        return 100;
    }

    string ItemResult(int itemNumberToString)
    {
        // コモン 15個 (0～14 / 各 3% [300] / 計 45%)
        if (itemNumberToString == 0) return "マント（赤）";
        if (itemNumberToString == 1) return "マント（青）";
        if (itemNumberToString == 2) return "マント（黄）";
        if (itemNumberToString == 3) return "マント（桃）";
        if (itemNumberToString == 4) return "マント（緑）";
        if (itemNumberToString == 5) return "猫耳（赤）";
        if (itemNumberToString == 6) return "猫耳（青）";
        if (itemNumberToString == 7) return "猫耳（黄）";
        if (itemNumberToString == 8) return "猫耳（桃）";
        if (itemNumberToString == 9) return "猫耳（緑）";
        if (itemNumberToString == 10) return "傘（赤）";
        if (itemNumberToString == 11) return "傘（青）";
        if (itemNumberToString == 12) return "傘（黄）";
        if (itemNumberToString == 13) return "傘（桃）";
        if (itemNumberToString == 14) return "傘（緑）";

        // レア 8個 (15～22 / 各 3.9% [390] / 計 31.2%)
        if (itemNumberToString == 15) return "マント（ダークカラフル）";
        if (itemNumberToString == 16) return "マント（ポップカラフル）";
        if (itemNumberToString == 17) return "猫耳（ダークカラフル）";
        if (itemNumberToString == 18) return "猫耳（ポップカラフル）";
        if (itemNumberToString == 19) return "猫耳（赤＆黄）";
        if (itemNumberToString == 20) return "猫耳（青＆桃）";
        if (itemNumberToString == 21) return "傘（ダークカラフル）";
        if (itemNumberToString == 22) return "傘（ポップカラフル）";

        // スーパーレア 5個 (23～27 / 各 4.15% [415] / 計 20.75%)
        if (itemNumberToString == 23) return "ステッキ（赤）";
        if (itemNumberToString == 24) return "ステッキ（青）";
        if (itemNumberToString == 25) return "剣（赤）";
        if (itemNumberToString == 26) return "剣（青）";
        if (itemNumberToString == 27) return "剣（黄）";
        

        // ウルトラレア 5個 (28～32 / 各 0.6% [60] / 計 3%)
        if (itemNumberToString == 28) return "ステッキ（カラフル）";
        if (itemNumberToString == 29) return "ステッキ（ダーク）";
        if (itemNumberToString == 30) return "剣（ダークカラフル）";
        if (itemNumberToString == 31) return "剣（ポップカラフル）";
        if (itemNumberToString == 32) return "剣（ダーク）";

        // レジェンド 5個 (33～37 / 各 0.01% [1] / 計 0.05%)
        if (itemNumberToString == 33) return "羽（カラフル）";
        if (itemNumberToString == 34) return "羽（ダークブルー）";
        if (itemNumberToString == 35) return "羽（白黒）";
        if (itemNumberToString == 36) return "羽（クリスタル）";
        if (itemNumberToString == 37) return "羽（ダーク）";

        return "error";
    }
    
    public void OneOpen()
    {
        audioSource.PlayOneShot(openGacha, 0.3f);
        if (gemCount < 1) return;
        lastGachaCount = 1;
        int resultNumber = GachaResult();
        Items.Add(resultNumber);
        if(!ItemButton[resultNumber].activeSelf) ItemButton[resultNumber].SetActive(true);
        itemImage[0].gameObject.SetActive(true);
        SetRarityText(0, resultNumber);
        gemCount -= 1;
        TriggerGachaBox();
        SaveData();
        Invoke(nameof(OpenGachaResult), 1.5f);
    }

    public void TenOpen()
    {
        audioSource.PlayOneShot(openGacha, 0.3f);
        if (gemCount < 10) return;
        lastGachaCount = 10;
        for (int i = 0; i < 10; i++)
        {
            int resultNumber = GachaResult();
            Items.Add(resultNumber);
            if(!ItemButton[resultNumber].activeSelf) ItemButton[resultNumber].SetActive(true);
            itemImage[i + 1].gameObject.SetActive(true);
            if (resultNumber < 15) itemText[((i + 1) * 2)].text = "コモン";         // 0～14 (各3%)
            else if (resultNumber < 23) itemText[((i + 1) * 2)].text = "レア";           // 15～22 (各3.9%)
            else if (resultNumber < 28) itemText[((i + 1) * 2)].text = "スーパーレア";   // 23～27 (各4.15%)
            else if (resultNumber < 33) itemText[((i + 1) * 2)].text = "ウルトラレア";   // 28～32 (各0.6%)
            else itemText[((i + 1) * 2)].text = "レジェンド";     // 33～37 (各0.01%)
            itemText[((i + 1) * 2) + 1].text = ItemResult(resultNumber);
        }
        gemCount -= 10;
        TriggerGachaBox();
        SaveData();
        Invoke(nameof(OpenGachaResult), 1.5f);
    }

    void SetRarityText(int index, int resultNumber)
    {
        int textIndex = (index == 0 && lastGachaCount == 1) ? 1 : ((index + 1) * 2);
        if (resultNumber < 15) itemText[textIndex].text = "コモン";         // 0～14 (各3%)
        else if (resultNumber < 23) itemText[textIndex].text = "レア";           // 15～22 (各3.9%)
        else if (resultNumber < 28) itemText[textIndex].text = "スーパーレア";   // 23～27 (各4.15%)
        else if (resultNumber < 33) itemText[textIndex].text = "ウルトラレア";   // 28～32 (各0.6%)
        else itemText[textIndex].text = "レジェンド";     // 33～37 (各0.01%)
        
        int numberTextIndex = (index == 0 && lastGachaCount == 1) ? 2 : (textIndex + 1);
        itemText[numberTextIndex].text = ItemResult(resultNumber);
    }

    void TriggerGachaBox()
    {
        var gachaBox = FindFirstObjectByType<StrongBox2>();
        if (gachaBox != null) gachaBox.action = true;
        CanvasObjs[1].gameObject.SetActive(false);
    }

    void OpenGachaResult()
    {
        for (int i = 0; i < itemImage.Length; i++) itemImage[i].gameObject.SetActive(false);
        CanvasObjs[2].gameObject.SetActive(true);
        
        // baseDelay(0.1f) は使わず、コルーチン内でレア度ごとのウェイトを計算します
        StartCoroutine(ShowImagesSequentiallyRoutine(lastGachaCount));
    }

    System.Collections.IEnumerator ShowImagesSequentiallyRoutine(int count)
    {
        gachaFinish = false;
        // 単発(1回)のときは index 0 を表示
        if (count == 1)
        {
            if (itemImage.Length > 0)
            {
                // 単発の場合もレア度に応じた溜めを入れてから表示したい場合
                int itemResultIndex = Items.Count - 1;
                float delay = 0.1f;
                if (itemResultIndex >= 0 && itemResultIndex < Items.Count)
                {
                    int resultNumber = Items[itemResultIndex];
                    if (resultNumber < 15)       delay = 0.05f; // コモン
                    else if (resultNumber < 23)  delay = 0.10f; // レア
                    else if (resultNumber < 28)  delay = 0.4f; // スーパーレア
                    else if (resultNumber < 33)  delay = 1.00f; // ウルトラレア
                    else                         delay = 2.00f; // レジェンド
                }

                yield return new WaitForSeconds(delay); // 表示前の溜め
                itemImage[0].gameObject.SetActive(true);
                audioSource.PlayOneShot(openGacha);
                if(delay >= 0.4f) audioSource.PlayOneShot(openGacha2);
                if(delay >= 1f) audioSource.PlayOneShot(openGacha3);
                if(delay >= 2f) audioSource.PlayOneShot(openGacha4);
            }
            gachaFinish = true;
        }
        // 10連のときは index 1 ～ 10 の10枚をレア度に応じた速度で順に表示
        else
        {
            for (int i = 0; i < count; i++)
            {
                int targetIndex = i + 1;
                if (targetIndex < itemImage.Length)
                {
                    // 直近で獲得したアイテムのリストからレア度を判別
                    int itemResultIndex = Items.Count - count + i;
                    float delay = 0.1f; // デフォルト（コモン用）

                    if (itemResultIndex >= 0 && itemResultIndex < Items.Count)
                    {
                        int resultNumber = Items[itemResultIndex];

                        // レア度が高くなるにつれて待ち時間を長く設定
                        if (resultNumber < 15)       delay = 0.05f; // コモン
                        else if (resultNumber < 23)  delay = 0.10f; // レア
                        else if (resultNumber < 28)  delay = 0.4f; // スーパーレア
                        else if (resultNumber < 33)  delay = 1.00f; // ウルトラレア
                        else                         delay = 2.00f; // レジェンド
                    }

                    // ① 表示前の溜め（ここでレア度が高いほどじらされる）
                    yield return new WaitForSeconds(delay);

                    // ② カード/画像を表示
                    itemImage[targetIndex].gameObject.SetActive(true);
                    audioSource.PlayOneShot(openGacha);
                    if(delay >= 0.4f) audioSource.PlayOneShot(openGacha2);
                    if(delay >= 1f) audioSource.PlayOneShot(openGacha3);
                    if(delay >= 2f) audioSource.PlayOneShot(openGacha4);

                    // ③ 表示後の余韻（表示を確認させる時間）
                    yield return new WaitForSeconds(delay * 0.7f);
                }
            }
            gachaFinish = true;
        }
    }

    public void ChangeStage()
    {
        audioSource.PlayOneShot(homeButtonAudio, 0.5f);
        childTransform = StageChangeImage.transform.Find("Stage1");
        if(StageChangeImage.activeSelf) StageChangeImage.SetActive(false);
        else StageChangeImage.SetActive(true);
    }

    public void ChangeToStage1()
    {
        audioSource.PlayOneShot(homeButtonAudio, 0.5f);
        NetworkPlayerInfo[] allPlayers = FindObjectsByType<NetworkPlayerInfo>(FindObjectsSortMode.None);
        foreach (var player in allPlayers)
        {
            if (player.HasStateAuthority)
            {
                player.desiredStage = 1;
            }
        }

        GameObject Stage1Obj = null;
        GameObject Stage2Obj = null;   
        GameObject allStagesObj = GameObject.Find("AllStages");
        childTransform = allStagesObj.transform.Find("Stage1");
        Stage1Obj = childTransform.gameObject;
        childTransform = allStagesObj.transform.Find("Stage2");
        Stage2Obj = childTransform.gameObject;
        Stage1Obj.SetActive(true);
        Stage2Obj.SetActive(false);
        childTransform = GameObject.Find("BaseCanvas").transform.Find("StageChangeImage");
        parentObj = childTransform.gameObject;
        childTransform = parentObj.transform.Find("Stage1Image");
        GameObject Stage1Image = childTransform.gameObject;
        Stage1Image.SetActive(true);
        childTransform = parentObj.transform.Find("Stage2Image");
        GameObject Stage2Image = childTransform.gameObject;
        Stage2Image.SetActive(false);
    }

    public void ChangeToStage2()
    {
        audioSource.PlayOneShot(homeButtonAudio, 0.5f);
        NetworkPlayerInfo[] allPlayers = FindObjectsByType<NetworkPlayerInfo>(FindObjectsSortMode.None);
        foreach (var player in allPlayers)
        {
            if (player.HasStateAuthority)
            {
                player.desiredStage = 2;
            }
        }

        GameObject Stage1Obj = null;
        GameObject Stage2Obj = null;   
        GameObject allStagesObj = GameObject.Find("AllStages");
        childTransform = allStagesObj.transform.Find("Stage1");
        Stage1Obj = childTransform.gameObject;
        childTransform = allStagesObj.transform.Find("Stage2");
        Stage2Obj = childTransform.gameObject;
        Stage1Obj.SetActive(false);
        Stage2Obj.SetActive(true);  
        childTransform = GameObject.Find("BaseCanvas").transform.Find("StageChangeImage");
        parentObj = childTransform.gameObject;
        childTransform = parentObj.transform.Find("Stage1Image");
        GameObject Stage1Image = childTransform.gameObject;
        Stage1Image.SetActive(false);
        childTransform = parentObj.transform.Find("Stage2Image");
        GameObject Stage2Image = childTransform.gameObject;
        Stage2Image.SetActive(true);
    }

    public void CloseGachaResult()
    {
        audioSource.PlayOneShot(closeAudio, 0.5f);
        if(gachaFinish)
        {
            CanvasObjs[1].gameObject.SetActive(true);
            CanvasObjs[2].gameObject.SetActive(false);
            var gachaBox = FindFirstObjectByType<StrongBox2>();
            if (gachaBox != null)
            {
                gachaBox.action = false;
                gachaBox.isOpen = false;
                gachaBox.cooldownTimer = 0f;
            }
            for (int i = 0; i < itemImage.Length; i++) itemImage[i].gameObject.SetActive(false);   
        }
    }

    public void ChangePolliceSkin()
    {
        audioSource.PlayOneShot(homeButtonAudio, 0.5f);
        if(polliceSkinNumber == 1) polliceSkinNumber = 0;
        else polliceSkinNumber ++;
        if (skinObj != null)
        {
            skinObj.SetActive(false);
        }
        skinObj = polliceSkinObjs[polliceSkinNumber];
        skinObjNumber = 2 + polliceSkinNumber;
        skinObj.SetActive(true);
    }

    public void OpenNotice()
    {
        CanvasObjs[2].gameObject.SetActive(false);
        CanvasObjs[3].gameObject.SetActive(false);
        if(NoticeImage.gameObject.activeSelf)
        {
            NoticeImage.gameObject.SetActive(false);
            audioSource.PlayOneShot(closeAudio, 0.5f);
        }
        else
        {
            NoticeImage.gameObject.SetActive(true);
            audioSource.PlayOneShot(openAudio, 0.5f);
        }
    }

    public void OpenRocker()
    {
        audioSource.PlayOneShot(homeButtonAudio, 0.5f);
        NoticeImage.gameObject.SetActive(false);
        CanvasObjs[2].gameObject.SetActive(false);
        if(CanvasObjs[3].gameObject.activeSelf)
        {
            CanvasObjs[3].gameObject.SetActive(false);
            audioSource.PlayOneShot(closeAudio, 0.5f);
        }
        else
        {
            CanvasObjs[3].gameObject.SetActive(true);
            audioSource.PlayOneShot(openAudio, 0.5f);
        }
    }

    public void SetItem(int buttonID)
    {
        if (nowItemID == buttonID)
        {
            audioSource.PlayOneShot(closeAudio, 0.5f);
            bool isActive = ItemObjs[buttonID].activeSelf;
            ItemObjs[buttonID].SetActive(!isActive);
            if (isActive) nowItemID = -1;
        }
        else
        {
            audioSource.PlayOneShot(homeButtonAudio, 0.5f);
            if (nowItemID != -1) ItemObjs[nowItemID].SetActive(false);
            ItemObjs[buttonID].SetActive(true);
            nowItemID = buttonID;
        }
    }

    private void ResetAndLoadTitle()
    {
        gameStarted = false;
        State = null;

        SceneManager.LoadScene("Home");
    }

    private void OnEnable()
    {
        // シーンが読み込まれたときのイベントを登録
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // イベントの登録解除（メモリリーク防止）
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameObject leaveBtnObj = GameObject.Find("LeaveButton");

        if (leaveBtnObj != null)
        {
            Button leaveButton = leaveBtnObj.GetComponent<Button>();
            if (leaveButton != null)
            {
                // 二重登録を防ぐため、一度クリアしてから登録
                leaveButton.onClick.RemoveAllListeners();
                leaveButton.onClick.AddListener(LeaveRoom);
                
                Debug.Log("退出ボタンに LeaveRoom を動的登録しました！");
            }
        }
    }

    public async void LeaveRoom()
    {
        audioSource.PlayOneShot(closeAudio, 0.5f);
        if (isLeaving) return;
        isLeaving = true;

        gameStarted = false;
        State = null;

        if (runner != null && runner.IsRunning)
        {
            await runner.Shutdown();
        }

        SceneManager.LoadScene(0);
    }

    public void SaveData()
    {
        PlayerPrefs.SetInt("GemCount", gemCount);
        // 所持アイテムリスト(List<int>)をカンマ区切り文字列にして保存
        string itemsString = string.Join(",", Items);
        PlayerPrefs.SetString("SavedItems", itemsString);

        PlayerPrefs.Save();
    }

    public void LoadData()
    {
        // ジェム数の読み込み（初回は初期値、例えば100個など）
        gemCount = PlayerPrefs.GetInt("GemCount", 10);

        // 所持アイテムリストの読み込み
        string itemsString = PlayerPrefs.GetString("SavedItems", "");
        Items.Clear();

        if (!string.IsNullOrEmpty(itemsString))
        {
            string[] splitItems = itemsString.Split(',');
            foreach (string itemStr in splitItems)
            {
                if (int.TryParse(itemStr, out int itemId))
                {
                    Items.Add(itemId);
                }
            }
            for(int i = 0; i < Items.Count; i ++)
            {
                int buttonNumber = Items[i];
                if(!ItemButton[buttonNumber].activeSelf) ItemButton[buttonNumber].SetActive(true);
            }
        }
    }

    void OnApplicationQuit()
    {
        SaveData();
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
    }

    
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, System.Collections.Generic.Dictionary<string, object> data) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, System.ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnSessionListUpdated(NetworkRunner runner, System.Collections.Generic.List<SessionInfo> sessionList) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
}