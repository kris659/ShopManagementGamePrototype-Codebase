using UnityEngine;

public class CleaningMachine : MonoBehaviour
{
    public bool isClaimed = false;
    public float speedBoost = 2f;

    [SerializeField] private Vector3 originalPosition;
    [SerializeField] private Quaternion originalRotation;
    [SerializeField] private Transform originalParent;

    [SerializeField] private Transform followTarget;
    [SerializeField] private Vector3 positionOffset;
    [SerializeField] private Quaternion rotationOffset;

    private void Awake()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalParent = transform.parent;
        WorkersManager.instance.CleaningMachines.Add(this);
    }

    private void LateUpdate()
    {
        if (followTarget != null)
        {
            // Update position and rotation to follow target with offsets
            transform.position = followTarget.position + positionOffset;
            transform.rotation = followTarget.rotation * rotationOffset;
        }
    }

    public void Claim()
    {
        isClaimed = true;
    }

    public void Follow(Transform target)
    {
        followTarget = target;
    }

    public void Release()
    {
        isClaimed = false;
        StopFollowing();
    }

    private void StopFollowing()
    {
        followTarget = null;
        transform.position = originalPosition;
        transform.rotation = originalRotation;
    }
}