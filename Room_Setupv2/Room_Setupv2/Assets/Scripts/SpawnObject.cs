﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vectrosity;
/*
 * This script is applied to all of the different types of Sphere objects attached to the right controller.
 * It controls how and when the spheres are allowed to be placed.
 * Note that each type of sphere has its own instance of this class, so don't assume that all spheres are looking at the same instances of these variables/objects. 
 * Use static variables if all spheres need to see the same object or value. 
 */
public class SpawnObject : MonoBehaviour { 

    //--- PRIVATE VARIABLES --//

    //Reference to where the first sphere was placed in a drag movement
    private GameObject originSphere;

    //Location of the Origin Sphere
    private Vector3 origin;

    //Location of the Destination Sphere
    private Vector3 dest;

    //Indicates whether the origin sphere has been placed
    private bool originSet;

    //Indicates whether the preview sphere is colliding with an existing sphere
    private bool isColliding;

    //Will be set to the object currently colliding with the preview if any
    private GameObject currCollidingObj;

    //Is the number of points inside mainLine in InitLines.cs
    //Note that this is not the same as the number of points that have been placed. Vectrosity creates two points for all line segments, even if they are the same point
    private static int numPoints;

    //Granularity of grid
    //e.g 0.25 means the cube will have four grid lines (including edges) running down each dimension of the cube 
    private float gridGranularity;

    //Vector3 that contains the coordinates to the closest point on the grid system to the gameObject (sphere on right controller)
    private Vector3 closestPoint;

    //Indicates whether the sphere is allowed to be placed. 
    //Not 100% reliable, should always double check conditions for placing.
    private bool allowPlacing;

    //--- PUBLIC VARIABLES ---//
    [Tooltip("If true, will allow user to drag to create a line between two points. If false, will only place point at origin point (Should be false for Input and Output type points.")]
    public bool allowDrag;
    [Tooltip("GameObject of the Domain Cube")]
    public GameObject domain;

    [Tooltip("Determines whether this sphere should be restricted to the boundary of the domain or not")]
    public bool restrictToBoundary;

    [Tooltip("GameObject of Right Controller")]
    public GameObject RightController;
    [Tooltip("GameObject of Preview Sphere")]
    public GameObject preview;
    [Tooltip("GameObject of PointTypeSwitcher")]
    public GameObject PointTypeSwitcher;

