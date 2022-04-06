using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARBodyTracker : MonoBehaviour
{
    [SerializeField]
    GameObject mSkeletonPrefab;

    [SerializeField]
    GameObject mDebugPrefab;

    [SerializeField]
    float scaleOffset = 0.4f;

    [SerializeField]
    Vector3 offset;

    ARHumanBodyManager mHumanBodyManager;

    GameObject bodyObject;
    
    Dictionary<int, Transform> debugJoints = new Dictionary<int, Transform>();

    Dictionary<TrackableId, BoneController> mSkeletonTracker = new Dictionary<TrackableId, BoneController>();

    BoneController boneController = new BoneController();

    void Awake()
    {
        mHumanBodyManager = (ARHumanBodyManager)GetComponent<ARHumanBodyManager>();
    }

    private void OnEnable()
    {
        mHumanBodyManager.humanBodiesChanged += OnHumanBodiesChanged;
    }

    private void OnDisable()
    {
        mHumanBodyManager.humanBodiesChanged -= OnHumanBodiesChanged;
    }

    void OnHumanBodiesChanged(ARHumanBodiesChangedEventArgs eventArgs)
    {
        foreach (var humanBody in eventArgs.added)
        {
            bodyObject = Instantiate(mSkeletonPrefab, humanBody.transform);
        }
        foreach (var humanBody in eventArgs.updated)
        {
            if (!bodyObject)
            {
bodyObject.transform.position = humanBody.transform.position + offset;
            bodyObject.transform.rotation = humanBody.transform.rotation;
            bodyObject.transform.localScale = humanBody.transform.localScale * scaleOffset;
            }
            
        }

        foreach (var humanBody in eventArgs.removed)
        {
            if (!bodyObject)
            {
                Destroy(bodyObject);
            }
        }
        // MappingBodyMesh(eventArgs);
        // DrawDebugJoints(eventArgs);
    }

    private void MappingBodyMesh(ARHumanBodiesChangedEventArgs eventArgs)
    {
        foreach (var humanBody in eventArgs.added)
        {
            if (!boneController) 
            {
                var newSkeleton = Instantiate(mSkeletonPrefab, humanBody.transform);
                boneController = newSkeleton.GetComponent<BoneController>();
                mSkeletonTracker.Add(humanBody.trackableId, boneController);

                boneController.transform.position += offset;
                boneController.transform.localScale *= scaleOffset;
            }

            boneController.InitializeSkeletonJoints();
            boneController.ApplyBodyPose(humanBody);
        }

        foreach (var humanBody in eventArgs.updated)
        {
            if (boneController)
            {
                boneController.ApplyBodyPose(humanBody);
            }
        }

        foreach (var humanBody in eventArgs.removed)
        {
            if (boneController)
            {
                Destroy(boneController.gameObject);
                mSkeletonTracker.Remove(humanBody.trackableId);
            }
        }

        // BoneController boneController;

        // foreach (var humanBody in eventArgs.added)
        // {
        //     if (!mSkeletonTracker.TryGetValue(humanBody.trackableId, out boneController))
        //     {
        //         var newSkeleton = Instantiate(mSkeletonPrefab, humanBody.transform);
        //         boneController = newSkeleton.GetComponent<BoneController>();
        //         mSkeletonTracker.Add(humanBody.trackableId, boneController);
        //     }

        //     boneController.InitializeSkeletonJoints();
        //     boneController.ApplyBodyPose(humanBody);

        //     Debug.Log("created Trackable ID: " + humanBody.trackableId);
        // }

        // foreach (var humanBody in eventArgs.updated)
        // {
        //     if (mSkeletonTracker.TryGetValue(humanBody.trackableId, out boneController))
        //     {
        //         boneController.ApplyBodyPose(humanBody);
        //     }
        //     Debug.Log("Trackable ID: " + humanBody.trackableId);
        // }

        // foreach (var humanBody in eventArgs.removed)
        // {
        //     Debug.Log("Removing a skeleton [{humanBody.trackableId}].");
        //     if (mSkeletonTracker.TryGetValue(humanBody.trackableId, out boneController))
        //     {
        //         Destroy(boneController.gameObject);
        //         mSkeletonTracker.Remove(humanBody.trackableId);
        //     }
        // }
    }

    private void DrawDebugJoints(ARHumanBodiesChangedEventArgs eventArgs)
    {
        foreach (var humanBody in eventArgs.updated)
        {
            var joints = humanBody.joints;
            foreach (var joint in joints) {
                if (!debugJoints.ContainsKey(joint.index)) {
                    debugJoints[joint.index] = Instantiate(mDebugPrefab, humanBody.transform).transform;
                }

                debugJoints[joint.index].localPosition = joint.anchorPose.position;
                debugJoints[joint.index].localRotation = joint.anchorPose.rotation;
                debugJoints[joint.index].localScale = joint.anchorScale;

                Debug.Log("[" + joint.index + "] " + joint.anchorPose.position + " / " + joint.localPose.position);
            }
        }

        foreach (var humanBody in eventArgs.removed)
        {
            var joints = humanBody.joints;
            foreach (var joint in joints) {
                Destroy(debugJoints[joint.index]);
            }
        }
    }
}
