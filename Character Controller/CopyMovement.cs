using UnityEngine;
public class CopyMovement : MonoBehaviour
{
    [SerializeField] public GameObject target;
    Transform target_transform;
    Transform common_ancestor;

    Quaternion previousAncestorRotation;
    Vector3 previousAncestorPosition;

    Matrix4x4 previousTargetWorldToLocal;
    Quaternion previousTargetRotation;

    void Start()
    {
        if (target != null)
            Initialize();
    }

    public void Initialize()
    {
        target_transform = target.transform;
        common_ancestor = Utils.FindCommonAncestor(transform, target_transform);
        if (common_ancestor != null)
        {
            previousAncestorPosition = common_ancestor.position;
            previousAncestorRotation = common_ancestor.rotation;
        }

        previousTargetWorldToLocal = target_transform.worldToLocalMatrix;
        previousTargetRotation = target_transform.rotation;
    }

    void LateUpdate()
    {
        //if the objects do not share ancestors, it's fairly straightforward: calculate the position and rotation of the constrained object in the local space of the previous parent pose, and 
        //move it to the same pose in the _current_ local space

        //if they are part of the same hierarchy, however, the transformations of the common ancestors get applied twice to the constrained object (once naturally, once because of this script)
        //so, we just apply the inverse of the movement of the common ancestors to correct it. Simple, right?
        if (target != null)
        {

            Vector3 pointInPreviousLocalSpace = previousTargetWorldToLocal.MultiplyPoint(transform.position);
            Quaternion rotationInPreviousLocalSpace = Quaternion.Inverse(previousTargetRotation) * transform.rotation;

            //set up the whole common-ancestor thing
            Quaternion offsetAncestorRotation = Quaternion.identity;
            Matrix4x4 ancestorMatrix = Matrix4x4.identity;
            if (common_ancestor != null)
            {
                offsetAncestorRotation = Quaternion.Inverse(previousAncestorRotation) * common_ancestor.rotation;
                ancestorMatrix = Matrix4x4.TRS(
                    common_ancestor.worldToLocalMatrix.MultiplyVector(previousAncestorPosition - common_ancestor.position),
                    Quaternion.Inverse(offsetAncestorRotation),
                    Vector3.one
                );
            }

            //the thing happens!
            Matrix4x4 theMatrix;
            if (common_ancestor != null)
                theMatrix = common_ancestor.localToWorldMatrix * ancestorMatrix * common_ancestor.worldToLocalMatrix * target_transform.localToWorldMatrix;
            else
                theMatrix = target_transform.localToWorldMatrix;

            transform.SetPositionAndRotation(theMatrix.MultiplyPoint(pointInPreviousLocalSpace),
                Quaternion.Inverse(offsetAncestorRotation) * target_transform.rotation * rotationInPreviousLocalSpace);


            //store the current local space information for next time
            previousTargetWorldToLocal = target_transform.worldToLocalMatrix;
            previousTargetRotation = target_transform.rotation;

            if (common_ancestor != null)
            {
                previousAncestorRotation = common_ancestor.rotation;
                previousAncestorPosition = common_ancestor.position;
            }

        }
    }
}
