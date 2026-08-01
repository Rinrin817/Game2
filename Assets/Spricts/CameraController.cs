using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Fusion;
using Fusion.Sockets;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    float Xsensitivity;
    float Ysensitivity;
    public Transform PlayerTransform;
    [SerializeField] Image Joystick;
    GameObject NetworkObj;
    public int pointerId = -1;
    float xRotation = 0f;
    StartFusion startFusion;
    public Vector3 topOffset    = new Vector3(0f, 2.5f, 0f);   // 真上に寄せた位置
    public float moveSpeed = 5f;
    public float switchAngle = 60f;
    public float pendingRotationY { get; private set; }

    void Start()
    {
        NetworkObj = GameObject.Find("NetworkRunner");
        startFusion = NetworkObj.GetComponent<StartFusion>();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        PlayerTransform = null;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pointerId = eventData.pointerId;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pointerId = -1;
    }

    void Update()
    {
        Xsensitivity = startFusion.XsensivitySetting;
        Ysensitivity = startFusion.YsensivitySetting;
        if(Joystick == null && GameObject.Find("Joystick") != null)
        {
            Joystick = GameObject.Find("Joystick").GetComponent<Image>();
        }
        if(PlayerTransform == null)
        {
            var runner = FindFirstObjectByType<NetworkRunner>();
            if (runner == null)
            {
                Debug.Log("noRunner");
                return;
            }

            if (!runner.TryGetPlayerObject(runner.LocalPlayer, out var obj))
            {
                Debug.Log("noPlayer");
                return;
            }

            PlayerTransform = obj.transform;

            return;
        }
        if(Joystick == null && GameObject.Find("Joystick") != null)
        {
            Joystick = GameObject.Find("Joystick").GetComponent<Image>();
        }

        if (EventSystem.current.IsPointerOverGameObject()) return;

        if(Input.GetMouseButton(0) && Joystick == null)
        {
            // マウス入力（既存想定）
            float mouseX = Input.GetAxis("Mouse X") * Xsensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * Ysensitivity * Time.deltaTime;

            // 回転更新（既存の xRotation を使用）
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -60f, 60f);

            PlayerTransform.Rotate(Vector3.up * mouseX);
            // 視線（上下のみカメラ）
            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            transform.localPosition = new Vector3(
                0f,
                1f + Mathf.Max(0f, Mathf.Sin(xRotation * Mathf.Deg2Rad)) * 1.5f,
                -4.5f + Mathf.Abs(Mathf.Sin(xRotation * Mathf.Deg2Rad)) * 5.5f
            );
            return;
        }
        if(Joystick == null) return;
        JoystickScript js = Joystick.GetComponent<JoystickScript>();
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            // ジョイスティックを操作している指は無視
            if (touch.fingerId == js.pointerId)
                continue;

            if (touch.phase == TouchPhase.Moved)
            {
                Vector2 delta = touch.deltaPosition;

                float mouseX = delta.x * Xsensitivity * Time.deltaTime * -0.2f;
                float mouseY = delta.y * Ysensitivity * Time.deltaTime * -0.1f;

                xRotation -= mouseY;
                xRotation = Mathf.Clamp(xRotation, -60f, 60f);

                PlayerTransform.Rotate(Vector3.up * mouseX);

                transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
                transform.localPosition = new Vector3(
                    0f,
                    1f + Mathf.Max(0f, Mathf.Sin(xRotation * Mathf.Deg2Rad)) * 1.5f,
                    -4.5f + Mathf.Abs(Mathf.Sin(xRotation * Mathf.Deg2Rad)) * 5.5f
                );
            }
            return;
        }
    }
    
    public void OnDrag(PointerEventData eventData){ }
}
