using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using Fusion;
using Fusion.Sockets;
using System.Dynamic;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] Rigidbody rb;
    [SerializeField] GameObject cameraObj;
    [SerializeField] Material[] materials;
    GameObject ManagerObj;
    GameObject[] prisonObj;
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
    Transform prisonTransform;
    CameraController cameraController;
    VariableManager variableManager;
    GameObject collisionObj;
    AudioSource audioSource;
    public Animator animator;
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
    private GameObject targetDaruma;
    float followSpeed = 1.5f;
    float rotateSpeed = 25.0f; // 回転速度
    string obstacleTag = "Stage";

    // ★他人の画面でもアニメーションを動かすために、ネットワーク変数(各画面で同期される)にします
    [Networked] float x { get; set; }
    [Networked] float z { get; set; }

    void Start()
    {
        Debug.Log($"{name} : {HasInputAuthority}");
        ManagerObj = FindFirstObjectByType<VariableManager>().gameObject;
        NetworkObj = GameObject.Find("NetworkRunner");
        prisonObj = GameObject.FindGameObjectsWithTag("Prison");
        prisonTransform = GameObject.Find("Prison").transform;
        cameraNumber = -1;
        audioSource = GetComponent<AudioSource>();
        jumpForce = 13;
        jumpCount = 0;
        jumpCountLimit = 1;
        missionSub = -1;
        canMove = true;
        runAnimationBool = false;
        jogJumpRequest = false;
        variableManager = ManagerObj.GetComponent<VariableManager>();
    }

    // ★確実に動くように、シンプルな入力同期処理にします
    public override void FixedUpdateNetwork()
    {
        // 自分が操作している画面のときだけ、キーボードの入力を反映してネットワーク変数に入れる
        if (HasInputAuthority && canMove)
        {
            //x = Input.GetAxisRaw("Horizontal");
            //z = Input.GetAxisRaw("Vertical");
            if (Input.GetKey(KeyCode.D)) x = 1;
            if (Input.GetKey(KeyCode.A)) x = -1;
            if (Input.GetKey(KeyCode.W)) z = 1;
            if (Input.GetKey(KeyCode.S)) z = -1;
            if (!Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.A)) x = 0;
            if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S)) z = 0;

            if(Joystick != null) x = Joystick.GetComponent<JoystickScript>().moveDirection.x / 13f;
            if(Joystick != null) z = Joystick.GetComponent<JoystickScript>().moveDirection.y / 13f;
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

                ExecuteJump();

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
        // 移動の計算は全員の画面で実行する（xとzがネットワーク変数なので他人にも同期されます）
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
        if(cooldownTimer > 0) cooldownTimer -= Time.deltaTime;
        if(startCount < 0.1f)
        {
            startCount += Time.deltaTime;
            return;
        }
        if(!isRoleCheck && roleNumber != -1)
        {
            if(animator != null)
            {
                Destroy(animator.gameObject);
            }
            isRoleCheck = true;
            roleNumber2 = roleNumber;
            if(roleNumber == 0)
            {
                gameObject.layer = 6;
                int thiefSkinNumber2 = NetworkObj.GetComponent<StartFusion>().thiefSkinNumber;
                animator = Instantiate(thiefBlenderObj[thiefSkinNumber2], gameObject.transform.position + new Vector3(0, 0.1f, 0),
                gameObject.transform.rotation * Quaternion.Euler(new Vector3(0, 180, 0)), gameObject.transform).GetComponent<Animator>();
                animator.gameObject.transform.localScale = new Vector3(0.3f, 0.15f, 0.3f);
                animator.gameObject.SetActive(true);
            }
            if(roleNumber == 1)
            {
                gameObject.layer = 7;
                int polliceSkinNumber2 = NetworkObj.GetComponent<StartFusion>().polliceSkinNumber;
                animator = Instantiate(polliceBlenderObj[polliceSkinNumber2], gameObject.transform.position + new Vector3(0, 0.1f, 0),
                gameObject.transform.rotation * Quaternion.Euler(new Vector3(0, 180, 0)), gameObject.transform).GetComponent<Animator>();
                animator.gameObject.transform.localScale = new Vector3(0.3f, 0.15f, 0.3f);
                animator.gameObject.SetActive(true);
            }
            if(roleNumber == 2)
            {
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

            if(Joystick == null && GameObject.Find("Joystick") != null)
            {
                Joystick = GameObject.Find("Joystick").GetComponent<Image>();
            }

            if(roleNumber == 2)
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
            if(roleNumber == 1)
            {
                for(int i = 0; i < 6; i ++)
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
            if(roleNumber != 3) GetComponent<Renderer>().material = materials[roleNumber];
            variableManager.isRoleChange = true;
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

        if (animator != null && animator.runtimeAnimatorController != null)
        {
            if (HasInputAuthority)
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
            PlayerController playerController = collisionObj.GetComponent<PlayerController>();
            if(roleNumber == 0 && playerController.roleNumber == 1)
            {
                if (audioSource != null) audioSource.PlayOneShot(badAction);
                stateNumber = 1;
                variableManager.prisonBreak = false;
                Invoke("MoveToPrison", 0.2f);
            }
        }
        if(collision.gameObject.CompareTag("Prison"))
        {
            if(roleNumber == 0 && stateNumber == 0)
            {
                variableManager.prisonBreak = true;
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
    }

    void MoveToPrison()
    {
        transform.position = prisonTransform.transform.position;
    }

    void OnTriggerEnter(Collider collider)
    {
        if(collider.gameObject.CompareTag("Gem"))
        {
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
            if (audioSource != null) audioSource.PlayOneShot(goodAction2);
            variableManager.clearMission = true;
        }
        if(collider.gameObject.CompareTag("Jump"))
        {
            if(roleNumber != 2)
            {
                if(!bigJump)
                {
                    if (audioSource != null) audioSource.PlayOneShot(bigJumpAudio);
                    if(animator != null) animator.SetBool("isJumpAnimation", true);
                    bigJump = true;
                    Vector3 vel = rb.velocity;
                    vel.y = 0;
                    rb.velocity = vel;
                    rb.AddForce(Vector3.up * jumpForce * 1.7f, ForceMode.Impulse);
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
                    vel.y = 0;
                    rb.velocity = vel;
                    rb.AddForce(Vector3.up * jumpForce * 1.7f, ForceMode.Impulse);
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
    }

    private void OnNameChanged()
    {
        nameText.text = NetworkPlayerName.ToString();
    }

    public override void Spawned()
    {
        stateNumber = 0;
        isMarked = false;
        Camera cam = GetComponentInChildren<Camera>(true);
        if (!Object.HasInputAuthority)
        {
            cam.gameObject.SetActive(false);
        }
        var safe = FindFirstObjectByType<StrongBoxScript>();
        safe.AddPlayer(this.gameObject);
        var safe2 = FindFirstObjectByType<GemScript>();
        safe2.AddPlayer(this.gameObject);
        if (Object.HasInputAuthority)
        {
            NetworkPlayerName = PlayerData.PlayerName;
        }
    }

    public override void Render()
    {
        // 毎フレーム、ネットワーク変数の値をテキストに反映する
        nameText.text = NetworkPlayerName.ToString();
    }
}