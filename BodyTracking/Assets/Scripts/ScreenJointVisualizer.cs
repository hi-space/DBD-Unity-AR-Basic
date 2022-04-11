using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ScreenJointVisualizer : MonoBehaviour
{
    enum JointIndices
    {
        Invalid = -1,
        Head = 0, // parent: Neck1 [1]
        Neck1 = 1, // parent: Root [16]
        RightShoulder1 = 2, // parent: Neck1 [1]
        RightForearm = 3, // parent: RightShoulder1 [2]
        RightHand = 4, // parent: RightForearm [3]
        LeftShoulder1 = 5, // parent: Neck1 [1]
        LeftForearm = 6, // parent: LeftShoulder1 [5]
        LeftHand = 7, // parent: LeftForearm [6]
        RightUpLeg = 8, // parent: Root [16]
        RightLeg = 9, // parent: RightUpLeg [8]
        RightFoot = 10, // parent: RightLeg [9]
        LeftUpLeg = 11, // parent: Root [16]
        LeftLeg = 12, // parent: LeftUpLeg [11]
        LeftFoot = 13, // parent: LeftLeg [12]
        RightEye = 14, // parent: Head [0]
        LeftEye = 15, // parent: Head [0]
        Root = 16, // parent: <none> [-1]
    }

    [SerializeField]
    Camera arCamera;
    ARHumanBodyManager humanBodyManager;

    // lines
    [SerializeField]
    GameObject linePrefab;

    Dictionary<int, GameObject> lineRenderers;
    static HashSet<int> s_JointSet = new HashSet<int>();

    // vertices
    [SerializeField]
    GameObject vertexPrefab;

    Dictionary<int, GameObject> vertexObjects;

    void Awake()
    {
        humanBodyManager = (ARHumanBodyManager) GetComponent<ARHumanBodyManager>();
        lineRenderers = new Dictionary<int, GameObject>();
        vertexObjects = new Dictionary<int, GameObject>();
    }

    void Update()
    {
        NativeArray<XRHumanBodyPose2DJoint> joints = humanBodyManager.GetHumanBodyPose2DJoints(Allocator.Temp);

        // if (joints.IsCreated)
        // {
        //     UpdateVertices(joints);
        // }

        if (!joints.IsCreated)
        {
            HideJointLines();
            return;
        }
        
        UpdateVertices(joints);
        s_JointSet.Clear();
        for (int i = joints.Length - 1; i >=0; --i) 
        {
            if (joints[i].parentIndex != -1) 
            {
                UpdateRenderer(joints, i);
            }
        }
    }

    void UpdateVertices(NativeArray<XRHumanBodyPose2DJoint> joints)
    {
        for (int index = 0; index < joints.Length; index++)
        {
            XRHumanBodyPose2DJoint joint = joints[index];

            GameObject vertexObject;
            if (!vertexObjects.TryGetValue(index, out vertexObject))
            {
                vertexObject = Instantiate(vertexPrefab, transform);
                vertexObjects.Add(index, vertexObject);
            }

            if (joint.tracked)
            {
                vertexObject.transform.position = arCamera.ViewportToWorldPoint(
                    new Vector3(joint.position.x, joint.position.y, 2.0f)); // world position
            }
            else 
            {
                vertexObject.SetActive(false);
            }
        }
    }
    void UpdateRenderer(NativeArray<XRHumanBodyPose2DJoint> joints, int index)
    {
        GameObject lineObject;
        if (!lineRenderers.TryGetValue(index, out lineObject))
        {
            lineObject = Instantiate(linePrefab, transform);
            lineRenderers.Add(index, lineObject);
        }

        var lineRenderer = lineObject.GetComponent<LineRenderer>();

        // Traverse hierarchy to determine the longest line set that needs to be drawn.
        var positions = new NativeArray<Vector2>(joints.Length, Allocator.Temp);
        try
        {
            var boneIndex = index;
            int jointCount = 0;
            while (boneIndex >= 0)
            {
                var joint = joints[boneIndex];

                if (joint.tracked)
                {
                    positions[jointCount++] = joint.position;
                    if (!s_JointSet.Add(boneIndex))
                        break;
                }
                else
                    break;

                boneIndex = joint.parentIndex;
            }

            // Render the joints as lines on the camera's near clip plane.
            lineRenderer.positionCount = jointCount;
            lineRenderer.startWidth = 0.001f;
            lineRenderer.endWidth = 0.001f;
            for (int i = 0; i < jointCount; ++i)
            {
                var position = positions[i];
                var worldPosition = arCamera.ViewportToWorldPoint(
                    new Vector3(position.x, position.y, arCamera.nearClipPlane));
                lineRenderer.SetPosition(i, worldPosition);
            }
            lineObject.SetActive(true);
        }
        finally
        {
            positions.Dispose();
        }
    }

    void HideJointLines()
    {
        foreach (var lineRenderer in lineRenderers)
        {
            lineRenderer.Value.SetActive(false);
        }
    }
}
