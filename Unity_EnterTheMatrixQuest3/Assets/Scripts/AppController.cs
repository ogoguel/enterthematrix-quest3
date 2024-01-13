// Copyright (c) Olivier Goguel 2024
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;

public class AppController : MonoBehaviour
{
    OVRSceneManager sceneManager;

    Dictionary<Component,bool> components = new Dictionary<Component, bool>();
    Dictionary<GameObject,Vector2> planes = new Dictionary<GameObject,Vector2>();
    float floorHeight = 0.2f;
    PortalManager portalManager ; 
    MatrixFX matrixFX ; 

    private void Awake()
    {
        portalManager = FindObjectOfType<PortalManager>();
        matrixFX =  FindObjectOfType<MatrixFX>();
        sceneManager = FindAnyObjectByType<OVRSceneManager>();
    }

    IEnumerator Start()
    {
        sceneManager.SceneModelLoadedSuccessfully += SceneModelLoadedSuccessfullyHandler;

        yield return  null;

#if UNITY_EDITOR
        // Simple setup to simulate the scene in the Editor

        Camera.main.transform.position = new Vector3(0,1,0);
        Camera.main.transform.localRotation = Quaternion.Euler(7,0,0);

        GameObject plane1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        plane1.transform.localScale = new Vector3(6,6,0.1f);
        plane1.transform.position = new Vector3(0,0,2);
        plane1.transform.rotation = Quaternion.Euler(0,160,0);
        AddVolume(plane1.transform,true);
        
        GameObject plane2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        plane2.transform.localScale = new Vector3(6,6,0.1f);
        plane2.transform.position = new Vector3(2,0,2);
        plane2.transform.rotation = Quaternion.Euler(0,90,0);
        AddVolume(plane2.transform,true);
        
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "floor";
        floor.transform.localScale = new Vector3(6,6,0.1f);
        floor.transform.position = new Vector3(0,0,0);
        floor.transform.rotation = Quaternion.Euler(90,0,0);

        AddVolume(floor.transform,false);

        Dictionary<GameObject, Vector2> planes = new ();
        foreach(var kv in components)
        {
            if (!kv.Value)
                continue;
            planes.Add(kv.Key.gameObject,kv.Key.transform.localScale);
        }

        portalManager.LaunchPortal(planes, 0);
#endif
    }

     private void AddVolume(Component _component, bool _isPlane)
    {
        Debug.Log($"AddComponent {_component.name}");
        components.Add(_component,_isPlane);

        MeshFilter meshFilter = GetMeshFilterFromNative(_component.gameObject);
        if (meshFilter != null)
        {
            meshFilter.GetComponent<MeshRenderer>().material = matrixFX.Material;
        }
    }

    void AddPlane(GameObject _plane, Vector2 _dimension)
    {
        planes.Add(_plane,_dimension);
    }

    private void SceneModelLoadedSuccessfullyHandler()
    {
        Debug.Log("SceneModelLoadedSuccessfullyHandler");

        OVRSceneRoom layout = FindAnyObjectByType<OVRSceneRoom>();
        AddVolume(layout.Ceiling,false);
        AddVolume(layout.Floor,false);
     
        // add each plane as both volume (to render the fx onto it) and planes (to place the portal accoridngly)
        foreach (var plane in layout.Walls)
        {
            Debug.Log($"Found {plane.Dimensions} {plane.transform.position} {plane.transform.forward}");
            bool isValidPlane = plane.Dimensions.y >2;
           AddVolume(plane,isValidPlane);
            if (isValidPlane)
                AddPlane(plane.gameObject,plane.Dimensions);
        }
        
        OVRSceneVolume[] volumes =  FindObjectsByType<OVRSceneVolume>(FindObjectsInactive.Include,FindObjectsSortMode.None);
        foreach(var volume in volumes)
        {
            AddVolume(volume,false);
        }
        portalManager.LaunchPortal(planes, layout.Floor.transform.position.y);
    }

    private MeshFilter GetMeshFilterFromNative(GameObject _source)
    {
          MeshFilter meshFilter =  _source.GetComponent<MeshFilter>();;
          if (meshFilter != null)
          {
            return meshFilter;
          }
         Transform transform = _source.transform.Find("Parent/Mesh");
         if (transform != null)
         {
            return transform.GetComponent<MeshFilter>();;
         }
         return  null;
    }
}
