using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Fusion;
using Fusion.Sockets;
using System.Threading.Tasks;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] Rigidbody rb;
    [SerializeField] GameObject cameraObj;
    [SerializeField] Material[] materials;
    GameObject ManagerObj;
    GameObject[] prisonObj;
    [SerializeField] GameObject nameCanvas;
    [SerializeField] GameObject[] blenderObj;
    [SerializeField] Collider myCollider;
    [SerializeField] PhysicMaterial defaultFriction;
    [SerializeField] PhysicMaterial noFriction;
    [SerializeField] AudioClip goodAction;
    [SerializeField] AudioClip goodAction2;
    [SerializeField] AudioClip jump;
    [SerializeField] AudioClip bigJumpAudio;
    [SerializeField] AudioClip badAction;
    [SerializeField] private Text nameText;
    [Networked, OnChangedRender(nameof(OnNameChanged))] public NetworkString<_16> NetworkPlayerName { get; set; }
    public float speed;
    public float jumpForce;
    [Networked] public int roleNumber { get; set; }
    [Networked] public int stateNumber { get; set; }
    [Networked, Capacity(16)] public NetworkString<_16> PlayerName { get; set; }
    [Networked] public int syncedSeed { get; set; } = 0;
    [Networked] public int currentEmote { get; set; } = 0;
    public bool canMove;
    Transform prisonTransform;
    CameraController cameraController;
    VariableManager variableManager;
    GameObject collisionObj;
    AudioSource audioSource;
    Animator animator;
    int jumpCount;
    int jumpCountLimit;
    int cameraNumber;
    public bool isStand;
    public bool bigJump;
    bool runAnimationBool;
    bool isRoleCheck = false;
    bool jogJumpRequest = false;
    int missionSub;

    // ★他人の画面でもアニメーションを動かすために、ネットワーク変数(各画面で同期される)にします
    [Networked] float x { get; set; }
    [Networked] float z { get; set; }

    void Start()
    {
        Debug.Log($"{name} : {HasInputAuthority}");
        ManagerObj = FindFirstObjectByType<VariableManager>().gameObject;
        prisonObj = GameObject.FindGameObjectsWithTag("Prison");
        prisonTransform = GameObject.Find("Prison").transform;
        cameraNumber = -1;
        audioSource = GetComponent<AudioSource>();
        jumpForce = 12f;
        jumpCount = 0;
        jumpCountLimit = 1;
        stateNumber = 0;
        missionSub = -1;
        canMove = true;
        runAnimationBool = false;
        variableManager = ManagerObj.GetComponent<VariableManager>();
    }

    // ★確実に動くように、シンプルな入力同期処理にします
    public override void FixedUpdateNetwork()
    {
        // 自分が操作している画面のときだけ、キーボードの入力を反映してネットワーク変数に入れる
        if (HasInputAuthority)
        {
            x = Input.GetAxisRaw("Horizontal");
            z = Input.GetAxisRaw("Vertical");

            if (jogJumpRequest)
            {
                // エモートの解除処理
                if(animator != null) animator.SetBool("isHelloAnimation", false);
                if(animator != null) animator.SetBool("isYurayuraAnimation", false);
                if(animator != null) animator.SetBool("isDanceAnimation", false);
                currentEmote = 0;

                ExecuteJump();

                // ★超重要：ジャンプ処理が終わったらフラグを下ろす（押しっぱなし暴発を防ぐ）
                jogJumpRequest = false; 
            }
            
            if (Input.GetKey(KeyCode.Space))
            {
                if (roleNumber == 2)
                {
                    transform.position += Vector3.up * 0.6f;
                }
            }
        }

        float currentYVelocity = rb.velocity.y;
        if (roleNumber == 2)
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
    void ExecuteJump()
    {
        if (roleNumber != 2 && isStand && canMove)
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

                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

                jumpCount++;
                isStand = false;
            }
        }
    }

    void Update()
    {
        if(!isRoleCheck && roleNumber != -1)
        {
            isRoleCheck = true;
            if(roleNumber == 0)
            {
                gameObject.layer = 6;
                animator = Instantiate(blenderObj[0], gameObject.transform.position + new Vector3(0, 0.1f, 0), Quaternion.Euler(new Vector3(0, 180, 0)), gameObject.transform).GetComponent<Animator>();
            }
            if(roleNumber == 1)
            {
                gameObject.layer = 7;
                animator = Instantiate(blenderObj[1], gameObject.transform.position + new Vector3(0, 0.1f, 0), Quaternion.Euler(new Vector3(0, 180, 0)), gameObject.transform).GetComponent<Animator>();
            }
            if(roleNumber == 2)
            {
                gameObject.layer = 8;
            }
            if(roleNumber == 2)
            {
                speed = 18f;
                rb.useGravity = false;
                GetComponent<BoxCollider>().enabled = false;
            }
            else
            {
                speed = 7f;
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
                speed = 7.5f;
            }
            GetComponent<Renderer>().material = materials[roleNumber];
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
            if(roleNumber == 2)
            {
                transform.position += Vector3.down * 0.6f;
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

        // カメラ切り替え用
        if(roleNumber == 2)
        {
            var playerObjs = variableManager.PlayerObj;
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

        if(missionSub != -1 && variableManager.missionSubNumber == -1 && roleNumber == 2)
        {
            if(missionSub == 0)
            {
                variableManager.textString = "Push any ArrowKey to set DARUMA";
                if(Input.GetKey(KeyCode.RightArrow)) { SetMission(0, 0); }
                if(Input.GetKey(KeyCode.LeftArrow)) { SetMission(0, 1); }
                if(Input.GetKey(KeyCode.UpArrow)) { SetMission(0, 2); }
                if(Input.GetKey(KeyCode.DownArrow)) { SetMission(0, 3); }
            }
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
                transform.position = prisonTransform.transform.position;
                variableManager.prisonBreak = false;
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
                    rb.AddForce(Vector3.up * jumpForce * 1.5f, ForceMode.Impulse);
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