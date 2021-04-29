using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 20;
    public float jumpForce = 10.0f;

    Rigidbody playerRB;
    CapsuleCollider playerCollider;

    // Start is called before the first frame update
    void Start()
    {
        playerRB = GetComponent<Rigidbody>();
        playerCollider = GetComponent<CapsuleCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        float hInput = Input.GetAxis("Horizontal");
        float vInput = Input.GetAxis("Vertical");

        transform.Translate(new Vector3(hInput, 0, vInput) *moveSpeed*Time.deltaTime);

        if(Input.GetKeyDown(KeyCode.Space) && GroundCheck())
        {
            GetComponent<Rigidbody>().velocity = Vector3.up * jumpForce;
        }
    }

    private bool GroundCheck()
    {
        if(Physics.Raycast(transform.position, Vector3.down, playerCollider.bounds.extents.y + 0.1f))
        {
            return true;
        }
        return false;
    }
}
