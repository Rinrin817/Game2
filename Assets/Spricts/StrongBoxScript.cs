using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class StrongBoxScript : MonoBehaviour
{
    [SerializeField] GameObject[] PlayerObj;
    [SerializeField] GameObject[] Objects;
    [SerializeField] GameObject GemObj;
    [SerializeField] GameObject ManagerObj;
    [SerializeField] AudioClip openStrongBox;
    AudioSource audioSource;
    public int boxNumber;
    public bool action;
    public bool isOpen;
    Rigidbody rb;
    PlayerController playerController;
    EnemyPlayerController[] enemyPlayerController;
    VariableManager variableManager;
    float timeLimit = 2.5f;
    float timeCount;
    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        variableManager = ManagerObj.GetComponent<VariableManager>();
        action = false;
        isOpen = false;
        enemyPlayerController = new EnemyPlayerController[variableManager.playerCount];
        for(int i = 0; i < variableManager.playerCount; i++)
        {
            if(i == 0)
            {
                playerController = PlayerObj[i].GetComponent<PlayerController>();
            }
            else
            {
                enemyPlayerController[i - 1] = PlayerObj[i].GetComponent<EnemyPlayerController>();
                //Debug.Log($"Index Check: i={i}, PlayerObjLength={PlayerObj.Length}, ArrayLength={enemyPlayerController.Length}");
            }
        }
    }

    void Update()
    {
        timeCount += Time.deltaTime;
        if(action && !isOpen)
        {
            timeCount = 0;
            isOpen = true;
            Objects[4].SetActive(false);
            for(int i = 0; i < 6; i ++)
            {
                GameObject Obj = Objects[i];
                Obj.GetComponent<NavMeshObstacle>().enabled = false; // 先に穴を埋める
                for(int i2 = 0; i2 < variableManager.playerCount; i2++)
                {
                    foreach (var objCol in Obj.GetComponents<Collider>())
                    {
                        foreach (var playerCol in PlayerObj[i2].GetComponents<Collider>())
                        {
                            Physics.IgnoreCollision(objCol, playerCol);
                        }
                        foreach (var gemCol in GemObj.GetComponents<Collider>())
                        {
                            Physics.IgnoreCollision(objCol, gemCol);
                        }
                    }   
                }
                rb = Obj.GetComponent<Rigidbody>();
                rb.constraints = RigidbodyConstraints.None;
                rb.AddForce(Vector3.up * 50f, ForceMode.Impulse);
            }
        }
        if(isOpen && timeCount >= timeLimit)
        {
            for(int i = 0; i < 6; i ++)
            {
                GameObject Obj = Objects[i];
                Obj.SetActive(false);
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            if(Input.GetMouseButton(1) && playerController.roleNumber == 0)
            {
                action = true;
                audioSource.PlayOneShot(openStrongBox);
            }
            if(other.gameObject.GetComponent<EnemyPlayerController>() != null && other.gameObject.GetComponent<EnemyPlayerController>().pushedKey.Contains("Mouse1") && other.gameObject.GetComponent<EnemyPlayerController>().roleNumber == 0)
            {
                action = true;
                audioSource.PlayOneShot(openStrongBox, 0.5f);
            }
        }
    }
}
