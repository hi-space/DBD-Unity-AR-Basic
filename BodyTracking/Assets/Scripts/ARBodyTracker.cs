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
    ARHumanBodyManager mHumanBodyManager;

    GameObject bodyObject;

    Dictionary<TrackableId, BoneController> mSkeletonTracker = new Dictionary<TrackableId, BoneController>();

    void Awake()
    {
        mHumanBodyManager = (ARHumanBodyManager) GetComponent<ARHumanBodyManager>();
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
        //foreach (var humanBody in eventArgs.added)
        //{
        //    Debug.Log("Created new body");
        //    bodyObject = Instantiate(mSkeletonPrefab, humanBody.transform);
        //}

        //foreach (var humanBody in eventArgs.updated)
        //{
        //    bodyObject.transform.position = humanBody.transform.position;
        //    bodyObject.transform.rotation = humanBody.transform.rotation;
        //    bodyObject.transform.localScale = humanBody.transform.localScale;
        //    var joints = humanBody.joints;

        //    for (int i = 0; i < joints.Length; i++)
        //    {
        //        Debug.Log("=====>" + joints[i].index + " / " + joints[i].localPose);
        //    }
        //}

        //foreach (var humanBody in eventArgs.removed)
        //{
        //    Destroy(bodyObject);
        //}

        BoneController boneController;

        foreach (var humanBody in eventArgs.added)
        {
            if (!mSkeletonTracker.TryGetValue(humanBody.trackableId, out boneController))
            {
                var newSkeleton = Instantiate(mSkeletonPrefab, humanBody.transform);
                boneController = newSkeleton.GetComponent<BoneController>();
                mSkeletonTracker.Add(humanBody.trackableId, boneController);
            }

            boneController.InitializeSkeletonJoints();
            boneController.ApplyBodyPose(humanBody);
        }

        foreach (var humanBody in eventArgs.updated)
        {
            if (mSkeletonTracker.TryGetValue(humanBody.trackableId, out boneController))
            {
                boneController.ApplyBodyPose(humanBody);
            }
        }

        foreach (var humanBody in eventArgs.removed)
        {
            Debug.Log("Removing a skeleton [{humanBody.trackableId}].");
            if (mSkeletonTracker.TryGetValue(humanBody.trackableId, out boneController))
            {
                Destroy(boneController.gameObject);
                mSkeletonTracker.Remove(humanBody.trackableId);
            }
        }
    }
}
