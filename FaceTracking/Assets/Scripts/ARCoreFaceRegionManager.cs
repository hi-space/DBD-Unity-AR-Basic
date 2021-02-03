using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARCore;
using UnityEngine.XR.ARFoundation;

public class ARCoreFaceRegionManager : MonoBehaviour
{
    public GameObject nosePrefab;
    public GameObject leftfab;
    public GameObject rightPrefab;

    ARFaceManager arFaceManager;
    ARSessionOrigin sessionOrigin;

    NativeArray<ARCoreFaceRegionData> faceRegions;
    //Dictionary<TrackableId, Dictionary<ARCoreFaceRegion, GameObject>> instantiatedPrefabs;

    GameObject noseObject;
    GameObject foreheadLeft;
    GameObject foreheadRight;

    private void Awake()
    {
        arFaceManager = GetComponent<ARFaceManager>();
        sessionOrigin = GetComponent<ARSessionOrigin>();

        // instantiatedPrefabs = new Dictionary<TrackableId, Dictionary<ARCoreFaceRegion, GameObject>>();
    }

    // Update is called once per frame
    void Update()
    {
        ARCoreFaceSubsystem subsystem = (ARCoreFaceSubsystem)arFaceManager.subsystem;

        foreach (ARFace face in arFaceManager.trackables)
        {
            subsystem.GetRegionPoses(face.trackableId, Allocator.Persistent, ref faceRegions);

            foreach (ARCoreFaceRegionData faceRegion in faceRegions)
            {
                ARCoreFaceRegion regionType = faceRegion.region;

                if (regionType == ARCoreFaceRegion.NoseTip)
                {
                    if (!noseObject)
                    {
                        noseObject = Instantiate(nosePrefab, sessionOrigin.trackablesParent);
                    }

                    noseObject.transform.localPosition = faceRegion.pose.position;
                    noseObject.transform.localRotation = faceRegion.pose.rotation;
                }
                else if (regionType == ARCoreFaceRegion.ForeheadLeft)
                {
                    if (!foreheadLeft)
                    {
                        foreheadLeft = Instantiate(leftfab, sessionOrigin.trackablesParent);
                    }

                    foreheadLeft.transform.localPosition = faceRegion.pose.position;
                    foreheadLeft.transform.localRotation = faceRegion.pose.rotation;
                }
                else if (regionType == ARCoreFaceRegion.ForeheadRight)
                {
                    if (!foreheadRight)
                    {
                        foreheadRight = Instantiate(rightPrefab, sessionOrigin.trackablesParent);
                    }

                    foreheadRight.transform.localPosition = faceRegion.pose.position;
                    foreheadRight.transform.localRotation = faceRegion.pose.rotation;
                }
            }

            /*
            Dictionary<ARCoreFaceRegion, GameObject> regionGos;
            if (!instantiatedPrefabs.TryGetValue(face.trackableId, out regionGos))
            {
                regionGos = new Dictionary<ARCoreFaceRegion, GameObject>();
                instantiatedPrefabs.Add(face.trackableId, regionGos);
            }

            subsystem.GetRegionPoses(face.trackableId, Allocator.Persistent, ref faceRegions);
            foreach(ARCoreFaceRegionData faceRegion in faceRegions)
            {
                var regionType = faceRegion.region;

                GameObject go;
                if (!regionGos.TryGetValue(regionType, out go))
                {
                    go = Instantiate(regionPrefab, sessionOrigin.trackablesParent);
                    regionGos.Add(regionType, go);
                }

                go.transform.localPosition = faceRegion.pose.position;
                go.transform.localRotation = faceRegion.pose.rotation;
            }
            */
        }
    }
}
