using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using Fusion;
using Fusion.Sockets;

public class PlayerController : NetworkBehaviour
{
    public bool isEnemy;
    [SerializeField] Rigidbody rb;
    [SerializeField] GameObject cameraObj;
    [SerializeField] Material[] materials;
    [SerializeField] GameObject goodImage;
    [SerializeField] GameObject badImage;
    GameObject ManagerObj;
    [SerializeField] GameObject[] prisonObj;
    GameObject[] goalObj;
    GameObject parent;
    GameObject NetworkObj;
    [SerializeField] GameObject nameCanvas;
    [SerializeField] Image Joystick;
    [SerializeField] GameObject[] thiefBlenderObj;
    [SerializeField] GameObject[] polliceBlenderObj;
    [SerializeField] Collider myCollider;
    [SerializeField] PhysicMaterial defaultFriction;
    [SerializeField] PhysicMaterial noFriction;
    [SerializeField] AudioClip goodAction;
    [SerializeField] AudioClip actionAudio;
    [SerializeField] AudioClip goodAction2;
    [SerializeField] AudioClip jump;
    [SerializeField] AudioClip bigJumpAudio;
    [SerializeField] AudioClip badAction;
    [SerializeField] TextMeshProUGUI nameText;
    [Networked, OnChangedRender(nameof(OnNameChanged))] public NetworkString<_16> NetworkPlayerName { get; set; }
    public float speed;
    public float jumpForce;
    [Networked] public int roleNumber { get; set; }
    int roleNumber2;
    [Networked] public int stateNumber { get; set; }
    [Networked, Capacity(16)] public NetworkString<_16> PlayerName { get; set; }
    [Networked] public int syncedSeed { get; set; } = 0;
    [Networked] public int currentEmote { get; set; } = 0;
    [Networked] public bool isMarked { get; set; } = false;
    public bool canMove;
    Transform prisonTransform = null;
    CameraController cameraController;
    VariableManager variableManager;
    GameObject collisionObj;
    AudioSource audioSource;
    public Animator animator;
    bool goodImageBool;
    bool badImageBool;
    int jumpCount;
    int jumpCountLimit;
    int cameraNumber;
    public bool isStand;
    public bool bigJump;
    bool runAnimationBool;
    bool isRoleCheck = false;
    bool jogJumpRequest;
    float startCount = 0;
    public int missionSub;
    float cooldownTimer = 0f;
    float cameraY2;
    int wishMissionNumber;
    public bool isFalling;
    private GameObject targetDaruma;
    float followSpeed = 1.5f;
    float rotateSpeed = 25.0f; // 回転速度
    string obstacleTag = "Stage";
    PlayerController playerController;
    // AI思考システム
    [SerializeField] Transform[] darumaTransform;
    [SerializeField] Transform[] prisonTransformArray;
    [SerializeField] Transform[] goalTransformArray;
    [SerializeField] GameObject randomObj;

    public string[] pushedKey = new string[12];

    public enum myState
    {
        Idle, Steal, Goal, Chasing, Escape, Mission, Rescue
    }

    public myState _state = myState.Idle;

    public GameObject targetObj;
    public GameObject[] playersObj;
    public Transform[] playersTransform;

    UnityEngine.AI.NavMeshAgent agent;
    Transform aiPrisonTransform;
    Transform aiGoalTransform;
    Transform goalTransform;
    Vector3 lastDestination;
    Vector3 nowDestination;
    Vector3 targetDir;
    Quaternion targetRot;
    int random;
    int random2;
    float randomDis;
    float distance;
    float obstacleCheckDistance = 1f;
    float obstacleCheckHeight = 3f;
    float lookTimer;
    float headAngle;
    float lookSpeed;
    float timer;
    float chaseTimer = 16f;
    float[] transformTimer = new float[5];
    bool wishJump;
    bool hasExtraInput;
    bool darumaTransformSet;
    Vector3 lastPosition;
    float stuckTimer;
    bool isSecondTime = false;

    [Networked] public int syncedThiefSkinNumber { get; set; }
    [Networked] public int syncedPoliceSkinNumber { get; set; }
    [Networked] public NetworkBool isSkinInitialized { get; set; }

    // ★他人の画面でもアニメーションを動かすために、ネットワーク変数(各画面で同期される)にします
    [Networked] float x { get; set; }
    [Networked] float z { get; set; }

    void Start()
    {
        // Debug.Log($"{name} : {HasStateAuthority}");
        ManagerObj = FindFirstObjectByType<VariableManager>().gameObject;
        NetworkObj = GameObject.Find("NetworkRunner");
        prisonObj = GameObject.FindGameObjectsWithTag("Prison");
        goalObj = FindObjectsByType<GameObject>(FindObjectsSortMode.None).Where(obj => obj.name == "Goal").ToArray();
        cameraNumber = -1;
        audioSource = GetComponent<AudioSource>();
        jumpForce = 13;
        jumpCount = 0;
        jumpCountLimit = 1;
        missionSub = -1;
        runAnimationBool = false;
        jogJumpRequest = false;
        variableManager = ManagerObj.GetComponent<VariableManager>();
        if (isEnemy)
        {
            if(VariableManager.stageNumber == 0) return;
            Debug.Log((VariableManager.stageNumber).ToString());
            aiPrisonTransform = prisonObj[VariableManager.stageNumber].transform;   
            // aiGoalTransform = goalObj[VariableManager.stageNumber].transform;
            agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
            {
                agent.updatePosition = false;
                agent.updateRotation = false;
            }
            if (roleNumber == 1)
            {
                SetAreaMask("DoroOnly", false);
            }
            else if (agent != null)
            {
                agent.areaMask = UnityEngine.AI.NavMesh.AllAreas;
            }

            jumpCount = 0;
            jumpCountLimit = 1;
            missionSub = -1;
            darumaTransformSet = false;
        }
        Transform parent = GameObject.Find("darumaTransforms").transform;
        darumaTransform = new Transform[parent.childCount];
        for (int i = 0; i < parent.childCount; i++)
        {
            darumaTransform[i] = parent.GetChild(i);
        }
        playersObj = GameObject.FindGameObjectsWithTag("Player");

        playersTransform = playersObj
            .Select(obj => obj.transform)
            .ToArray();
        transformTimer = new float[playersTransform.Length];

        GameObject randoms = GameObject.Find("randoms");
        string enemyName = gameObject.name;
        int number = 0;
        if(!gameObject.name.Contains("Player"))
        {
            if (enemyName.StartsWith("Enemy_") &&
            int.TryParse(enemyName.Replace("Enemy_", ""), out int parsedNumber))
            {
                number = parsedNumber;
                nameText.text = variableManager.GetCPUName(gameObject);
                Debug.Log(variableManager.GetCPUName(gameObject));
            }
            string randomName = number == 1
                ? "Random"
                : $"Random ({number - 1})";

            if (randoms != null)
            {
                randomObj = randoms.transform.Find(randomName)?.gameObject;
            }
        }
    }

    void SetAreaMask(string areaName, bool canPass)
    {
        // エリア名からインデックス（番号）を取得
        int areaIndex = UnityEngine.AI.NavMesh.GetAreaFromName(areaName);
        
        if (canPass)
        {
            // そのエリアを通れるようにする（ビットを立てる）
            agent.areaMask |= (1 << areaIndex);
        }
        else
        {
            // そのエリアを通れないようにする（ビットを降ろす）
            agent.areaMask &= ~(1 << areaIndex);
        }
    }

