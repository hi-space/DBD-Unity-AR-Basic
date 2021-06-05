
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARKit;
public class AREyeTracking : MonoBehaviour
{
    public GameObject hudPrefab;
    public GameObject laserPrefab;

    GameObject hudObject;
    GameObject laserObject;

    ARFaceManager arFaceManager;

    // Start is called before the first frame update
    void Awake()
    {
        arFaceManager = GetComponent<ARFaceManager>();

        var support = arFaceManager.descriptor.supportsEyeTracking;
        Debug.Log("support Eye Tracking: " + support.ToString());
    }

    // Update is called once per frame
    void Update()
    {
        foreach (ARFace face in arFaceManager.trackables)
        {
            if (face.leftEye && !hudObject)
            {
                hudObject = Instantiate(hudPrefab, face.leftEye);
            }
            if (face.rightEye && !laserObject)
            {
                laserObject = Instantiate(laserPrefab, face.rightEye);
            }

            laserObject.transform.position = face.leftEye.position;

            hudObject.transform.position = face.rightEye.position;
            hudObject.transform.rotation = face.rightEye.rotation;
        }

    }

}
