using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARBodyTracker : MonoBehaviour
{
    [SerializeField]
    GameObject mSkeletonPrefab;
    ARHumanBodyManager mHumanBodyManager;

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
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
