using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class daruma1Script : NetworkBehaviour
{
    [SerializeField] Rigidbody rb;
    GameObject playerObj;
    float force;
    int critical;
    float boundTimer;

    void Update()
    {
        boundTimer += Time.deltaTime;

        if (playerObj == null)
        {
            PlayerController[] controllers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
            foreach (var controller in controllers)
            {
                if (controller != null && controller.HasInputAuthority)
                {
                    playerObj = controller.gameObject;
                    break;
                }
            }
        }

        if (playerObj == null) return;

        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.LeftArrow) ||
            Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            Vector3 camForward = playerObj.transform.forward;
            camForward.y = 0f;

            if (camForward.sqrMagnitude < 0.01f)
            {
                camForward = playerObj.transform.up;
                camForward.y = 0f;
            }
            camForward.Normalize();

            Vector3 camRight = new Vector3(camForward.z, 0f, -camForward.x);

            Vector3 moveDirection = Vector3.zero;
            if (Input.GetKeyDown(KeyCode.RightArrow)) moveDirection = camRight;
            if (Input.GetKeyDown(KeyCode.LeftArrow)) moveDirection = -camRight;
            if (Input.GetKeyDown(KeyCode.UpArrow)) moveDirection = camForward;
            if (Input.GetKeyDown(KeyCode.DownArrow)) moveDirection = -camForward;

            // 自分が権限を持っていれば直接実行、持っていなければRPCで権限者へ依頼（二重実行を防止）
            if (HasStateAuthority)
            {
                ApplyForce(moveDirection);
            }
            else
            {
                Rpc_AddForce(moveDirection);
            }
        }

        if (rb.velocity.y >= 10f) rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y / 1.5f, rb.velocity.z);
        if (rb.velocity.y >= 30f) rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y / 2f, rb.velocity.z);
        if (rb.velocity.y >= 50f) rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y / 3f, rb.velocity.z);
    }

    private void ApplyForce(Vector3 dir)
    {
        critical = Random.Range(0, 10);
        if (critical == 0) force = Random.Range(5f, 10f);
        else force = Random.Range(1f, 3f);

        rb.AddForce(dir * force, ForceMode.Impulse);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void Rpc_AddForce(Vector3 dir)
    {
        ApplyForce(dir);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (HasStateAuthority && collision.gameObject.CompareTag("Stage") && boundTimer >= 0.3f)
        {
            boundTimer = 0;
            rb.AddForce(Vector3.up * Random.Range(10f, 20f), ForceMode.Impulse);
        }
        if(HasStateAuthority && !collision.gameObject.CompareTag("Stage"))
        {
            rb.velocity = new Vector3(rb.velocity.x, Mathf.Abs(rb.velocity.y) / -2f, rb.velocity.z);
        }
    }
}