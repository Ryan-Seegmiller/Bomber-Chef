using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwivel : MonoBehaviour
{
    #region Delerations
    public Vector3 cameraOffset = Vector3.zero;
    public Vector3 rotationOffset;
    public CameraAngles camAngle;

    private Dictionary<CameraAngles, Vector3> camAngles;
    private CameraAngles[] cameraAngleArray = new CameraAngles[] {CameraAngles.Front, CameraAngles.Left, CameraAngles.Back, CameraAngles.Right, CameraAngles.Top };
    private int cameraIndex = 0;
    #endregion

    private void Start()
    {
        SetupDictionary();
        camAngle = cameraAngleArray[cameraIndex];
        SetCameraAngle();
    }
    /// <summary>
    /// Sets up the dictonary with the proper positions
    /// </summary>
    void SetupDictionary()
    {
        camAngles = new Dictionary<CameraAngles, Vector3>()
        {
            {CameraAngles.Top, new Vector3(0, cameraOffset.y, 0)},
            {CameraAngles.Back, new Vector3(0, cameraOffset.y, -cameraOffset.z)},
            {CameraAngles.Left, new Vector3(cameraOffset.z, cameraOffset.y, 0)},
            {CameraAngles.Right, new Vector3(-cameraOffset.z, cameraOffset.y, 0) },
            {CameraAngles.Front, new Vector3(0, cameraOffset.y, cameraOffset.z) }
        };
    }
    /// <summary>
    /// Sets the camera angle
    /// </summary>
    void SetCameraAngle()
    {
        transform.position = Vector3.zero + camAngles[camAngle];

        if (camAngle != CameraAngles.Top)
        {
            transform.LookAt(Vector3.zero);
            transform.forward += rotationOffset;
        }
        else
        {
            transform.rotation = Quaternion.Euler(90,180 ,0);
        }
    }
    /// <summary>
    /// Cycles to the next camera angle
    /// </summary>
    public void CycleCameraAngle()
    {
        cameraIndex = (cameraIndex < cameraAngleArray.Length - 1) ? cameraIndex + 1 : 0;
        camAngle = cameraAngleArray[cameraIndex];

        SetCameraAngle();
    }
    
    private void OnValidate()
    {
        if(camAngles == null) { SetupDictionary(); }
        SetCameraAngle();
    }
}
public enum CameraAngles
{
    Top,
    Back,
    Left,
    Right,
    Front
}
