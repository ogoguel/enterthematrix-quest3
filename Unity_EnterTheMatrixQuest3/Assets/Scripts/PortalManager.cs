
// Copyright (c) Olivier Goguel 2024
// Licensed under the MIT License.

using System.Collections.Generic;
using UnityEngine;

public class PortalManager : MonoBehaviour
{
  
    [SerializeField]
    protected GameObject portal;
    [SerializeField]
    protected float distanceFromWall = 0.5f;
    [SerializeField]
    protected float offsetFromAnchor = -1;

    public GameObject Portal => portal;
     public Material Material => matrixFX.Material;

    float floorHeight {get; set; }
    GameObject? bestPlane  = null;

    Vector3 initialCameraForward;
    Vector3 initialCameraPosition;
    
    MatrixFX matrixFX ;
    Vector3 portalSize = Vector3.zero;

    private void Awake()
    {
        matrixFX = FindObjectOfType<MatrixFX>();
        portalSize = portal.transform.localScale;
    }

    private void Start()
    {
        initialCameraForward  = Camera.main.transform.forward;
        initialCameraPosition  = Camera.main.transform.position; 

    }
  
    public void LaunchPortal(Dictionary<GameObject,Vector2> _planes, float _floorHeight)
    {
        floorHeight = _floorHeight;
  
        float bestDot = 1;
        float bestDistance = 1000;
        initialCameraForward  = Camera.main.transform.forward;
        initialCameraPosition  = Camera.main.transform.position; 
     
        Debug.Log($"Initial Camera {initialCameraPosition} {initialCameraPosition}");

        foreach (var kv in _planes)
        {
            GameObject plane = kv.Key;
            Vector2 dimensions = kv.Value;

            float dot = Vector3.Dot(initialCameraForward, plane.transform.forward);        
            float distance = Vector3.Dot(plane.transform.position - initialCameraPosition, plane.transform.forward);
            Vector3 point = initialCameraPosition + distance * plane.transform.forward;
        
            Vector3 localPoint = point - plane.transform.position;
            float x = Vector3.Dot(plane.transform.right,localPoint);
            float y = Vector3.Dot(plane.transform.up,localPoint);
            Debug.Log($"processing {plane.transform.position}/{plane.transform.transform.forward}, x={x} y={y} vs {dimensions}, dot = {dot}");
        
            if (Mathf.Abs(x) > dimensions.x/2 || Mathf.Abs(y) > dimensions.y/2)
            {
                continue;
            }

            if (dot > -0.7f)
            {
                continue;
            }

            if (dot < bestDot && distance < bestDistance)
            {
                bestDot = dot;
                bestDistance = distance;
                bestPlane = plane;
                Debug.Log("Found Best!");
            }
        }

        if (bestPlane == null)
        {
            Debug.LogError("Missing bestplane");
            return;
        }
    }
  
    void Update()
    {
        // always update the position - to improve
        if (bestPlane != null)
        {
            Vector3 pos = bestPlane.transform.position + bestPlane.transform.forward * distanceFromWall + bestPlane.transform.right * offsetFromAnchor;
            pos.y = floorHeight;
            portal.transform.position = pos;
            portal.transform.rotation = bestPlane.transform.rotation  * Quaternion.Euler(0,90,0);

            float worldAngle = portal.transform.rotation.eulerAngles.y;
            matrixFX.Material.SetFloat("_World_Angle", 90 - worldAngle + 90);
        }
    }
}
