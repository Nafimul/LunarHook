using System.Collections;
using System.Globalization;
using Unity.VisualScripting;
using UnityEngine;

public class Head : MonoBehaviour
{
    Rigidbody2D rb;
    FixedJoint2D attachedObjJoint;
    GameObject wieldedObj;
    float wieldedObjHalfHeight;

    public GameObject handle;
    public GrapplingHook grappleScript;
    [HideInInspector]
    public GameObject attachedTo;
    public bool IsAttached { get; private set; }
    public bool IsWieldingObj { get; private set; }

    public float distanceFromHandleAtRest;
    public float wieldedObjDist;
    public int collisionDamage;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        attachedObjJoint = GetComponent<FixedJoint2D>();
    }

    void LateUpdate()
    {
        FollowHandle();
    }

    public void FollowHandle()
    {
        //follow the handle when not grappling
        if (!grappleScript.IsGrappling || IsWieldingObj)
        {
            transform.SetPositionAndRotation(handle.transform.position + HandleForward() * distanceFromHandleAtRest
                                            , handle.transform.rotation);

            if (IsWieldingObj)
            {
                wieldedObj.GetComponent<Rigidbody2D>().MovePositionAndRotation(
                                                        transform.position + wieldedObjDist * wieldedObjHalfHeight * HandleForward()
                                                        , handle.transform.eulerAngles.z - 90);
            }
        }
    }

    Vector3 HandleForward()
    {
        return new Vector3(Mathf.Cos(Mathf.Deg2Rad * handle.transform.eulerAngles.z)
                                        , Mathf.Sin(Mathf.Deg2Rad * handle.transform.eulerAngles.z)
                                        , 0).normalized;
    }

    public void Attach(GameObject attachTo)
    {
        if (!IsAttached)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = 0;

            attachedObjJoint.connectedBody = attachTo.GetComponent<Rigidbody2D>();
            attachedObjJoint.enabled = true;
            //so that if the attached object moves, the head moves with it
            attachedObjJoint.autoConfigureConnectedAnchor = false;

            attachedTo = attachTo;
            IsAttached = true;
        }
    }

    public void DetachAndDewield()
    {
        //to stop uncontrollable head spinning
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = 0;

        if (IsAttached)
        {
            attachedObjJoint.enabled = false;
            //so that the joint is attached where it hit, next time it's attached
            attachedObjJoint.autoConfigureConnectedAnchor = true;

            attachedTo = null;
            IsAttached = false;
        }
        else if (IsWieldingObj)
        {
            Destroy(wieldedObj.GetComponent<FixedJoint2D>());
            ShootWieldedObj();
            wieldedObj.layer = LayerMask.NameToLayer("Obstacle");

            IsWieldingObj = false;
            wieldedObj = null;
        }
    }

    public void WieldAttachedObj()
    {
        if (!IsWieldingObj)
        {
            DetachAndDewield();

            wieldedObj = attachedObjJoint.connectedBody.gameObject;
            wieldedObj.layer = LayerMask.NameToLayer("Wielded");
            wieldedObjHalfHeight = wieldedObj.GetComponent<Collider2D>().bounds.extents.y;

            IsWieldingObj = true;
        }
    }

    void ShootWieldedObj()
    {
        wieldedObj.GetComponent<Rigidbody2D>().angularVelocity = 0;
        wieldedObj.GetComponent<Rigidbody2D>().linearVelocity = (HandleForward() * grappleScript.objShootPower);
    }
}
