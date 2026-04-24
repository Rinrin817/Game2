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
    [SerializeField] Text timeCountLimitText;
    [SerializeField] GameObject daruma0Obj;
    [SerializeField] Text upText;
    public string textString;
    public int[] gemList;
    public int playerCount;
    public GameObject[] prisonObjects;
    public bool prisonBreak;
    public int isFinish;
    public bool isGoalOpen;
    public int missionNumber;
    public int missionSubNumber;
    public bool canMission;
    public bool clearMission;
    public float missionTimeCount;
    public float missionTimeCountLimit;
    public bool missionStart;
    PlayerController playerController;
    EnemyPlayerController[] enemyPlayerController = new EnemyPlayerController[4];
    int[] roleArray = new int[5]{-1, -1, -1, -1, -1};
    int random;
    bool previousPrisonBreak;
    float canMissionTimeCount;
    float canMissionTimeLimit;

    // Start is called before the first frame update
    void Awake()
    {
        playerController = PlayerObj[0].GetComponent<PlayerController>();
        for(int i = 0; i < playerCount - 1; i ++)
        {
            Debug.Log(i.ToString());
            enemyPlayerController[i] = PlayerObj[i + 1].GetComponent<EnemyPlayerController>();
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
        canMissionTimeLimit = 10f;
        textString = " ";

        List<int> roles = new List<int>();
        if(playerCount == 5)
        {
            roles = new List<int> { 0, 0, 0, 1, 2 };   
        }
        roles = roles.OrderBy(x => System.Guid.NewGuid()).ToList();
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
        }
        else
        {
            if(canMissionTimeCount >= canMissionTimeLimit)
            {
                canMission = true;
            }
        }
        if(missionNumber != -1)
        {
            if(missionStart)
            {
                clearMission = false;
                missionStart = false;
                missionTimeCount = 0;
                if(missionNumber == 0)
                {
                    missionTimeCountLimit = 30f;
                    daruma0Obj.SetActive(true);
                    if(missionSubNumber == 0)
                    {
                        daruma0Obj.transform.position = new Vector3(70, 0, 0);
                    }
                    if(missionSubNumber == 1)
                    {
                        daruma0Obj.transform.position = new Vector3(-70, 0, 0);
                    }
                    if(missionSubNumber == 2)
                    {
                        daruma0Obj.transform.position = new Vector3(0, 0, 70);
                    }
                    if(missionSubNumber == 3)
                    {
                        daruma0Obj.transform.position = new Vector3(0, 0, -70);
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
            timeCountText.text = "Time " + (Mathf.Floor(missionTimeCount)).ToString();
            timeCountLimitText.text = "Limit " + missionTimeCountLimit.ToString();
            if(clearMission)
            {
                if(missionNumber == 0)
                {
                    daruma0Obj.transform.position = new Vector3(500, 0, 500);
                    daruma0Obj.SetActive(false);
                }
                missionNumber = -1;
                missionSubNumber = -1;
                canMissionTimeCount = 0;
            }
        }
        else
        {
            timeCountText.text = " ";
            timeCountLimitText.text = " ";
        }
        if(isFinish != -1)
        {
            ChangeScene("Home");
        }
    }
    
    void ChangeScene(string sceneName)
    {
        // 指定した名前のシーンに切り替える
        SceneManager.LoadScene(sceneName);
    }
}
