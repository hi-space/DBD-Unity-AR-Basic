using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class HandEffect : MonoBehaviour
{
    ARHumanBodyManager arHumanBodyManager;

    [SerializeField]
    GameObject vertexPrefab;

    Dictionary<int, GameObject> vertexObjects;

    private void Awake()
    {
        arHumanBodyManager = (ARHumanBodyManager) GetComponent<ARHumanBodyManager>();
        vertexObjects = new Dictionary<int, GameObject>();
    }

    private void OnEnable()
    {
        arHumanBodyManager.humanBodiesChanged += OnHumanBodiesChanged;
    }

    private void OnDisable()
    {
        arHumanBodyManager.humanBodiesChanged -= OnHumanBodiesChanged;
    }
    
    void OnHumanBodiesChanged(ARHumanBodiesChangedEventArgs eventArgs)
    {
        foreach (ARHumanBody humanBody in eventArgs.updated)
        {
            NativeArray<XRHumanBodyJoint> joints = humanBody.joints;

            foreach (XRHumanBodyJoint joint in joints)
            {
                GameObject obj;
                if (!vertexObjects.TryGetValue(joint.index, out obj))
                {
                    obj = Instantiate(vertexPrefab);
                    vertexObjects.Add(joint.index, obj);
                }

                if (joint.tracked)
                {
                    obj.transform.parent = humanBody.transform;
                    obj.transform.localPosition = joint.anchorPose.position * humanBody.estimatedHeightScaleFactor;
                    obj.transform.localRotation = joint.anchorPose.rotation;
                    obj.transform.localScale = joint.anchorScale;
                    obj.SetActive(true);
                }
                else 
                {
                    obj.SetActive(false);
                }
            }
        }
    }
}
