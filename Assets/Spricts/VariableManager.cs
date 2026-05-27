using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;

public class VariableManager : MonoBehaviour
{
    [SerializeField] GameObject[] PlayerObj;
    [SerializeField] Material[] materials;
    [SerializeField] Text timeCountText;
    [SerializeField] GameObject[] daruma0Obj;
    [SerializeField] GameObject[] prisonObj;
    [SerializeField] GameObject lightObj;
    [SerializeField] Text upText;
    [SerializeField] Text coolTimeText;
    [SerializeField] AudioClip missionStartAudio;
    [SerializeField] AudioClip openGoal;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioSource smallAudioSource;
    [SerializeField] AudioClip prisonBreakAudio;
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
    PlayerController playerController;
    EnemyPlayerController[] enemyPlayerController;
    int[] roleArray;
    int random;
    bool previousPrisonBreak;
    bool audio = false;
    float canMissionTimeCount;
    float canMissionTimeLimit;
    int prisonPlayerCount;
    int goalPlayerCount;
    int continuePlayerCount;
    PlayerController playerController2;

    // Start is called before the first frame update
    void Awake()
    {
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
        playerController = PlayerObj[0].GetComponent<PlayerController>();
        enemyPlayerController = new EnemyPlayerController[playerCount];
        for(int i = 0; i < playerCount - 1; i ++)
        {
            // Debug.Log(i.ToString());
            enemyPlayerController[i] = PlayerObj[i + 1].GetComponent<EnemyPlayerController>();
        }
        for(int i = playerCount; i < 13; i ++)
        {
            PlayerObj[i].SetActive(false);
        }
        // playerController.roleNumber = 2;
        prisonBreak = true;
        previousPrisonBreak = true;
        isFinish = -1;
        missionNumber = -1;
        missionSubNumber = -1;
        canMission = true;
        missionStart = false;
        canMissionTimeCount = 0;
        textString = " ";
        coolTimeText.text = " ";
        audio = false;

        List<int> roles = new List<int>();
        if(playerCount == 5)
        {
            roles = new List<int> { 0, 0, 0, 1, 2 };
            canMissionTimeLimit = 20f;
        }
        if(playerCount == 9)
        {
            roles = new List<int> { 0, 0, 0, 0, 0, 0, 1, 1, 2 };
            canMissionTimeLimit = 16f;
        }
        if(playerCount == 13)
        {
            roles = new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 2 };
            canMissionTimeLimit = 12f;  
        }
        roles = roles.OrderBy(x => System.Guid.NewGuid()).ToList();
        if(playerRole == 0)
        {
            while(roles[0] != 0)
            {
                roles = roles.OrderBy(x => System.Guid.NewGuid()).ToList();
            }
        }
        if(playerRole == 1)
        {
            while(roles[0] != 1)
            {
                roles = roles.OrderBy(x => System.Guid.NewGuid()).ToList();
            }
        }
        if(playerRole == 2)
        {
            while(roles[0] != 2)
            {
                roles = roles.OrderBy(x => System.Guid.NewGuid()).ToList();
            }
        }
        playerRole = roles[0];
        roleArray = new int[playerCount];
        for(int i = 0; i < playerCount; i++)
        {
            int finalRole = roles[i]; // シャッフル済みリストから順番に取る
            roleArray[i] = finalRole;

            if(i == 0)
            {
                playerController.roleNumber = finalRole;
            }
            else
            {
                enemyPlayerController[i - 1].roleNumber = finalRole;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        upText.text = textString;
        missionTimeCount += Time.deltaTime;
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
        if(canMission)
        {
            canMissionTimeCount = 0;
            if(missionNumber == -1 && playerController.roleNumber == 2) coolTimeText.text = "canMission!";
            else coolTimeText.text = " ";
        }
        else
        {
            if(playerController.roleNumber == 2)
            {
                if(missionNumber != -1)
                {
                    coolTimeText.text = " ";
                }
                else
                {
                    coolTimeText.text = (Mathf.Floor(canMissionTimeLimit) - Mathf.Floor(canMissionTimeCount)).ToString();
                }
            }
            if(canMissionTimeCount >= canMissionTimeLimit)
            {
                canMission = true;
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
                    if(playerCount == 5) missionTimeCountLimit = 30;
                    if(playerCount == 9) missionTimeCountLimit = 22;
                    if(playerCount == 13) missionTimeCountLimit = 15;
                    if(missionSubNumber == 0)
                    {
                        for(int i = 0; i < daruma0Obj.Length; i ++)
                        {
                            daruma0Obj[i].SetActive(true);
                            daruma0Obj[i].transform.position = new Vector3(70, 0, 0);
                            daruma0Obj[i].transform.rotation = Quaternion.Euler(0, 90, 0);
                        }
                    }
                    if(missionSubNumber == 1)
                    {
                        for(int i = 0; i < daruma0Obj.Length; i ++)
                        {
                            daruma0Obj[i].SetActive(true);
                            daruma0Obj[i].transform.position = new Vector3(-70, 0, 0);
                            daruma0Obj[i].transform.rotation = Quaternion.Euler(0, -90, 0);
                        }
                    }
                    if(missionSubNumber == 2)
                    {
                        for(int i = 0; i < daruma0Obj.Length; i ++)
                        {
                            daruma0Obj[i].SetActive(true);
                            daruma0Obj[i].transform.position = new Vector3(0, 0, 70);
                            daruma0Obj[i].transform.rotation = Quaternion.Euler(0, 180, 0);
                        }
                    }
                    if(missionSubNumber == 3)
                    {
                        for(int i = 0; i < daruma0Obj.Length; i ++)
                        {
                            daruma0Obj[i].SetActive(true);
                            daruma0Obj[i].transform.position = new Vector3(0, 0, -70);
                            daruma0Obj[i].transform.rotation = Quaternion.Euler(0, -180, 0);
                        }
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
            timeCountText.text = "Time " + ((Mathf.Floor(missionTimeCountLimit)) - (Mathf.Floor(missionTimeCount))).ToString();
            if(clearMission)
            {
                lightObj.GetComponent<Light>().color = Color.white;
                lightObj.GetComponent<Light>().intensity = 1f;
                if(missionNumber == 0)
                {
                    for(int i = 0; i < daruma0Obj.Length; i ++)
                    {
                        daruma0Obj[i].transform.position = new Vector3(500, 0, 500);
                        daruma0Obj[i].SetActive(false);
                    }
                }
                missionNumber = -1;
                missionSubNumber = -1;
                canMissionTimeCount = 0;
                canMission = false;
                clearMission = false;
                textString = " ";
                for (int i = 0; i < playerCount; i++)
                {
                    playerController2 = PlayerObj[i].GetComponent<PlayerController>();
                    EnemyPlayerController EPC = PlayerObj[i].GetComponent<EnemyPlayerController>();

                    if (playerController2 != null) playerController2.canMove = true;
                    if (EPC != null) EPC.canMove = true;
                }
                playerController2 = PlayerObj[0].GetComponent<PlayerController>();
            }
        }
        else
        {
            timeCountText.text = " ";
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
            playerController2 = PlayerObj[i].GetComponent<PlayerController>();
            EnemyPlayerController EPC = PlayerObj[i].GetComponent<EnemyPlayerController>();
            if (playerController2 != null)
            {
                if(playerController2.roleNumber == 0 && playerController2.stateNumber == 1)
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
            if (EPC != null)
            {
                if(EPC.roleNumber == 0 && EPC.stateNumber == 1)
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
            }
            else
            {
                isFinish = 1;
            }
        }
        if((playerCount == 5 && prisonPlayerCount == 3) || (playerCount == 9 && prisonPlayerCount == 6) || (playerCount == 13 && prisonPlayerCount == 9))
        {
            if(playerController.roleNumber == 1)
            {
                isFinish = 0;
            }
            else
            {
                isFinish = 1;
            }
        }
        if((playerCount == 5 && goalPlayerCount == 3) || (playerCount == 9 && goalPlayerCount == 6) || (playerCount == 13 && goalPlayerCount == 9))
        {
            if(playerController.roleNumber != 0)
            {
                isFinish = 1;
            }
        }
        if(isFinish != -1)
        {
            SceneManager.LoadScene("Home");
        }
    }
}
