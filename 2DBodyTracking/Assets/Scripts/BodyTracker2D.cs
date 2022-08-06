using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class BodyTracker2D : MonoBehaviour
{
    ARHumanBodyManager humanBodyManager;

    [SerializeField]
    Camera arCamera;

    [SerializeField]
    GameObject linePrefab;

    [SerializeField]
    GameObject vertexPrefab;

    Dictionary<int, GameObject> lineObjects;
    Dictionary<int, GameObject> vertexObjects;

    static HashSet<int> jointSet = new HashSet<int>();
    
    void Awake()
    {
        humanBodyManager = (ARHumanBodyManager) GetComponent<ARHumanBodyManager>();
        lineObjects = new Dictionary<int, GameObject>();
        vertexObjects = new Dictionary<int, GameObject>();
    }

    void Update()
    {
        NativeArray<XRHumanBodyPose2DJoint> joints = humanBodyManager.GetHumanBodyPose2DJoints(Allocator.Temp);

        // 2D joint가 생성되지 않았다면, 화면에 그려진 라인 객체들을 숨김
        if (!joints.IsCreated)
        {
            HideLines();
            return;
        }

        UpdateVertices(joints);

        // 2D joint 라인 렌더링
        jointSet.Clear();
        for (int i = 0; i < joints.Length; i++)
        {
            if (joints[i].parentIndex != -1)
            {
                UpdateLines(joints, i);
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
                vertexObject = Instantiate(vertexPrefab);
                vertexObjects.Add(index, vertexObject);
            }

            if (joint.tracked)
            {
                vertexObject.transform.position = arCamera.ViewportToWorldPoint(
                    new Vector3(joint.position.x, joint.position.y, 2.0f)); // world position

                vertexObject.SetActive(true);
            }
            else 
            {
                vertexObject.SetActive(false);
            }
        }
    }

    void UpdateLines(NativeArray<XRHumanBodyPose2DJoint> joints, int index)
    {
        GameObject lineObject;
        if (!lineObjects.TryGetValue(index, out lineObject))
        {
            lineObject = Instantiate(linePrefab);
            lineObjects.Add(index, lineObject);
        }

        NativeArray<Vector2> positions = new NativeArray<Vector2>(joints.Length, Allocator.Temp);

        try
        {
            int boneIndex = index;
            int jointCount = 0;

            while (boneIndex >= 0)
            {
                XRHumanBodyPose2DJoint joint = joints[boneIndex];
                if (joint.tracked)
                {
                    positions[jointCount++] = joint.position;
                }
                else
                {
                    break;
                }

                boneIndex = joint.parentIndex;
            }

            LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();
            lineRenderer.positionCount = jointCount;
            lineRenderer.startWidth = 0.01f;
            lineRenderer.endWidth = 0.01f;

            for (int i = 0; i < jointCount; i++)
            {
                Vector2 position = positions[i];
                Vector3 worldPosition = arCamera.ViewportToWorldPoint(new Vector3(position.x, position.y, 2.0f));
                lineRenderer.SetPosition(i, worldPosition);
            }

            lineObject.SetActive(true);
        }
        finally
        {
            positions.Dispose();
        }
    }

    void HideLines()
    {
        foreach (var lineRenderer in lineObjects)
        {
            lineRenderer.Value.SetActive(false);
        }
    }
}