    // ★確実に動くように、シンプルな入力同期処理にします
    public override void FixedUpdateNetwork()
    {
        // 自分が操作している画面のときだけ、キーボードの入力を反映してネットワーク変数に入れる
        if (HasStateAuthority && canMove)
        {
            //x = Input.GetAxisRaw("Horizontal");
            //z = Input.GetAxisRaw("Vertical");
            if (Input.GetKey(KeyCode.D)) x = 1;
            if (Input.GetKey(KeyCode.A)) x = -1;
            if (Input.GetKey(KeyCode.W)) z = 1;
            if (Input.GetKey(KeyCode.S)) z = -1;
            if (!Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.A)) x = 0;
            if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S)) z = 0;

            if(Joystick != null) x = Joystick.GetComponent<JoystickScript>().moveDirection.x / 18f;
            if(Joystick != null) z = Joystick.GetComponent<JoystickScript>().moveDirection.y / 18f;
            if(Joystick != null && Joystick.GetComponent<JoystickScript>().isJumpRequest)
            {
                if (roleNumber == 2 || roleNumber == 3)
                {
                    transform.position += Vector3.up * 0.4f;
                }
                else
                {
                    if(animator != null) animator.SetBool("isHelloAnimation", false);
                    if(animator != null) animator.SetBool("isYurayuraAnimation", false);
                    if(animator != null) animator.SetBool("isDanceAnimation", false);
                    currentEmote = 0;
                    ExecuteJump();
                    jogJumpRequest = false;
                }
                Joystick.GetComponent<JoystickScript>().isJumpRequest = false;
            }

            if (jogJumpRequest)
            {
                // エモートの解除処理
                if(animator != null) animator.SetBool("isHelloAnimation", false);
                if(animator != null) animator.SetBool("isYurayuraAnimation", false);
                if(animator != null) animator.SetBool("isDanceAnimation", false);
                currentEmote = 0;

                if(roleNumber != 2 && roleNumber != 3) ExecuteJump();

                jogJumpRequest = false;
            }
            
            if (Input.GetKey(KeyCode.Space))
            {
                if (roleNumber == 2 || roleNumber == 3)
                {
                    transform.position += Vector3.up * 0.4f;
                }
            }
        }

        float currentYVelocity = rb.velocity.y;

        if (roleNumber == 2 || roleNumber == 3)
        {
            currentYVelocity = 0;
        }

        Vector3 direction = transform.forward * z + transform.right * x;

        rb.velocity = new Vector3(
            direction.x * speed,
            currentYVelocity,
            direction.z * speed
        );
    }

    // ジャンプの実体（FixedUpdateNetworkから呼び出します）
    public void ExecuteJump()
    {
        if (roleNumber != 2 && roleNumber != 3 && isStand && canMove)
        {
            if (jumpCount < jumpCountLimit)
            {
                if (audioSource != null) audioSource.PlayOneShot(jump, 0.7f);
                if (animator != null) animator.SetBool("isJumpAnimation", true);

                myCollider.sharedMaterial = noFriction;

                Vector3 vel = rb.velocity;
                if (!bigJump) vel.y = 0;
                rb.velocity = vel;

                RaycastHit hit;
                Vector3 pushDirection = Vector3.zero;
                Vector3 rayOrigin = transform.position + Vector3.down * 0.5f;

                /*

                if (Physics.Raycast(rayOrigin, transform.forward, out hit, 0.7f))
                {
                    if (hit.collider.CompareTag("Stage"))
                    {
                        pushDirection = hit.normal;
                    }
                }

                if (pushDirection != Vector3.zero)
                {
                    rb.AddForce(pushDirection * 25f, ForceMode.Impulse);
                }

                */

                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

                jumpCount++;
                isStand = false;
            }
        }
    }

    void Update()
    {
        if(prisonObj.Length == 0 || (isEnemy && agent == null) || (isEnemy && randomObj == null))
        {
            if(GameObject.FindGameObjectsWithTag("Prison") != null && VariableManager.stageNumber != 0)
            {
                ManagerObj = FindFirstObjectByType<VariableManager>().gameObject;
                NetworkObj = GameObject.Find("NetworkRunner");
                prisonObj = GameObject.FindGameObjectsWithTag("Prison");
                goalObj = FindObjectsByType<GameObject>(FindObjectsSortMode.None).Where(obj => obj.name == "Goal").ToArray();
                cameraNumber = -1;
                audioSource = GetComponent<AudioSource>();
                jumpForce = 13;
                jumpCount = 0;
                jumpCountLimit = 1;
                missionSub = -1;
                runAnimationBool = false;
                jogJumpRequest = false;
                variableManager = ManagerObj.GetComponent<VariableManager>();
                if (isEnemy)
                {
                    if(VariableManager.stageNumber == 0) return;
                    // Debug.Log((VariableManager.stageNumber).ToString());
                    aiPrisonTransform = prisonObj[VariableManager.stageNumber].transform;   
                    // aiGoalTransform = goalObj[VariableManager.stageNumber].transform;
                    agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
                    if (agent != null)
                    {
                        agent.updatePosition = false;
                        agent.updateRotation = false;
                    }
                    if (roleNumber == 1)
                    {
                        SetAreaMask("DoroOnly", false);
                    }
                    else if (agent != null)
                    {
                        agent.areaMask = UnityEngine.AI.NavMesh.AllAreas;
                    }

                    jumpCount = 0;
                    jumpCountLimit = 1;
                    missionSub = -1;
                    darumaTransformSet = false;
                }
                Transform parent = GameObject.Find("darumaTransforms").transform;
                darumaTransform = new Transform[parent.childCount];
                for (int i = 0; i < parent.childCount; i++)
                {
                    darumaTransform[i] = parent.GetChild(i);
                }
                playersObj = GameObject.FindGameObjectsWithTag("Player");

                playersTransform = playersObj
                    .Select(obj => obj.transform)
                    .ToArray();
                transformTimer = new float[playersTransform.Length];

                GameObject randoms = GameObject.Find("randoms");
                string enemyName = gameObject.name;
                int number = 0;
                if(!gameObject.name.Contains("Player"))
                {
                    if (enemyName.StartsWith("Enemy_") &&
                    int.TryParse(enemyName.Replace("Enemy_", ""), out int parsedNumber))
                    {
                        number = parsedNumber;
                        nameText.text = variableManager.GetCPUName(gameObject);
                        Debug.Log(variableManager.GetCPUName(gameObject));
                    }
                    string randomName = number == 1
                        ? "Random"
                        : $"Random ({number - 1})";

                    if (randoms != null)
                    {
                        randomObj = randoms.transform.Find(randomName)?.gameObject;
                    }
                }
            }
            return;
        }
        if(isEnemy)
        {
            if(variableManager.prisonBreak) stateNumber = 0;

            ConsiderMove();
            CheckStuck();
            agent.nextPosition = transform.position;
            Vector3 agentPos = agent.nextPosition;
            Vector3 realPos = transform.position;
            float dist = Vector3.Distance(agentPos, realPos);
            if (dist >= 3.0f)
            {
                agent.Warp(transform.position);
            }
            CheckAndAutoJump();
            if (canMove)
            {
                x = 0; z = 0;
                if (pushedKey.Contains("D")) x += 1;
                if (pushedKey.Contains("A")) x -= 1;
                if (pushedKey.Contains("W")) z += 1;
                if (pushedKey.Contains("S")) z -= 1;
                if(animator != null)
                {
                    if (!pushedKey.Contains("D") && !pushedKey.Contains("A") && !pushedKey.Contains("W") && !pushedKey.Contains("S")) animator.SetBool("isRunAnimation", false);
                    else animator.SetBool("isRunAnimation", true);   
                }
            }
            else
            {
                if(animator != null) animator.SetBool("isRunAnimation", false);
                x = 0; z = 0;
            }
            Vector3 direction = transform.forward * z + transform.right * x;
            rb.velocity = new Vector3(direction.x * speed, rb.velocity.y, direction.z * speed);
            if (_state == myState.Idle && roleNumber == 1)
            {
                chaseTimer += Time.deltaTime;
                if(targetObj != null)
                {
                    playerController = targetObj.GetComponent<PlayerController>();
                }
                if(playerController != null && playerController.stateNumber == 1)
                {
                    chaseTimer = 20f;
                    targetObj = null;
                }
                if(chaseTimer <= 15f)
                {
                    if(targetObj != null) randomObj.transform.position = targetObj.transform.position;
                    chaseTimer = 0;
                }
                else
                {
                    lookSpeed = 5f;
                    timer += Time.deltaTime;
                    if (timer < 5) lookSpeed = 0.1f;

                    lookTimer += Time.deltaTime;
                    headAngle = Mathf.Sin(lookTimer * lookSpeed) * 60f;
                }
            }
            else if (_state == myState.Chasing)
            {
                timer = 0;
            }
            if(roleNumber == 2)
            {
                for(int i = 0; i < playersTransform.Length; i ++)
                {
                    transformTimer[i] += Time.deltaTime;
                }
            }
            if(pushedKey.Contains("Space"))
            {
                // Debug.Log(gameObject.name + "space" + isStand.ToString());
                if(roleNumber == 2)
                {
                    transform.position += Vector3.up * 0.6f;
                }
                else if(isStand && canMove)
                {
                    // Debug.Log(gameObject.name + "space2");
                    if(jumpCount < jumpCountLimit)
                    {
                        // Debug.Log(gameObject.name + "space3");
                        myCollider.sharedMaterial = noFriction;
                        if(animator != null) animator.SetBool("isJumpAnimation", true);

                        Vector3 vel = rb.velocity;
                        if(!bigJump) vel.y = 0;
                        rb.velocity = vel;

                        RaycastHit hit;
                        Vector3 pushDirection = Vector3.zero;

                        Vector3 rayOrigin = transform.position + Vector3.down * 0.6f;

                        if (Physics.Raycast(rayOrigin, transform.forward, out hit, 0.7f))
                        {
                            if (hit.collider.CompareTag("Stage"))
                            {
                                // ★ hit.normal は「壁が向いている方向」
                                pushDirection = hit.normal;
                            }
                        }

                        if (pushDirection != Vector3.zero)
                        {
                            rb.AddForce(pushDirection * 25f, ForceMode.Impulse);
                        }

                        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

                        jumpCount++;
                        isStand = false;
                        pushedKey[4] = "";
                    }
                }
            }
            if(pushedKey.Contains("LShift"))
            {
                if(roleNumber == 2)
                {
                    transform.position += Vector3.down * 0.6f;
                }
                if(roleNumber == 0)
                {
                    if(animator.GetBool("isStandAnimation"))
                    {
                        if(animator != null) animator.SetBool("isStandAnimation", false);
                        speed = 3f;
                    }
                    else
                    {
                        if(animator != null) animator.SetBool("isStandAnimation", true);
                        speed = 7f;
                    }
                }
            }
            if(pushedKey.Contains("Mouse1"))
            {
                if(roleNumber == 2 && variableManager.canMission)
                {
                    if(pushedKey.Contains("1"))
                    {
                        missionSub = 0;
                    }
                }
            }
            if(missionSub != -1 && variableManager.missionSubNumber == -1 && roleNumber == 2 && canMove)
            {
                if(missionSub == 0)
                {
                    //ミッションスタート
                    if(pushedKey.Contains("RArrow"))
                    {
                        variableManager.missionNumber = 0;
                        variableManager.missionSubNumber = 0;
                        variableManager.canMission = false;
                        variableManager.missionStart = true;
                        variableManager.textString = " ";
                        missionSub = -1;
                    }
                    if(pushedKey.Contains("LArrow"))
                    {
                        variableManager.missionNumber = 0;
                        variableManager.missionSubNumber = 1;
                        variableManager.canMission = false;
                        variableManager.missionStart = true;
                        variableManager.textString = " ";
                        missionSub = -1;
                    }
                    if(pushedKey.Contains("UArrow"))
                    {
                        variableManager.missionNumber = 0;
                        variableManager.missionSubNumber = 2;
                        variableManager.canMission = false;
                        variableManager.missionStart = true;
                        variableManager.textString = " ";
                        missionSub = -1;
                    }
                    if(pushedKey.Contains("DArrow"))
                    {
                        variableManager.missionNumber = 0;
                        variableManager.missionSubNumber = 3;
                        variableManager.canMission = false;
                        variableManager.missionStart = true;
                        variableManager.textString = " ";
                        missionSub = -1;
                    }
                }
            }
        }
        if(cooldownTimer > 0) cooldownTimer -= Time.deltaTime;
        if(startCount < 0.1f)
        {
            startCount += Time.deltaTime;
            return;
        }
        if(prisonTransform == null)
        {
            prisonTransform = variableManager.targetPrisonObj;
        }
        if(goodImage == null)
        {
            GameObject CanvasObj = GameObject.Find("Canvas");
            Transform[] allTransforms = CanvasObj.GetComponentsInChildren<Transform>(true);
            foreach (Transform t in allTransforms)
            {
                // Debug.Log(t.name);
                if (t.name == "GoodImage") goodImage = t.gameObject;
                if (t.name == "BadImage") badImage = t.gameObject;
            }
        }
        if (isEnemy && roleNumber != -1 && !isSkinInitialized && Object.HasStateAuthority)
        {
            StartFusion startFusion = NetworkObj.GetComponent<StartFusion>();
            syncedThiefSkinNumber = startFusion.thiefSkinNumber;
            syncedPoliceSkinNumber = startFusion.polliceSkinNumber;
            isSkinInitialized = true;
        }

        if (!isRoleCheck && roleNumber != -1 && (!isEnemy || isSkinInitialized))
        {
            stateNumber = 0;

            if (animator != null)
            {
                Destroy(animator.gameObject);
            }

            roleNumber2 = roleNumber;

            if (roleNumber == 0)
            {
                gameObject.layer = 6;

                int thiefSkinNumber2 = isEnemy
                    ? syncedThiefSkinNumber
                    : NetworkObj.GetComponent<StartFusion>().thiefSkinNumber;

                animator = Instantiate(
                    thiefBlenderObj[thiefSkinNumber2],
                    transform.position + new Vector3(0, 0.1f, 0),
                    transform.rotation * Quaternion.Euler(0, 180, 0),
                    transform
                ).GetComponent<Animator>();

                animator.gameObject.transform.localScale = new Vector3(0.3f, 0.15f, 0.3f);
                animator.gameObject.SetActive(true);

                for (int i = 0; i < 6; i++)
                {
                    foreach (var objCol in prisonObj[i].GetComponents<Collider>())
                    {
                        foreach (var playerCol in GetComponents<Collider>())
                        {
                            Physics.IgnoreCollision(objCol, playerCol, false);
                        }
                    }
                }
            }

            if (roleNumber == 1)
            {
                gameObject.layer = 7;

                int polliceSkinNumber2 = isEnemy
                    ? syncedPoliceSkinNumber
                    : NetworkObj.GetComponent<StartFusion>().polliceSkinNumber;

                animator = Instantiate(
                    polliceBlenderObj[polliceSkinNumber2],
                    transform.position + new Vector3(0, 0.1f, 0),
                    transform.rotation * Quaternion.Euler(0, 180, 0),
                    transform
                ).GetComponent<Animator>();

                animator.gameObject.transform.localScale = new Vector3(0.3f, 0.15f, 0.3f);
                animator.gameObject.SetActive(true);
            }

            if (roleNumber == 2)
            {
                gameObject.layer = 8;

                foreach (Transform child in transform)
                {
                    if (child.gameObject.name == "WorldCanvas")
                    {
                        parent = child.gameObject;
                    }
                }

                foreach (Transform child in parent.transform)
                {
                    if (child.gameObject.GetComponent<Text>() != null)
                    {
                        child.gameObject.SetActive(false);
                    }
                }
            }

            if (Joystick == null && GameObject.Find("Joystick") != null)
            {
                Joystick = GameObject.Find("Joystick").GetComponent<Image>();
            }

            if (roleNumber == 2)
            {
                speed = 18f;
                rb.useGravity = false;
                GetComponent<BoxCollider>().enabled = false;
            }
            else
            {
                speed = 8f;
                rb.useGravity = true;
                GetComponent<BoxCollider>().enabled = true;
            }

            if (roleNumber == 1)
            {
                for (int i = 0; i < 6; i++)
                {
                    foreach (var objCol in prisonObj[i].GetComponents<Collider>())
                    {
                        foreach (var playerCol in GetComponents<Collider>())
                        {
                            Physics.IgnoreCollision(objCol, playerCol);
                        }
                    }
                }

                speed = 9f;
            }

            if (roleNumber != 3)
            {
                GetComponent<Renderer>().material = materials[roleNumber];
            }

            variableManager.isRoleChange = true;
            isRoleCheck = true;
        }
        if(roleNumber == 3)
        {
            if(animator != null)
            {
                Destroy(animator.gameObject);
                gameObject.layer = 8;
                int count = 0;
                foreach (Transform child in gameObject.transform)
                {
                    if(child.gameObject.name == "WorldCanvas")
                    {
                        parent = child.gameObject;
                    }
                }
                foreach (Transform child in parent.transform)
                {
                    if(child.gameObject.GetComponent<Text>() != null)
                    {
                        child.gameObject.SetActive(false);
                    }
                }   
            }
        }
        if(roleNumber != roleNumber2) isRoleCheck = false;
        GameObject[] roleCubes = GameObject.FindGameObjectsWithTag("RoleChange");
        foreach (GameObject cube in roleCubes)
        {
            if (cube != null)
            {
                float distance = Vector3.Distance(gameObject.transform.position, cube.transform.position);
                if (distance <= 3f)
                {
                    if (cube.name.Contains("Thief")) // 例: キューブの名前が「ThiefCube」など
                    {
                        roleNumber = 0;
                    }
                    else if (cube.name.Contains("Pollice"))
                    {
                        roleNumber = 1;
                    }
                    else if (cube.name.Contains("Daruma")) // 例: キューブの名前が「DarumaCube」など
                    {
                        roleNumber = 2;
                    }
                    if (audioSource != null && cooldownTimer > 0)
                    {
                        audioSource.PlayOneShot(actionAudio, 0.7f);
                        cooldownTimer = 1f;
                    }
                    variableManager.RequestSetActiveItem();
                    break;
                }
            }
        }

        if(roleNumber == 3)
        {
            speed = 18f;
            rb.useGravity = false;
            GetComponent<BoxCollider>().enabled = false;
        }

        cameraController = cameraObj.GetComponent<CameraController>();
        if (Object.HasStateAuthority)
        {
            if(variableManager.prisonBreak) stateNumber = 0;
        }

        if(goodImageBool)
        {
            goodImage.SetActive(true);
            goodImageBool = false;
        }
        if(badImageBool)
        {
            badImage.SetActive(true);
            badImageBool = false;
        }

        if (animator != null && animator.runtimeAnimatorController != null)
        {
            if (HasStateAuthority)
            {
                // Zキー：ハロー
                if (Input.GetKeyDown(KeyCode.Z))
                {
                    // すでにハロー(1)なら解除(0)に、そうでないならハロー(1)にする
                    currentEmote = (currentEmote == 1) ? 0 : 1;
                }

                // Xキー：ゆらゆら
                if (Input.GetKeyDown(KeyCode.X))
                {
                    currentEmote = (currentEmote == 2) ? 0 : 2;
                }

                // Cキー：ダンス
                if (Input.GetKeyDown(KeyCode.C))
                {
                    currentEmote = (currentEmote == 3) ? 0 : 3;
                }
            }

            ApplyEmoteAnimation();   
        }

        // アニメーション判定（全員の画面で動きます）
        if(canMove)
        {
            float stopThreshold = runAnimationBool ? 0.2f : 0.5f;
            bool changed = false;

            if(animator != null && !animator.GetBool("isRunAnimation"))
            {
                if(runAnimationBool)
                {
                    if(x > 0.7f || z > 0.7f || x < -0.7f || z < -0.7f)
                    {
                        if(animator != null) animator.SetBool("isRunAnimation", true);
                        if(animator != null) animator.SetBool("isHelloAnimation", false);
                        if(animator != null) animator.SetBool("isYurayuraAnimation", false);
                        if(animator != null) animator.SetBool("isDanceAnimation", false);
                        currentEmote = 0;
                        runAnimationBool = false;
                        changed = true;
                    }
                }
                else
                {
                    if(x > 0.5f || z > 0.5f || x < -0.5f || z < -0.5f)
                    {
                        if(animator != null) animator.SetBool("isRunAnimation", true);
                        if(animator != null) animator.SetBool("isHelloAnimation", false);
                        if(animator != null) animator.SetBool("isYurayuraAnimation", false);
                        if(animator != null) animator.SetBool("isDanceAnimation", false);
                        currentEmote = 0;
                        runAnimationBool = true;
                        changed = true;
                    }
                }
            }

            if(animator != null && animator.GetBool("isRunAnimation") && !changed)
            {
                if(runAnimationBool)
                {
                    if(x > 0.7f || z > 0.7f || x < -0.7f || z < -0.7f)
                    {
                        runAnimationBool = false;
                    }

                    if(x < stopThreshold && z < stopThreshold && x > -stopThreshold && z > -stopThreshold)
                    {
                        runAnimationBool = false;
                        if(animator != null) animator.SetBool("isRunAnimation", false);
                    }
                }
                else
                {
                    if(x < stopThreshold && z < stopThreshold && x > -stopThreshold && z > -stopThreshold)
                    {
                        if(animator != null) animator.SetBool("isRunAnimation", false);
                    }   
                }
            }
        }
        else
        {
            if(animator != null) animator.SetBool("isRunAnimation", false);
        }

        // 自分の画面だけの処理
        if (!Object.HasStateAuthority) return;
        if (isEnemy) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            jogJumpRequest = true; // 「ジャンプしたい」という意思を記録
        }

        if(Input.GetKeyDown(KeyCode.LeftShift))
        {
            if(roleNumber == 0)
            {
                if(animator != null && animator.GetBool("isStandAnimation"))
                {
                    if(animator != null) animator.SetBool("isStandAnimation", false);
                    if(animator != null) animator.SetBool("isHelloAnimation", false);
                    if(animator != null) animator.SetBool("isYurayuraAnimation", false);
                    if(animator != null) animator.SetBool("isDanceAnimation", false);
                    currentEmote = 0;
                    speed = 3f;
                }
                else
                {
                    if(animator != null) animator.SetBool("isStandAnimation", true);
                    if(animator != null) animator.SetBool("isHelloAnimation", false);
                    if(animator != null) animator.SetBool("isYurayuraAnimation", false);
                    if(animator != null) animator.SetBool("isDanceAnimation", false);
                    currentEmote = 0;
                    speed = 7f;
                }
            }
        }
        if(Input.GetKey(KeyCode.LeftShift))
        {
            if(roleNumber == 2 || roleNumber == 3)
            {
                transform.position += Vector3.down * 0.4f;
            }
        }
        if(Input.GetMouseButton(1))
        {
            if(roleNumber == 2 && variableManager.canMission)
            {
                if(Input.GetKey(KeyCode.Alpha1))
                {
                    missionSub = 0;
                }
                if(Input.GetKey(KeyCode.Alpha2))
                {
                    missionSub = 1;
                }
                if(Input.GetKey(KeyCode.Alpha3))
                {
                    missionSub = 2;
                }
            }
        }
        if(Input.GetKeyDown(KeyCode.Z))
        {
            if(animator != null && animator.GetBool("isHelloAnimation"))
            {
                if(animator != null) animator.SetBool("isHelloAnimation", false);
            }
            else
            {
                if(animator != null) animator.SetBool("isHelloAnimation", true);
                if(animator != null) animator.SetBool("isYurayuraAnimation", false);
                if(animator != null) animator.SetBool("isDanceAnimation", false);
            }
        }
        if(Input.GetKeyDown(KeyCode.X))
        {
            if(animator != null && animator.GetBool("isYurayuraAnimation"))
            {
                if(animator != null) animator.SetBool("isYurayuraAnimation", false);
            }
            else
            {
                if(animator != null) animator.SetBool("isYurayuraAnimation", true);
                if(animator != null) animator.SetBool("isHelloAnimation", false);
                if(animator != null) animator.SetBool("isDanceAnimation", false);
            }
        }
        if(Input.GetKeyDown(KeyCode.C))
        {
            if(animator != null && animator.GetBool("isDanceAnimation"))
            {
                if(animator != null) animator.SetBool("isDanceAnimation", false);
            }
            else
            {
                if(animator != null) animator.SetBool("isDanceAnimation", true);
                if(animator != null) animator.SetBool("isYurayuraAnimation", false);
                if(animator != null) animator.SetBool("isHelloAnimation", false);
            }
        }

        if(variableManager.missionNumber == 2 && isMarked)
        {
            roleCubes = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject cube in roleCubes)
            {
                if (cube != null)
                {
                    float distance = Vector3.Distance(gameObject.transform.position, cube.transform.position);
                    if (distance <= 2.5f)
                    {
                        if (cube.name.Contains("Thief")) // 例: キューブの名前が「ThiefCube」など
                        {
                            roleNumber = 0;
                        }
                        else if (cube.name.Contains("Pollice"))
                        {
                            roleNumber = 1;
                        }
                        else if (cube.name.Contains("Daruma")) // 例: キューブの名前が「DarumaCube」など
                        {
                            roleNumber = 2;
                        }
                        variableManager.RequestSetActiveItem();
                        break;
                    }
                }
            }   
        }

        if (roleNumber == 2)
        {
            if (targetDaruma == null)
            {
                targetDaruma = GameObject.Find("daruma1(Clone)");
            }
            else
            {
                float backDistance = 5.0f;
                Vector3 targetPosition = targetDaruma.transform.position;
                Rigidbody darumaRb = targetDaruma.GetComponent<Rigidbody>();

                if (darumaRb != null)
                {
                    Vector3 horizontalVelocity = new Vector3(darumaRb.velocity.x, 0f, darumaRb.velocity.z);

                    if (horizontalVelocity.sqrMagnitude > 0.1f)
                    {
                        Vector3 moveDirection = horizontalVelocity.normalized;
                        targetPosition.x -= moveDirection.x * backDistance;
                        targetPosition.z -= moveDirection.z * backDistance;
                    }
                    else
                    {
                        Vector3 darumaForward = targetDaruma.transform.forward;
                        darumaForward.y = 0f;
                        darumaForward.Normalize();
                        
                        targetPosition.x -= darumaForward.x * backDistance;
                        targetPosition.z -= darumaForward.z * backDistance;
                    }
                }

                Vector3 directionToDaruma = targetDaruma.transform.position - transform.position;
                float distanceToDaruma = directionToDaruma.magnitude;

                // 強制ワープ
                if (distanceToDaruma >= 20.0f)
                {
                    transform.position = targetPosition;
                    
                    Vector3 lookPos = targetDaruma.transform.position - transform.position;
                    lookPos.y = 0f;
                    if (lookPos != Vector3.zero)
                    {
                        transform.rotation = Quaternion.LookRotation(lookPos);
                    }
                    return; 
                }

                bool isBlocked = false;
                if (Physics.Raycast(transform.position, directionToDaruma.normalized, out RaycastHit hit, distanceToDaruma))
                {
                    if (hit.collider.CompareTag(obstacleTag))
                    {
                        isBlocked = true;
                    }
                }

                if (isBlocked)
                {
                    float targetYRotation = transform.eulerAngles.y + rotateSpeed * Time.deltaTime;
                    transform.rotation = Quaternion.Euler(0f, targetYRotation, 0f);
                }
                else
                {
                    transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);
                    
                    Vector3 lookPos = targetDaruma.transform.position - transform.position;
                    lookPos.y = 0f; 
                    
                    if (lookPos != Vector3.zero)
                    {
                        transform.rotation = Quaternion.LookRotation(lookPos);
                    }
                }   
            }
        }

        if(missionSub != -1 && roleNumber == 2)
        {
            if(variableManager.missionSubNumber == -1)
            {
                if(missionSub == 0)
                {
                    variableManager.textString = "矢印キーを押してだるまを設置！";
                    if(Input.GetKey(KeyCode.RightArrow)) SetMission(0, 0);
                    if(Input.GetKey(KeyCode.LeftArrow)) SetMission(0, 1);
                    if(Input.GetKey(KeyCode.UpArrow)) SetMission(0, 2);
                    if(Input.GetKey(KeyCode.DownArrow)) SetMission(0, 3);
                }
                if(missionSub == 1)
                {
                    variableManager.textString = "矢印キーを押してだるま加速！";
                    SetMission(1, 0);
                }
                if(missionSub == 2)
                {
                    List<GameObject> playerObjs = new List<GameObject>();
                    foreach (var player in variableManager.PlayerObj)
                    {
                        if (player == null) continue;
                        var nObj = player.GetComponent<NetworkObject>();
                        if (nObj != null)
                        {
                            if (!nObj.HasStateAuthority) 
                            {
                                playerObjs.Add(player); // 自分以外ならリストに加える
                            }
                        }
                    }
                    variableManager.textString = "数字キーを押して呪うプレイヤーを選択！";
                    int number = -1;
                    if(Input.GetKeyDown(KeyCode.Alpha1))
                    {
                        if(Input.GetKey(KeyCode.LeftShift) && playerObjs.Count > 9) number = 9;
                        else if(playerObjs.Count > 0) number = 0;
                    }
                    if(Input.GetKeyDown(KeyCode.Alpha2))
                    {
                        if(Input.GetKey(KeyCode.LeftShift) && playerObjs.Count > 10) number = 10;
                        else if(playerObjs.Count > 1) number = 1;
                    }
                    if(Input.GetKeyDown(KeyCode.Alpha3))
                    {
                        if(Input.GetKey(KeyCode.LeftShift) && playerObjs.Count > 11) number = 11;
                        else if(playerObjs.Count > 2) number = 2;
                    }
                    if(Input.GetKeyDown(KeyCode.Alpha4)) if(playerObjs.Count > 3) number = 3;
                    if(Input.GetKeyDown(KeyCode.Alpha5)) if(playerObjs.Count > 4) number = 4;
                    if(Input.GetKeyDown(KeyCode.Alpha6)) if(playerObjs.Count > 5) number = 5;
                    if(Input.GetKeyDown(KeyCode.Alpha7)) if(playerObjs.Count > 6) number = 6;
                    if(Input.GetKeyDown(KeyCode.Alpha8)) if(playerObjs.Count > 7) number = 7;
                    if(Input.GetKeyDown(KeyCode.Alpha9)) if(playerObjs.Count > 8) number = 8;
                    
                    if(number != null) SetMission(2, number);
                }   
            }
        }
        else if(roleNumber == 2 || roleNumber == 3)
        {
            List<GameObject> playerObjs = new List<GameObject>();
            foreach (var player in variableManager.PlayerObj)
            {
                if (player == null) continue;
                var nObj = player.GetComponent<NetworkObject>();
                if (nObj != null)
                {
                    if (!nObj.HasStateAuthority) 
                    {
                        playerObjs.Add(player); // 自分以外ならリストに加える
                    }
                }
            }

            if(cameraNumber != -1 && cameraNumber < playerObjs.Count)
            {
                transform.position = playerObjs[cameraNumber].transform.position;

                if(Input.GetKey(KeyCode.UpArrow))
                {
                    transform.rotation = playerObjs[cameraNumber].transform.rotation;
                }

                if(Input.GetKey(KeyCode.DownArrow))
                {
                    transform.rotation = playerObjs[cameraNumber].transform.rotation * Quaternion.Euler(0, 180, 0);
                }
            }
            else
            {
                cameraNumber = -1;
            }

            if(Input.GetMouseButton(1)) return;

            if(Input.GetKeyDown(KeyCode.Alpha1))
            {
                if(Input.GetKey(KeyCode.LeftShift) && playerObjs.Count > 9) { cameraNumber = cameraNumber == 9 ? -1 : 9; }
                else if(playerObjs.Count > 0) { cameraNumber = cameraNumber == 0 ? -1 : 0; }
            }
            if(Input.GetKeyDown(KeyCode.Alpha2))
            {
                if(Input.GetKey(KeyCode.LeftShift) && playerObjs.Count > 10) { cameraNumber = cameraNumber == 10 ? -1 : 10; }
                else if(playerObjs.Count > 1) { cameraNumber = cameraNumber == 1 ? -1 : 1; }
            }
            if(Input.GetKeyDown(KeyCode.Alpha3))
            {
                if(Input.GetKey(KeyCode.LeftShift) && playerObjs.Count > 11) { cameraNumber = cameraNumber == 11 ? -1 : 11; }
                else if(playerObjs.Count > 2) { cameraNumber = cameraNumber == 2 ? -1 : 2; }
            }
            if(Input.GetKeyDown(KeyCode.Alpha4)) { if(playerObjs.Count > 3) cameraNumber = cameraNumber == 3 ? -1 : 3; }
            if(Input.GetKeyDown(KeyCode.Alpha5)) { if(playerObjs.Count > 4) cameraNumber = cameraNumber == 4 ? -1 : 4; }
            if(Input.GetKeyDown(KeyCode.Alpha6)) { if(playerObjs.Count > 5) cameraNumber = cameraNumber == 5 ? -1 : 5; }
            if(Input.GetKeyDown(KeyCode.Alpha7)) { if(playerObjs.Count > 6) cameraNumber = cameraNumber == 6 ? -1 : 6; }
            if(Input.GetKeyDown(KeyCode.Alpha8)) { if(playerObjs.Count > 7) cameraNumber = cameraNumber == 7 ? -1 : 7; }
            if(Input.GetKeyDown(KeyCode.Alpha9)) { if(playerObjs.Count > 8) cameraNumber = cameraNumber == 8 ? -1 : 8; }
        }
    }

    void LateUpdate()
    {
        if (Camera.main != null && nameCanvas != null)
        {
            nameCanvas.transform.rotation = Camera.main.transform.rotation;
        }
    }

    void ApplyEmoteAnimation()
    {
        // currentEmoteの番号に応じて、Boolの値を一斉に制御する
        animator.SetBool("isHelloAnimation",    currentEmote == 1);
        animator.SetBool("isYurayuraAnimation", currentEmote == 2);
        animator.SetBool("isDanceAnimation",    currentEmote == 3);
    }

    void SetMission(int num, int subNum)
    {
        variableManager.missionNumber = num;
        variableManager.missionSubNumber = subNum;
        variableManager.canMission = false;
        variableManager.missionStart = true;
        variableManager.textString = " ";
        missionSub = -1;
    }

    void OnCollisionStay(Collision collision)
    {
        if(collision.gameObject.CompareTag("Stage"))
        {
            foreach (ContactPoint contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    isStand = true;
                    bigJump = false;
                    jumpCount = 0;
                    myCollider.sharedMaterial = defaultFriction;
                    if(animator != null) animator.SetBool("isJumpAnimation", false);
                    return;
                }
            }
        }
        if(collision.gameObject.CompareTag("Player"))
        {
            collisionObj = collision.gameObject;
            PlayerController playerController2 = collisionObj.GetComponent<PlayerController>();
            if(roleNumber == 0 && playerController2.roleNumber == 1 && stateNumber == 0)
            {
                stateNumber = 1;
                string catcherName = playerController.NetworkPlayerName.ToString(); 
                if(isEnemy) catcherName = variableManager.GetCPUName(playerController.gameObject);
                string wasCatchedName = playerController2.NetworkPlayerName.ToString();
                if(playerController2.isEnemy) wasCatchedName = variableManager.GetCPUName(playerController2.gameObject);
                variableManager.catcher = catcherName + "が" + wasCatchedName + "を捕まえた！";
                if (audioSource != null) audioSource.PlayOneShot(badAction);
                variableManager.prisonBreak = false;
                Invoke("MoveToPrison", 0.2f);
                badImageBool = true;
            }
        }
        if(collision.gameObject.CompareTag("Prison"))
        {
            if(roleNumber == 0 && stateNumber == 0)
            {
                int myNumber;
                variableManager.prisonBreak = true;
                if(isEnemy) variableManager.prisonBreaker = variableManager.GetCPUName(gameObject);
                else variableManager.prisonBreaker = NetworkPlayerName.ToString();
                goodImageBool = true;
            }
        }
        if(collision.gameObject.CompareTag("PolliceObject"))
        {
            if(roleNumber == 0)
            {
                if (audioSource != null) audioSource.PlayOneShot(badAction);
                stateNumber = 1;
                variableManager.prisonBreak = false;
                Invoke("MoveToPrison", 0.2f);
            }
        }
        if(!isEnemy || agent == null) return;
        if(collision.gameObject.CompareTag("Stage"))
        {
            if(roleNumber != 2)
            {
                LookDestinationPath(90f);
            }
            if(animator != null) animator.SetBool("isJumpAnimation", false);
            bigJump = false;
            isFalling = false;
            if (!agent.isOnNavMesh) return;
            foreach (ContactPoint contact in collision.contacts)
            {
                // 上向きの法線（地面がプレイヤーを押し上げている状態）かチェック
                // 0.6f くらいにすると、ある程度の坂道も地面として認められます
                if (contact.normal.y > 0.6f)
                {
                    isStand = true;
                    jumpCount = 0;
                    myCollider.sharedMaterial = defaultFriction;
                    return; // 地面が見つかったので終了
                }
            }
            Vector3 escapeDir = Vector3.zero;

            foreach (var contact in collision.contacts)
            {
                // 壁の法線（壁が向いている方向）
                Vector3 normal = contact.normal;

                // プレイヤーを押し返す方向（反対）
                escapeDir += normal;
            }

            escapeDir.y = 0f;
            escapeDir.Normalize();

            if (escapeDir.sqrMagnitude > 0.01f)
            {
                Vector3 local = transform.InverseTransformDirection(escapeDir);

                // Debug.Log(gameObject.name + "ExtraInput");
                if (local.z > 0.1f) PushKey(2);   // W
                else if (local.z < -0.1f) PushKey(3); // S
                else if (local.x > 0.1f) PushKey(0);  // D
                else if (local.x < -0.1f) PushKey(1); // A
                LookDestinationPath(180f);
                hasExtraInput = true;
            }
        }
        if(collision.gameObject.CompareTag("Player"))
        {
            collisionObj = collision.gameObject;
            playerController = collisionObj.GetComponent<PlayerController>();
            if(playerController != null && playerController.roleNumber == 1)
            {
                if(roleNumber == 0 && stateNumber == 0)
                {
                    audioSource.PlayOneShot(goodAction);
                    stateNumber = 1;
                    transform.position = prisonTransform.transform.position;
                    variableManager.prisonBreak = false;
                }    
            }
        }
        if(collision.gameObject.CompareTag("Prison"))
        {
            if(roleNumber == 0 && stateNumber == 0)
            {
                variableManager.prisonBreak = true;
            }
        }
    }

    void CheckStuck()
    {
        if (Vector3.Distance(transform.position, lastPosition) < 0.01f)
            stuckTimer += Time.deltaTime * 2f;
        else
            stuckTimer = 0f;

        lastPosition = transform.position;

        if (stuckTimer >= 1f)
        {
            stuckTimer -= Time.deltaTime / 2f;
            if(!isSecondTime)
            {
                isSecondTime = true;
                PushKey(4);
            }
            else
            {
                isSecondTime = false;
                transform.eulerAngles = new Vector3(transform.rotation.x, 100, transform.rotation.z);
            }
            PushKey(3); // Sキー
        }
    }

    void MoveToPrison()
    {
        transform.position = prisonTransform.transform.position;
    }

    void OnTriggerEnter(Collider collider)
    {
        if(collider.gameObject.CompareTag("Gem"))
        {
            goodImageBool = true;
            if(isEnemy) variableManager.gemGet = variableManager.GetCPUName(gameObject);
            else variableManager.gemGet = this.NetworkPlayerName.ToString();
            if (audioSource != null) audioSource.PlayOneShot(goodAction);
            variableManager.gemList[collider.gameObject.GetComponent<GemScript>().gemNumber] = 1;
            if(!variableManager.gemList.Contains(0))
            {
                variableManager.isGoalOpen = true;
            }
            collider.gameObject.GetComponent<GemScript>().effect = true;
        }
        if(collider.gameObject.CompareTag("daruma"))
        {
            goodImageBool = true;
            if (audioSource != null) audioSource.PlayOneShot(goodAction2);
            variableManager.clearMission = true;
            variableManager.missionClearPlayer = NetworkPlayerName.ToString();
        }
        if(collider.gameObject.CompareTag("Jump"))
        {
            if(roleNumber != 2)
            {
                if(!bigJump)
                {
                    if (audioSource != null) audioSource.PlayOneShot(bigJumpAudio);
                    if(animator != null) animator.SetBool("isJumpAnimation", true);
                    StartCoroutine(JumpCubeAnimation(collider.gameObject));
                    bigJump = true;
                    Vector3 vel = rb.velocity;
                    vel.y = 0;
                    rb.velocity = vel;
                    rb.AddForce(Vector3.up * jumpForce * 1.53f, ForceMode.Impulse);
                    isStand = false;
                    myCollider.sharedMaterial = noFriction; 
                }
            }
        }
        if(collider.gameObject.CompareTag("Jump2"))
        {
            if(roleNumber != 2)
            {
                if(!bigJump)
                {
                    if (audioSource != null) audioSource.PlayOneShot(bigJumpAudio);
                    if(animator != null) animator.SetBool("isJumpAnimation", true);
                    StartCoroutine(JumpCubeAnimation(collider.gameObject));
                    bigJump = true;
                    Vector3 vel = rb.velocity;
                    vel.y = 0;
                    rb.velocity = vel;
                    rb.AddForce(Vector3.up * jumpForce * 2.2f, ForceMode.Impulse);
                    isStand = false;
                    myCollider.sharedMaterial = noFriction; 
                }
            }
        }
        if(collider.gameObject.CompareTag("RoleChange"))
        {
            if(collider.name == "ToThief")
            {
                roleNumber = 0;
            }
            if(collider.name == "ToPollice")
            {
                roleNumber = 1;
            }
            if(collider.name == "ToDaruma")
            {
                roleNumber = 2;
            }
            if (audioSource != null) audioSource.PlayOneShot(actionAudio, 0.7f);
            variableManager.isSetName = 0;
            variableManager.RequestSetActiveItem();
        }
        if(!isEnemy || agent == null) return;
        if(collider.gameObject.tag == "Gem")
        {
            variableManager.gemList[collider.gameObject.GetComponent<GemScript>().gemNumber] = 1;
            if(!variableManager.gemList.Contains(0))
            {
                variableManager.isGoalOpen = true;
            }
            collider.gameObject.GetComponent<GemScript>().effect = true;
        }
        if(collider.gameObject.tag == "daruma")
        {
            variableManager.clearMission = true;
        }
        if(collider.gameObject.tag == "Jump")
        {
            if(roleNumber != 2)
            {
                LookDestinationPath(360f);
                if(!bigJump)
                {
                    bigJump = true;
                    Vector3 vel = rb.velocity;
                    vel.y = 0;
                    rb.velocity = vel;
                    rb.AddForce(Vector3.up * jumpForce * 1.5f, ForceMode.Impulse);
                    isStand = false;
                    myCollider.sharedMaterial = noFriction;
                    if(animator != null) animator.SetBool("isJumpAnimation", true);
                }
            }
        }
        if(collider.gameObject.tag == "Jump2")
        {
            if(roleNumber != 2)
            {
                LookDestinationPath(360f);
                if(!bigJump)
                {
                    bigJump = true;
                    Vector3 vel = rb.velocity;
                    vel.y = 0;
                    rb.velocity = vel;
                    rb.AddForce(Vector3.up * jumpForce * 2.2f, ForceMode.Impulse);
                    isStand = false;
                    myCollider.sharedMaterial = noFriction;   
                    if(animator != null) animator.SetBool("isJumpAnimation", true);
                }
            }
        }
        if(collider.gameObject.tag == "haveJump")
        {
            if(roleNumber != 2)
            {
                LookDestinationPath(360f);
            }
            wishJump = true;
        }
    }
    
    void OnTriggerStay(Collider collider)
    {
        if(collider.gameObject.CompareTag("Jump"))
        {
            if(roleNumber != 2)
            {
                Vector3 vel = rb.velocity;
                if(bigJump && vel.y < 0)
                {
                    if (audioSource != null) audioSource.PlayOneShot(bigJumpAudio);
                    StartCoroutine(JumpCubeAnimation(collider.gameObject));
                    vel.y = 0;
                    rb.velocity = vel;
                    rb.AddForce(Vector3.up * jumpForce * 1.53f, ForceMode.Impulse);
                    isStand = false;
                    myCollider.sharedMaterial = noFriction; 
                }
            }
        }
        if(collider.gameObject.CompareTag("Jump2"))
        {
            if(roleNumber != 2)
            {
                Vector3 vel = rb.velocity;
                if(bigJump && vel.y < 0)
                {
                    if (audioSource != null) audioSource.PlayOneShot(bigJumpAudio);
                    StartCoroutine(JumpCubeAnimation(collider.gameObject));
                    vel.y = 0;
                    rb.velocity = vel;
                    rb.AddForce(Vector3.up * jumpForce * 2.2f, ForceMode.Impulse);
                    isStand = false;
                    myCollider.sharedMaterial = noFriction; 
                }
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if(collision.gameObject.CompareTag("Stage"))
        {
            isStand = false;
            myCollider.sharedMaterial = noFriction;
            if(animator != null) animator.SetBool("isJumpAnimation", true);
        }
        if(!isEnemy || agent == null) return;
        if(collision.gameObject.tag == "Stage")
        {
            isStand = false;
            myCollider.sharedMaterial = noFriction;
            if(animator != null) animator.SetBool("isJumpAnimation", true);
        }
    }

    private void OnNameChanged()
    {
        if(!isEnemy) nameText.text = NetworkPlayerName.ToString();
    }

    IEnumerator JumpCubeAnimation(GameObject jumpObj)
    {
        Vector3 initialScale = jumpObj.transform.localScale;
        float duration = 1.0f; // アニメーション全体の時間
        float elapsed = 0f;
        float maxY = 5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float damping = Mathf.Exp(-4f * t); // 時間経過で揺れを小さくする
            float bounce = Mathf.Sin(t * Mathf.PI * 8f) * damping;
            float yOffset = bounce * maxY;
            jumpObj.transform.localScale = initialScale + new Vector3(0, yOffset, 0);

            yield return null; // 1フレーム待つ
        }

        jumpObj.transform.localScale = initialScale;
    }

    public override void Spawned()
    {
        stateNumber = 0;
        isMarked = false;
        Camera cam = GetComponentInChildren<Camera>(true);
        if (cam != null && !Object.HasInputAuthority)
        {
            cam.gameObject.SetActive(false);
        }
        var safe = FindFirstObjectByType<StrongBoxScript>();
        if (safe != null)
        {
            safe.AddPlayer(this.gameObject);
        }
        var safe2 = FindFirstObjectByType<GemScript>();
        if (safe2 != null)
        {
            safe2.AddPlayer(this.gameObject);
        }
        if (Object.HasStateAuthority)
        {
            NetworkPlayerName = PlayerData.PlayerName;
        }
    }

    public override void Render()
    {
        if(!isEnemy) nameText.text = NetworkPlayerName.ToString();
    }

    void ConsiderMove()
    {
        if (!hasExtraInput)
        {
            for(int i = 0; i < pushedKey.Length; i++)
                pushedKey[i] = "";
        }
        hasExtraInput = false;
        
        switch (_state)
        {
            case myState.Idle:
                if(roleNumber != 2 && variableManager.missionNumber != -1)
                {
                    darumaTransformSet = false;
                    _state = myState.Mission;
                }
                if(roleNumber == 0)
                {
                    if(GetNearest1Player() != null && distance <= 100 && IsTargetInSight(GetNearest1Player()))
                    {
                        _state = myState.Escape;
                        break;
                    }
                    if(!variableManager.prisonBreak && stateNumber == 0)
                    {
                        float distA = Vector3.Distance(agent.transform.position, prisonTransform.position);
                        float distB = 10000f;
                        if(GetNearestStrongBox() != null) distB = Vector3.Distance(agent.transform.position, GetNearestStrongBox().transform.position);
                        if(distA < distB || distB > 100f)
                        {
                            _state = myState.Rescue;
                            break;   
                        }
                    }
                    _state = myState.Steal;
                    break;                  
                }
                if(roleNumber == 1)
                {
                    if(IsTargetInSight(GetNearest0Player()))
                    {
                        _state = myState.Chasing;   
                    }
                    else
                    {
                        MoveToID(randomObj.transform.position);
                    }
                    break;
                }
                if(roleNumber == 2)
                {
                    int nullCount = 0;
                    for(int i = 0; i < playersTransform.Length; i ++)
                    {
                        if(transformTimer[i] != null && transformTimer[i] > 20f)
                        {
                            playersTransform[i] = null;
                        }
                        if(playersTransform[i] == null)
                        {
                            nullCount ++;
                            if(IsTargetInSight(playersObj[i]))
                            {
                                playersTransform[i] = playersObj[i].transform;
                                transformTimer[i] = 0f;
                            }
                            if(playersObj[i].transform == gameObject.transform)
                            {
                                playersTransform[i] = playersObj[i].transform;
                            }
                        }
                    }
                    Vector3 dir = randomObj.transform.position - transform.position;
                    dir.y = 0;
                    if (dir.sqrMagnitude < 0.001f) return;
                    // 回転も入力も同じ基準にする
                    Vector3 localDir = transform.InverseTransformDirection(dir);
                    // 回転
                    Quaternion targetRot = Quaternion.LookRotation(dir);
                    transform.rotation = Quaternion.RotateTowards(
                        transform.rotation,
                        targetRot,
                        540f * Time.deltaTime // 少し強める
                    );
                    if (localDir.z > 0.1f) PushKey(2);
                    if (localDir.x > 0.3f) PushKey(0);
                    if (localDir.x < -0.3f) PushKey(1);
                    if (localDir.z <= 0.1f && localDir.x <= 0.3f && localDir.x >= -0.3f) PushKey(3);
                    
                    if (randomObj.transform.position.y > transform.position.y) PushKey(4);
                    else if(transform.position.y - randomObj.transform.position.y >= 10) PushKey(5);
                    if(nullCount == 0 && variableManager.canMission)
                    {
                        darumaTransformSet = false;
                        _state = myState.Mission;
                        break;
                    }
                }
                break;

            case myState.Steal:
                if(roleNumber != 2 && variableManager.missionNumber != -1)
                {
                    darumaTransformSet = false;
                    _state = myState.Mission;
                    break;
                }
                if(!variableManager.gemList.Contains(0))
                {
                    _state = myState.Goal;
                    break;
                }
                if(GetNearest1Player() && distance <= 150 && IsTargetInSight(GetNearest1Player()))
                {
                    float distB = Vector3.Distance(agent.transform.position, GetNearestStrongBox().transform.position);
                    if(distB > 10f)
                    {
                        _state = myState.Rescue;
                        break;   
                    }
                }
                if(GetNearest1Player() && distance <= 100 && IsTargetInSight(GetNearest1Player()))
                {
                    _state = myState.Escape;
                    break;
                }
                if(!variableManager.prisonBreak && stateNumber == 0)
                {
                    float distA = Vector3.Distance(agent.transform.position, prisonTransform.position);
                    float distB = 10000f;
                    if(GetNearestStrongBox() != null) distB = Vector3.Distance(agent.transform.position, GetNearestStrongBox().transform.position);
                    if(distA < distB || distB > 100f)
                    {
                        _state = myState.Rescue;
                        break;   
                    }
                }
                if(GetNearestStrongBox())
                {
                    GameObject targetStrongBox = GetNearestStrongBox();
                    if(distance <= 100)
                    {
                        PushKey(6);
                    }
                    MoveToID(targetStrongBox.transform.position);
                    break;
                }
                _state = myState.Idle;
                break; 

            case myState.Goal:
                if(GetNearest1Player() && distance <= 100 && IsTargetInSight(GetNearest1Player()))
                {
                    _state = myState.Escape;
                    break;
                }
                MoveToID(aiGoalTransform.position);
                break;

            case myState.Chasing:
                GameObject target = GetNearest0Player();
                if (target != null && IsTargetInSight(target)) 
                {
                    if(variableManager.missionNumber != -1 && Vector3.Distance(agent.transform.position, target.transform.position) > 10f)
                    {
                        darumaTransformSet = false;
                        _state = myState.Mission;
                        break;
                    }
                    else
                    {
                        MoveToID(target.transform.position);
                        targetObj = target;
                        randomObj.transform.position = targetObj.transform.position;   
                    }
                }
                else
                {
                    // 見失ったらその場で止まるか、パトロールに切り替える
                    if(_state == myState.Chasing) _state = myState.Idle;
                    chaseTimer = 0;
                }
                break;

            case myState.Escape:
                if(roleNumber != 2 && variableManager.missionNumber != -1)
                {
                    darumaTransformSet = false;
                    _state = myState.Mission;
                }
                GameObject police = GetNearest1Player();
                if(animator != null && !animator.GetBool("isStandAnimation"))
                {
                    if(animator != null) animator.SetBool("isStandAnimation", true);
                    speed = 7f;
                }
                if(distance > 100)
                {
                    _state = myState.Idle;
                    break;
                }
                if (police != null)
                {
                    // 1. 基本は警察と反対方向
                    Vector3 runDirection = (transform.position - police.transform.position).normalized;
                    Vector3 escapeTarget = transform.position + runDirection * 8f; // 少し遠めを目標にする

                    // 2. 逃げ先が「角」や「壁の外」でないかチェック
                    UnityEngine.AI.NavMeshHit hit;
                    // ターゲット地点の半径5m以内で、NavMesh上の有効な場所を探す
                    if (UnityEngine.AI.NavMesh.SamplePosition(escapeTarget, out hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
                    {
                        escapeTarget = hit.position;
                    }
                    else
                    {
                        // 3. もし真後ろが行き止まりなら、左右45度方向に逃げ道がないか試す
                        Vector3 leftPath = Quaternion.Euler(0, 45, 0) * runDirection;
                        Vector3 rightPath = Quaternion.Euler(0, -45, 0) * runDirection;
                        
                        // 左斜め後ろをチェック
                        if (UnityEngine.AI.NavMesh.SamplePosition(transform.position + leftPath * 8f, out hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
                        {
                            escapeTarget = hit.position;
                        }
                        // 右斜め後ろをチェック
                        else if (UnityEngine.AI.NavMesh.SamplePosition(transform.position + rightPath * 8f, out hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
                        {
                            escapeTarget = hit.position;
                        }
                    }

                    MoveToID(escapeTarget);
                }
                break;

            case myState.Mission:
                if(roleNumber == 2)
                {
                    int nullCount = 0;
                    for(int i = 0; i < playersTransform.Length; i ++)
                    {
                        if(playersTransform[i] == null)
                        {
                            nullCount ++;
                            if(IsTargetInSight(playersObj[i]))
                            {
                                playersTransform[i] = playersObj[i].transform;
                            }
                            if(playersObj[i].transform == gameObject.transform)
                            {
                                playersTransform[i] = playersObj[i].transform;
                            }
                        }
                    }
                    if(nullCount != 0)
                    {
                        _state = myState.Idle;
                        break;
                    }
                    if(!variableManager.canMission)
                    {
                        _state = myState.Idle;
                        break;
                    }
                    if(variableManager.canMission)
                    {
                        PushKey(6);
                        PushKey(7);
                        wishMissionNumber = 0;
                    }
                    if(wishMissionNumber == 0)
                    {
                        int right = 0, left = 0, up = 0, down = 0;

                        for (int i = 0; i < 5; i++)
                        {
                            if (playersTransform[i] == null) continue;
                            if (playersTransform[i].position.x > 0) right++;
                            else left++;

                            if (playersTransform[i].position.z > 0) up++;
                            else down++;
                        }

                        int averageDirection;

                        if (right > left && right > up && right > down) averageDirection = 0;
                        else if (left > up && left > down) averageDirection = 1;
                        else if (up > down) averageDirection = 2;
                        else averageDirection = 3;
                        
                        if(averageDirection == 0) //右
                        {
                            PushKey(9); //左
                        }
                        if(averageDirection == 1) //左
                        {
                            PushKey(8); //右
                        }
                        if(averageDirection == 2) //上
                        {
                            PushKey(11); //下
                        }
                        if(averageDirection == 3) //下
                        {
                            PushKey(10); //上
                        }
                    }
                    break;
                }
                if(variableManager.missionNumber == 0)
                {
                    //Debug.Log(gameObject.name + "Go to DARUMA");
                    //Debug.Log((darumaTransform.position).ToString());
                    float minDist = 10000f;
                    int number = 0;
                    if(darumaTransformSet)
                    {
                        for(int i = 0; i < darumaTransform.Length; i ++)
                        {
                            if(minDist - randomDis > Vector3.Distance(agent.transform.position, darumaTransform[i].transform.position))
                            {
                                minDist = Vector3.Distance(agent.transform.position, darumaTransform[i].transform.position);
                                number = i;
                            }
                        }   
                    }
                    else
                    {
                        darumaTransformSet = true;
                        for(int i = 0; i < darumaTransform.Length; i ++)
                        {
                            if(minDist > Vector3.Distance(agent.transform.position, darumaTransform[i].transform.position))
                            {
                                minDist = Vector3.Distance(agent.transform.position, darumaTransform[i].transform.position);
                                number = i;
                            }
                        }
                        randomDis = Random.Range(1f, 100f);
                    }
                    MoveToID(darumaTransform[number].position);
                }
                if(roleNumber == 0 && GetNearest1Player() && distance <= 100f && IsTargetInSight(GetNearest1Player()))
                {
                    _state = myState.Escape;
                    break;
                }
                if(variableManager.missionNumber == -1)
                {
                    darumaTransformSet = false;
                    _state = myState.Idle;
                    break;
                }
                break;

            case myState.Rescue:
                if(GetNearest1Player() && distance <= 200 && IsTargetInSight(GetNearest1Player()))
                {
                    _state = myState.Escape;
                    break;
                }
                if(variableManager.prisonBreak || stateNumber == 1)
                {
                    _state = myState.Idle;
                    break;
                }
                MoveToID(prisonTransform.position);
                break;

            default:
                break;
        }
    }

    Vector3 GetSteeringDirection(UnityEngine.AI.NavMeshPath path)
    {
        if (path.corners == null || path.corners.Length < 2)
            return Vector3.zero;

        // 一番近い次のコーナーを探す
        for (int i = 0; i < path.corners.Length - 1; i++)
        {
            float dist = Vector3.Distance(transform.position, path.corners[i]);

            if (dist > 0.5f)
            {
                return path.corners[i] - transform.position;
            }
        }

        return path.corners[1] - transform.position;
    }

    void MoveToID(Vector3 destination)
    {
        if (agent == null)
        {
            Debug.LogError($"{gameObject.name}: NavMeshAgentがありません");
            return;
        }

        if (!agent.isActiveAndEnabled)
        {
            Debug.LogError($"{gameObject.name}: NavMeshAgentが無効です");
            return;
        }

        if (!agent.isOnNavMesh)
        {
            Debug.LogError($"{gameObject.name}: NavMesh上にいません");
            return;
        }
        nowDestination = destination;

        if (!isStand || isFalling)
        {
            if(roleNumber != 2)
            {
                LookDestinationPath(Random.Range(90f, 360f));
            }
            PushKey(2);
            return;
        }

        if (!agent.isOnNavMesh) return;
        if (hasExtraInput)
        {
            LookDestinationPath(90);
            return;
        }
        if (IsDropRequired(destination))
        {
            StartDrop();
            PushKey(2);
            return;
        }

        UnityEngine.AI.NavMeshHit hit;
        Vector3 validDestination = destination;

        if (UnityEngine.AI.NavMesh.SamplePosition(destination, out hit, 5.0f, UnityEngine.AI.NavMesh.AllAreas))
        {
            validDestination = hit.position;
        }

        // 移動先に到着していたら、移動処理を終了
        Vector3 toDestination = validDestination - transform.position;
        toDestination.y = 0f;

        if (toDestination.magnitude < 1.0f)
        {
            return;
        }

        // ルート比較
        UnityEngine.AI.NavMeshPath newPath = new UnityEngine.AI.NavMeshPath();
        bool canCalc = UnityEngine.AI.NavMesh.CalculatePath(transform.position, validDestination, UnityEngine.AI.NavMesh.AllAreas, newPath);

        bool shouldSet = false;

        if (canCalc && newPath.status != UnityEngine.AI.NavMeshPathStatus.PathInvalid)
        {
            float newLen = GetPathLength(newPath);
            float currentLen = GetPathLength(agent.path);

            float threshold = 10.0f;

            if (!agent.hasPath || newLen < currentLen - threshold || agent.pathStatus != UnityEngine.AI.NavMeshPathStatus.PathComplete)
            {
                shouldSet = true;
            }
        }
        else
        {
            if (!agent.hasPath)
            {
                shouldSet = true;
            }
        }

        if (shouldSet || Vector3.Distance(agent.destination, validDestination) > 0.5f)
        {
            agent.SetDestination(validDestination);
            lastDestination = validDestination;
        }

        if (agent.pathStatus == UnityEngine.AI.NavMeshPathStatus.PathInvalid && !agent.pathPending)
        {
            agent.SetDestination(validDestination);
        }

        UnityEngine.AI.NavMeshPath path = agent.path;
        if (path.corners != null && path.corners.Length >= 2)
        {
            targetDir = path.corners[1] - transform.position;
        }
        else
        {
            targetDir = validDestination - transform.position;
        }

        targetDir.y = 0f;
        targetDir.Normalize();
        targetRot = Quaternion.LookRotation(targetDir);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRot,
            180f * Time.deltaTime
        );

        Vector3 localDir = transform.InverseTransformDirection(targetDir);
        if (isStand)
        {
            if (localDir.z > 0.1f) PushKey(2);
            if (localDir.x > 0.3f) PushKey(0);
            if (localDir.x < -0.3f) PushKey(1);
            if (localDir.z <= 0.1f && localDir.x <= 0.3f && localDir.x >= -0.3f) PushKey(3);
        }
    }

    GameObject GetNearest0Player()
    {
        // "Player"タグが付いているすべてのオブジェクトを取得
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject nearestPlayer = null;
        float minDistance = float.MaxValue;
        Vector3 currentPos = transform.position;

        foreach (GameObject p in players)
        {
            if (p == gameObject) continue;
            playerController = p.gameObject.GetComponent<PlayerController>();
            if(playerController != null && (playerController.roleNumber == 2 || playerController.roleNumber == 1 || playerController.stateNumber == 1)) continue;
            // 距離を計算（平方根の計算を避けるためsqrMagnitudeを使うと高速です）
            float dist = (p.transform.position - currentPos).sqrMagnitude;
            
            if (dist < minDistance)
            {
                minDistance = dist;
                nearestPlayer = p;
            }
        }

        return nearestPlayer;
    }

    GameObject GetNearest1Player()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject nearest1Player = null;
        float minDistance = float.MaxValue;
        Vector3 currentPos = transform.position;

        foreach (GameObject p in players)
        {
            if (p == gameObject) continue;

            // 相手のコンポーネントを取得
            PlayerController pc = p.GetComponent<PlayerController>();
            PlayerController epc = p.GetComponent<PlayerController>();

            // 警察(roleNumber 1)かどうかを判定
            bool isPolice = (pc != null && pc.roleNumber == 1) || (epc != null && epc.roleNumber == 1);

            // 警察じゃないなら無視
            if (!isPolice) continue;

            float dist = (p.transform.position - currentPos).sqrMagnitude;
            
            if (dist < minDistance)
            {
                minDistance = dist;
                nearest1Player = p;
            }
        }

        // 全員チェックし終わったあと、一番近い警察との距離を代入
        distance = minDistance; 

        // デバッグログ：見つからなかった場合は "None" と出す
        // Debug.Log($"{gameObject.name} nearest police distance: {distance} (Target: {(nearest1Player != null ? nearest1Player.name : "None")})");

        return nearest1Player;
    }

    GameObject GetNearestStrongBox()
    {
        GameObject[] boxs = GameObject.FindGameObjectsWithTag("StrongBox");
        GameObject nearestGem= null;
        float minDistance = float.MaxValue;
        Vector3 currentPos = transform.position;

        foreach (GameObject b in boxs)
        {
            if (variableManager.gemList[b.GetComponent<StrongBoxScript>().boxNumber] == 1) continue;

            float dist = (b.transform.position - currentPos).sqrMagnitude;
            
            if (dist < minDistance)
            {
                minDistance = dist;
                foreach (Transform child in b.transform)
                {
                    if (child.CompareTag("Gem"))
                    {
                        nearestGem = child.gameObject;
                    }
                }
            }
        }

        distance = minDistance; 

        return nearestGem;
    }



    void CheckAndAutoJump()
    {
        if (!isStand) return;
        if (jumpCount >= jumpCountLimit) return;

        if (wishJump)
        {
            PushKey(4);
            PushKey(2);
            return;
        }

        Vector3 origin =
            transform.position +
            Vector3.up * 0.1f +
            transform.forward * 0.2f;

        Vector3 dir = transform.forward;

        float radius = 0.45f;
        float distance = obstacleCheckDistance + 2f;

        RaycastHit[] hits =
            Physics.SphereCastAll(origin, radius, dir, distance);

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var h in hits)
        {
            if (h.collider.gameObject == gameObject) continue;
            if (!h.collider.CompareTag("Stage")) continue;

            // =========================
            // 接触地点の高さを見る
            // =========================
            float hitHeight = h.point.y;

            float myBottom =
                transform.position.y -
                transform.localScale.y / 2f;

            float heightDiff = hitHeight - myBottom;

            // Debug.Log(heightDiff);

            if (heightDiff > 0.02f &&
                heightDiff <= obstacleCheckHeight)
            {
                // =========================
                // 頭上チェック
                // =========================
                Vector3 upperOrigin =
                    transform.position +
                    Vector3.up * 1.3f;

                RaycastHit[] upperHits =
                    Physics.SphereCastAll(
                        upperOrigin,
                        0.35f,
                        dir,
                        1.2f
                    );

                bool blocked = false;

                foreach (var uh in upperHits)
                {
                    if (uh.collider.gameObject == gameObject)
                        continue;

                    if (uh.collider.CompareTag("Stage"))
                    {
                        blocked = true;
                        break;
                    }
                }

                if (blocked) return;

                PushKey(4);
                return;
            }
        }
    }

    bool IsTargetInSight(GameObject target)
    {
        if (target == null) return false;

        Vector3 targetPos = target.transform.position;
        Vector3 myPos = transform.position + Vector3.up * 1.5f + Vector3.back * 1f;
        Vector3 directionToTarget = (targetPos - myPos).normalized;

        float distanceToTarget = Vector3.Distance(myPos, targetPos);
        if (distanceToTarget > 1000f) return false;

        float angle = Vector3.Angle(transform.forward, directionToTarget);
        if (angle > 50f) return false;

        // 全てのヒット情報を取得（自分も他人も含めて全部突き抜ける）
        RaycastHit[] hits = Physics.RaycastAll(myPos, directionToTarget, distanceToTarget);
        
        // ヒットしたものを距離が近い順に並び替える
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var hit in hits)
        {
            // 自分自身に当たった場合は無視して次のヒットへ（突き抜ける処理）
            if (hit.collider.gameObject == gameObject) continue;

            // 自分以外で最初に当たったのがターゲットなら「見える」
            if (hit.collider.gameObject == target)
            {
                return true;
            }
            else
            {
                // ターゲットより手前に自分以外の何か（壁など）があったら遮蔽されている
                return false;
            }
        }

        return false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 myPos = transform.position + Vector3.up * 1.5f;
        
        // 前方の視界限界線を書く
        Vector3 leftRayDirection = Quaternion.AngleAxis(-30, Vector3.up) * transform.forward;
        Vector3 rightRayDirection = Quaternion.AngleAxis(30, Vector3.up) * transform.forward;

        Gizmos.DrawRay(myPos, leftRayDirection * 60f);
        Gizmos.DrawRay(myPos, rightRayDirection * 60f);
        Gizmos.DrawLine(myPos + leftRayDirection * 60f, myPos + rightRayDirection * 60f);
    }

    void PushKey(int pushKeyNumber)
    {
        if(pushKeyNumber == 0)
        {
            pushedKey[pushKeyNumber] = "D";
        }
        if(pushKeyNumber == 1)
        {
            pushedKey[pushKeyNumber] = "A";
        }
        if(pushKeyNumber == 2)
        {
            pushedKey[pushKeyNumber] = "W";
        }
        if(pushKeyNumber == 3)
        {
            pushedKey[pushKeyNumber] = "S";
        }
        if(pushKeyNumber == 4)
        {
            pushedKey[pushKeyNumber] = "Space";
        }
        if(pushKeyNumber == 5)
        {
            pushedKey[pushKeyNumber] = "LShift";
        }
        if(pushKeyNumber == 6)
        {
            pushedKey[pushKeyNumber] = "Mouse1";
        }
        if(pushKeyNumber == 7)
        {
            pushedKey[pushKeyNumber] = "1";
        }
        if(pushKeyNumber == 8)
        {
            pushedKey[pushKeyNumber] = "RArrow";
        }
        if(pushKeyNumber == 9)
        {
            pushedKey[pushKeyNumber] = "LArrow";
        }
        if(pushKeyNumber == 10)
        {
            pushedKey[pushKeyNumber] = "UArrow";
        }
        if(pushKeyNumber == 11)
        {
            pushedKey[pushKeyNumber] = "DArrow";
        }
    }

    bool IsDropRequired(Vector3 destination)
    {
        float heightDiff = transform.position.y - destination.y;
        if (heightDiff <= 1.5f) return false;

        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Vector3 forward = transform.forward;

        // 前に壁があるか
        if (Physics.Raycast(origin, forward, 1.0f))
        {
            // 壁なら落ちない
            return false;
        }

        // 壁が無くて高さ差あり → 落下
        return true;
    }

    void StartDrop()
    {
        isFalling = true;
    }

    float GetPathLength(UnityEngine.AI.NavMeshPath path)
    {
        if (path == null || path.corners == null || path.corners.Length < 2)
            return float.MaxValue;

        float length = 0f;
        for (int i = 1; i < path.corners.Length; i++)
        {
            length += Vector3.Distance(path.corners[i - 1], path.corners[i]);
        }
        return length;
    }
    
    void LookDestinationPath(float rotSpeed)
    {
        if(agent == null) return;
        UnityEngine.AI.NavMeshHit hit;
        Vector3 validDestination = nowDestination;

        if (UnityEngine.AI.NavMesh.SamplePosition(nowDestination, out hit, 5.0f, UnityEngine.AI.NavMesh.AllAreas))
        {
            validDestination = hit.position;
        }

        // ルート比較
        UnityEngine.AI.NavMeshPath newPath = new UnityEngine.AI.NavMeshPath();
        bool canCalc = UnityEngine.AI.NavMesh.CalculatePath(transform.position, validDestination, UnityEngine.AI.NavMesh.AllAreas, newPath);

        bool shouldSet = false;

        if (canCalc && newPath.status != UnityEngine.AI.NavMeshPathStatus.PathInvalid)
        {
            float newLen = GetPathLength(newPath);
            float currentLen = GetPathLength(agent.path);

            float threshold = 50.0f;

            if (!agent.hasPath || newLen < currentLen - threshold || agent.pathStatus != UnityEngine.AI.NavMeshPathStatus.PathComplete)
            {
                shouldSet = true;
            }
        }
        else
        {
            if (!agent.hasPath)
            {
                shouldSet = true;
            }
        }

        if (shouldSet || Vector3.Distance(agent.destination, validDestination) > 0.5f)
        {
            if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                agent.SetDestination(validDestination);
            }
            lastDestination = validDestination;
        }

        if (agent.pathStatus == UnityEngine.AI.NavMeshPathStatus.PathInvalid && !agent.pathPending)
        {
            if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                agent.SetDestination(validDestination);
            }
        }

        UnityEngine.AI.NavMeshPath path = agent.path;
        if (path.corners != null && path.corners.Length >= 2)
        {
            targetDir = path.corners[1] - transform.position;
        }
        else
        {
            targetDir = validDestination - transform.position;
        }

        targetDir.y = 0;
        if (targetDir.sqrMagnitude > 0.01f)
        {
            // ★ここで計算した方向を回転値に変換する
            Quaternion targetRot = Quaternion.LookRotation(targetDir);

            if (Quaternion.Angle(transform.rotation, targetRot) > 50f)
            {
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation, targetRot, rotSpeed * Time.deltaTime);
            }
            else
            {
                transform.rotation = targetRot;
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if(!isEnemy || agent == null) return;
        if (collision.gameObject.CompareTag("Stage"))
        {
            Vector3 agentPos = agent.nextPosition;
            Vector3 realPos = transform.position;
            float dist = Vector3.Distance(agentPos, realPos);
            if (dist >= 1.0f)
            {
                agent.Warp(transform.position);
            }
            if(roleNumber != 2)
            {
                LookDestinationPath(90f);
            }
            if (isFalling)
            {
                isFalling = false;
                UnityEngine.AI.NavMeshHit hit;
                if (!agent.isOnNavMesh) return;
            }
        }
    }

    void OnTriggerExit(Collider collider)
    {
        if(!isEnemy || agent == null) return;
        if(collider.gameObject.tag == "haveJump")
        {
            wishJump = false;
        }
    }

    void OnDrawGizmos()
    {
        if(!isEnemy || agent == null) return;
        if (agent == null || agent.path == null) return;

        var path = agent.path;

        for (int i = 0; i < path.corners.Length - 1; i++)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(path.corners[i], path.corners[i + 1]);
        }
    }
}
