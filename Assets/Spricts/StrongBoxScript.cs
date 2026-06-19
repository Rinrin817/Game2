using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class StrongBoxScript : MonoBehaviour
{
    List<GameObject> PlayerObj = new List<GameObject>();
    [SerializeField] GameObject[] Objects;
    [SerializeField] GameObject GemObj;
    [SerializeField] GameObject ManagerObj;
    [SerializeField] AudioClip openStrongBox;
    AudioSource audioSource;
    public int boxNumber;
    public bool action;
    public bool isOpen;
    Rigidbody rb;
    List<PlayerController> playerController = new List<PlayerController>();
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
    }

    public void AddPlayer(GameObject player)
    {
        if (!PlayerObj.Contains(player))
        {
            PlayerObj.Add(player);
            playerController.Add(player.GetComponent<PlayerController>());
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
                Obj.GetComponent<NavMeshObstacle>().enabled = false;
                foreach (GameObject player in PlayerObj)
                {
                    if (player == null) continue;

                    foreach (var objCol in Obj.GetComponents<Collider>())
                    {
                        foreach (var playerCol in player.GetComponents<Collider>())
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
            if(other.gameObject.GetComponent<PlayerController>().roleNumber == 0)
            {
                action = true;
                audioSource.PlayOneShot(openStrongBox);
            }
        }
    }
}
