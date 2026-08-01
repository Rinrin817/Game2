using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class StrongBox2 : MonoBehaviour
{
    [SerializeField] GameObject[] Objects;
    [SerializeField] GameObject gemObj;
    [SerializeField] AudioClip openStrongBox;
    Vector3[] objectTransfrom = new Vector3[6];
    AudioSource audioSource;
    public bool action;
    public bool isOpen;
    bool haveOpen;
    Rigidbody rb;
    VariableManager variableManager;
    float timeLimit = 1.5f;
    float timeCount;
    int audioCount;
    public float cooldownTimer = 0f;
    bool audioBool = false;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        action = false;
        isOpen = false;
        for(int i = 0; i < 6; i ++)
        {
            objectTransfrom[i] = Objects[i].transform.position;
        }
    }

    void Update()
    {
        if(audioCount > 5) cooldownTimer -= Time.deltaTime;
        if(cooldownTimer < 0) audioCount = 0;
        timeCount += Time.deltaTime;
        if(action)
        {
            if(!audioBool && audioCount <= 5)
            {
                audioCount ++;
                audioSource.PlayOneShot(openStrongBox);
                cooldownTimer = 1000f;
            }
        }
        if(action && !isOpen)
        {
            haveOpen = false;
            timeCount = 0;
            isOpen = true;
            Objects[4].SetActive(false);
            for(int i = 0; i < 6; i ++)
            {
                GameObject Obj = Objects[i];
                Obj.GetComponent<NavMeshObstacle>().enabled = false;
                Obj.GetComponent<BoxCollider>().enabled = false;

                rb = Obj.GetComponent<Rigidbody>();
                rb.constraints = RigidbodyConstraints.None;
                float random = Random.Range(-80f, 80f);
                float random2 = Random.Range(-80f, 80f);
                float random3 = Random.Range(80f, 150f);
                Vector3 targetVector3 = new Vector3(random, random3, random2);
                rb.AddForce(targetVector3, ForceMode.Impulse);

                float rotX = Random.Range(-1f, 1f);
                float rotY = Random.Range(-1f, 1f);
                float rotZ = Random.Range(-1f, 1f);
                Vector3 torqueVector = new Vector3(rotX, rotY, rotZ);
                rb.AddTorque(torqueVector, ForceMode.Impulse);
            }
        }
        if(action && isOpen && timeCount >= timeLimit)
        {
            haveOpen = true;
            for(int i = 0; i < 6; i ++)
            {
                GameObject Obj = Objects[i];
                Obj.GetComponent<BoxCollider>().enabled = true;
                rb = Obj.GetComponent<Rigidbody>();
                rb.velocity = new Vector3(0, 0, 0);
                Obj.transform.position = objectTransfrom[i];
                Obj.transform.rotation = Quaternion.Euler(0, 0, 0);
            }
        }
        if(!isOpen && haveOpen)
        {
            haveOpen = false;
        }
    }
}