    void Start()
    {
        preview.SetActive(false); //disable until we need it
        gridGranularity = (float)(1m / 20m);
        originSet = false;
        isColliding = false;
        allowPlacing = false;        
    }
    void Update()
    {
        var distToCube = Vector3.Distance(domain.GetComponent<Collider>().ClosestPoint(gameObject.transform.position), gameObject.transform.position);
        //We show the preview once the controller is close enough to the cube and we aren't colliding with an existing sphere
        if (distToCube < 0.1 && (!restrictToBoundary || restrictToBoundary && distToCube > 0))
        {
            getClosestPoint();
            preview.transform.position = closestPoint;
            preview.transform.localScale = gameObject.transform.lossyScale;
            preview.SetActive(true);
            allowPlacing = true;
        }
        else
        {
            preview.SetActive(false);
            allowPlacing = false;
            return;
        }
        isColliding = false;
        //check if our preview is colliding with a placed sphere
        foreach (Transform transform in ((PointTypeSwitcher)PointTypeSwitcher.GetComponent(typeof(PointTypeSwitcher))).allTransformList)
        {
            //print(dist);
            if (transform.position == closestPoint && distToCube < 0.1)
            {
                preview.SetActive(false);
                isColliding = true;
                currCollidingObj = transform.gameObject;
                if(allowDrag)
                {
                    Color color = ((Renderer)transform.gameObject.GetComponent<Renderer>()).material.color;
                    color.a = 1;
                    ((Renderer)transform.gameObject.GetComponent<Renderer>()).material.color = color;
                }
                else
                {
                    allowPlacing = false; //we dont want to allow placing if there is already a point there and we are not allowed to drag
                }
            }
            else
            {
                Color color = ((Renderer)transform.gameObject.GetComponent<Renderer>()).material.color;
                color.a = 0.353F;
                ((Renderer)transform.gameObject.GetComponent<Renderer>()).material.color = color;
            }
        }

        if (OVRInput.GetDown(OVRInput.Button.One) && allowPlacing) //Places the initial sphere
        {
            
            if (isColliding  && allowDrag)
            {
                originSphere = currCollidingObj;
                originSet = true;
                ((InitLines)domain.GetComponent(typeof(InitLines))).mainLine.points3.Add(originSphere.transform.position);
                ((InitLines)domain.GetComponent(typeof(InitLines))).mainLine.points3.Add(originSphere.transform.position);
                numPoints = ((InitLines)domain.GetComponent(typeof(InitLines))).mainLine.points3.Count;
            }
            else if(!allowDrag)
            {
                originSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                originSphere.transform.position = closestPoint;
                originSphere.transform.localScale = gameObject.transform.lossyScale;
                originSphere.tag = "Point";
                Renderer rend = originSphere.GetComponent<Renderer>();
                if (rend != null)
                {
                    rend.material = gameObject.GetComponent<Renderer>().material;
                }
                originSphere.transform.SetParent(domain.transform, true);
                ((PointTypeSwitcher)PointTypeSwitcher.GetComponent(typeof(PointTypeSwitcher))).allTransformList.Add(originSphere.transform);
            }
        }
        if (OVRInput.Get(OVRInput.Button.One) && originSet && allowDrag)
        {
            origin = originSphere.transform.position;
            if (!allowPlacing)
                dest = gameObject.transform.position;
            else
                dest = closestPoint;
            if (origin != Vector3.zero && dest != Vector3.zero)
            {
                ((InitLines)domain.GetComponent(typeof(InitLines))).mainLine.points3[numPoints - 2] = origin;
                ((InitLines)domain.GetComponent(typeof(InitLines))).mainLine.points3[numPoints - 1] = dest;
            }
        }

        if (OVRInput.GetUp(OVRInput.Button.One) && originSet && allowDrag)
        {
            originSet = false;
            GameObject destSphere;
            if (isColliding)
            {
                destSphere = currCollidingObj;
            }
            else
            {
                if (!allowPlacing) //if we aren't allowed to place, we shouldn't
                {
                    ((InitLines)domain.GetComponent(typeof(InitLines))).mainLine.points3.RemoveAt(--numPoints);
                    ((InitLines)domain.GetComponent(typeof(InitLines))).mainLine.points3.RemoveAt(--numPoints);

                    return;
                }
                else
                {
                    destSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    destSphere.transform.position = closestPoint;
                    destSphere.tag = "Point";
                    destSphere.transform.localScale = gameObject.transform.lossyScale;
                    Renderer rend = destSphere.GetComponent<Renderer>();
                    if (rend != null)
                    {
                        rend.material = gameObject.GetComponent<Renderer>().material;
                    }
                    destSphere.transform.SetParent(domain.transform, true);
                    ((PointTypeSwitcher)PointTypeSwitcher.GetComponent(typeof(PointTypeSwitcher))).allTransformList.Add(destSphere.transform);
                }
            }

            //add to our list of line coordinates
            domain.GetComponent<InitLines>().lineTransformList.Add(originSphere.transform);
            domain.GetComponent<InitLines>().lineTransformList.Add(destSphere.transform);
        }
    }

    void getClosestPoint()
    {
        float tileSize = domain.transform.localScale.x * gridGranularity;
        Vector3 vectorToLoc = gameObject.transform.position - domain.transform.position;
        vectorToLoc = domain.transform.InverseTransformDirection(vectorToLoc);
        Vector3 relativePos = new Vector3();
        relativePos.x = Mathf.Round(vectorToLoc.x / tileSize) * tileSize;
        relativePos.y = Mathf.Round(vectorToLoc.y / tileSize) * tileSize;
        relativePos.z = Mathf.Round(vectorToLoc.z / tileSize) * tileSize;

        relativePos = domain.transform.TransformDirection(relativePos);
        closestPoint = relativePos + domain.transform.position;
        if (Vector3.Distance(domain.GetComponent<Collider>().ClosestPoint(closestPoint), closestPoint) > 0)
        {
            closestPoint = domain.GetComponent<Collider>().ClosestPoint(closestPoint);
        }
    }
}
