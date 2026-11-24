using System.Collections;
using UnityEngine;

public class ObjectGrabbable : MonoBehaviour
{
    private Rigidbody objectRigidBody;
    private Transform objectGrabPointTransform;
    private Collider[] myColliders;
    private Collider[] ignoredPlayerColliders;
    private Coroutine restoreRoutine;
    private Parcel parcel;

    private void Awake()
    {
        objectRigidBody = GetComponent<Rigidbody>();
        myColliders = GetComponentsInChildren<Collider>();
        parcel = GetComponent<Parcel>();
    }

    public void Grab(Transform grabPoint, Collider[] playerColliders)
    {
        // If grabbed again before collisions were restored, cancel the pending restore
        if (restoreRoutine != null)
        {
            StopCoroutine(restoreRoutine);
            restoreRoutine = null;
        }

        objectGrabPointTransform = grabPoint;
        objectRigidBody.useGravity = false;
        objectRigidBody.linearVelocity = Vector3.zero;   // use .velocity on Unity 2022 and older
        objectRigidBody.angularVelocity = Vector3.zero;
        objectRigidBody.interpolation = RigidbodyInterpolation.Interpolate;

        ignoredPlayerColliders = playerColliders;
        SetIgnorePlayerCollision(true);
        if (parcel != null) parcel.IsHeld = true;
    }

    public void Drop()
    {
        objectGrabPointTransform = null;
        objectRigidBody.useGravity = true;

        if (parcel != null) parcel.IsHeld = false;

        // Don't re-enable immediately, or the parcel may pop out of the player's capsule
        restoreRoutine = StartCoroutine(RestoreCollisionWhenClear());
    }

    private IEnumerator RestoreCollisionWhenClear()
    {
        // Wait until the parcel is no longer overlapping any player collider
        while (IsOverlappingPlayer())
            yield return new WaitForFixedUpdate();

        SetIgnorePlayerCollision(false);
        ignoredPlayerColliders = null;
        restoreRoutine = null;
    }

    private bool IsOverlappingPlayer()
    {
        if (ignoredPlayerColliders == null) return false;

        foreach (Collider mine in myColliders)
        {
            if (mine == null) continue;
            foreach (Collider other in ignoredPlayerColliders)
            {
                if (other == null) continue;
                if (mine.bounds.Intersects(other.bounds)) return true;
            }
        }
        return false;
    }

    private void SetIgnorePlayerCollision(bool ignore)
    {
        if (ignoredPlayerColliders == null) return;

        foreach (Collider mine in myColliders)
        {
            if (mine == null) continue;
            foreach (Collider other in ignoredPlayerColliders)
            {
                if (other == null) continue;
                Physics.IgnoreCollision(mine, other, ignore);
            }
        }
    }

    private void FixedUpdate()
    {
        if (objectGrabPointTransform != null)
        {
            float lerpSpeed = 10f;
            Vector3 newPosition = Vector3.Lerp(
                transform.position,
                objectGrabPointTransform.position,
                Time.fixedDeltaTime * lerpSpeed);

            objectRigidBody.MovePosition(newPosition);
        }
    }
}