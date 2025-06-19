using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// a class to add a ragdoll effect to an entity
public class Ragdoll : MonoBehaviour
{
    [SerializeField]
    private Transform rootBoneTransform; 
    [SerializeField] 
    private float velocityThreshold = 0.01f;   
    private Rigidbody[] rigidbodies;
    private Animator animator;
    private Rigidbody rootBoneRigidbody;
    private bool ragdollActive;


    // Awake is called before the first frame update
    void Awake()
    {
        rigidbodies = GetComponentsInChildren<Rigidbody>();
        animator = GetComponent<Animator>();
        rootBoneRigidbody = rootBoneTransform.GetComponent<Rigidbody>();
        
        foreach(Rigidbody rigidbody in rigidbodies)
        {
            DestroyIt.TagIt tag = rigidbody.gameObject.AddComponent<DestroyIt.TagIt>();
            tag.tags.Add(DestroyIt.Tag.DontDoDamage);
        }  
    }

    public void deactivateRagdoll()
    {
        ragdollActive = false;
        foreach(Rigidbody rigidbody in rigidbodies)
        {
            rigidbody.isKinematic = true;
            if(rigidbody.gameObject.TryGetComponent<Collider>(out Collider collider))
            {
                if(!rigidbody.TryGetComponent<HitBox>(out HitBox hitBox))
                {
                    collider.enabled = false;                    
                }
                else
                {
                    collider.enabled = true;    
                }
            }
        }

        animator.enabled = true;
    }
    
    public void activateRagdoll()
    {
        ragdollActive = true;
        foreach(Rigidbody rigidbody in rigidbodies)
        {
            bool kinematic = false;
            if(rigidbody.gameObject.TryGetComponent<Collider>(out Collider collider))
            {                
                if(!rigidbody.TryGetComponent<HitBox>(out HitBox hitBox))
                {
                    collider.enabled = true;                    
                }
                else
                {
                    kinematic = true;
                    collider.enabled = false;    
                }
            }
            rigidbody.isKinematic = kinematic;
        }
        animator.enabled = false;
    }

    public void applyForce(Vector3 force)
    {
        Rigidbody rb = rootBoneTransform.GetComponent<Rigidbody>();
        rb.AddForce(force, ForceMode.VelocityChange);
    }

    private void LateUpdate() 
    {
        // move the root GameObject along with the ragdoll
        if (ragdollActive)
        {
             // Check if the root bone's velocity is close to zero
            if (rootBoneRigidbody != null && rootBoneRigidbody.velocity.magnitude < velocityThreshold)
            {
                // The root bone has stopped moving, allow 1 more update
                ragdollActive = false;
            }

            // Update position and rotation based on root bone's current state
            transform.position = rootBoneTransform.position; 
            rootBoneTransform.localPosition = Vector3.zero;
            transform.rotation = rootBoneTransform.rotation;
            rootBoneTransform.localRotation = Quaternion.identity;

            // Update skinned mesh renderer bounds and child positions
            for(int i = 0; i < transform.childCount; ++i)
            {
                Transform child = transform.GetChild(i);
                SkinnedMeshRenderer skinnedMeshRenderer = child.GetComponent<SkinnedMeshRenderer>();
                if(skinnedMeshRenderer)
                {
                    Bounds newBounds = calculateBounds(skinnedMeshRenderer);
                    skinnedMeshRenderer.localBounds = newBounds;
                }
                else
                {
                    child.localPosition = Vector3.zero;
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        SkinnedMeshRenderer skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        if (skinnedMeshRenderer != null)
        {
            Matrix4x4 originalGizmosMatrix = Gizmos.matrix;

            // Set Gizmos matrix to match the transform (position, rotation, and scale) of the SkinnedMeshRenderer
            Gizmos.matrix = skinnedMeshRenderer.transform.localToWorldMatrix;

            // Draw the bounds using the SkinnedMeshRenderer's local bounds
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(skinnedMeshRenderer.localBounds.center, skinnedMeshRenderer.localBounds.size);

            // Restore the original Gizmos matrix
            Gizmos.matrix = originalGizmosMatrix;
        }
    }

    private Bounds calculateBounds(SkinnedMeshRenderer skinnedMeshRenderer)
    {
        if (skinnedMeshRenderer.bones.Length == 0)
        {
            Debug.LogError("No bones found on the SkinnedMeshRenderer!");
            return new Bounds();
        }

        Bounds newBounds = new Bounds(skinnedMeshRenderer.bones[0].localPosition, Vector3.zero);

        // Calculate the bounds by encapsulating each bone's local position (relative to SkinnedMeshRenderer)
        foreach (Transform bone in skinnedMeshRenderer.bones)
        {
            Vector3 boneLocalPosition = skinnedMeshRenderer.transform.InverseTransformPoint(bone.position); // Convert world to local
            newBounds.Encapsulate(boneLocalPosition);
        }

        return newBounds;
    }
}
