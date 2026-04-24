using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float Xsensitivity;
    public float Ysensitivity;
    public Transform PlayerTransform;

    float xRotation = 0f;


    public Vector3 topOffset    = new Vector3(0f, 2.5f, 0f);   // 真上に寄せた位置
    public float moveSpeed = 5f;
    public float switchAngle = 60f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        if(Input.GetMouseButton(0))
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
        }
    }
}
