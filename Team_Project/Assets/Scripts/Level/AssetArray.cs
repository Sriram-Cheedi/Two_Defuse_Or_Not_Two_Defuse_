using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class AssetArray : MonoBehaviour
{
    [SerializeField] private GameObject obj;
    [SerializeField] private  Quaternion rotation;
    [SerializeField] private  bool dx;
    [SerializeField] private  bool dy;
    [SerializeField] private  bool dz;
    private float Xoffset;
    private float Yoffset;
    private float Zoffset;
    public bool direction;
    public float sign = 1;
    public int noOfAssets;
    private List<GameObject> objects = new List<GameObject>();
    [Header("manual offset")]
    public float Addedoffset;


    public void Start()
    {
        
        direction = true;
        noOfAssets = 0;

    }

    public void OnValidate()
    {
        
        if (!obj) return;
        Renderer r = obj.GetComponent<Renderer>();
        Bounds b = r.localBounds;

        Xoffset = b.size.x * obj.transform.localScale.x + Addedoffset;
        Yoffset = b.size.y * obj.transform.localScale.y + Addedoffset;
        Zoffset = b.size.z * obj.transform.localScale.z + Addedoffset;


        DuplicateObject();
    }

    public void Update()
    {
        rotation = obj.transform.rotation;
        sign = direction ? 1.0f : -1.0f;
    }

    public void DuplicateObject()
    {
        if (objects.Count > 0)
        {
            foreach (var o in objects)
            {
                if (!o) continue;

                if (Application.isPlaying)
                {
                    Destroy(o);
                }
                else
                {
#if UNITY_EDITOR
                    EditorApplication.delayCall += () =>
                    {
                        if (o) DestroyImmediate(o);
                    };
#else
        Destroy(o);
#endif
                }
            }
            objects.Clear();

        }
        for (int i = 1; i < noOfAssets; i++)
        {
            if (dx)
            {
                var clone = Instantiate(obj, obj.transform.position + obj.transform.right * Xoffset * sign * i, obj.transform.rotation);
                objects.Add(clone);
            }

            if (dy)
            {
                var clone =Instantiate(obj, obj.transform.position + obj.transform.up * Yoffset * sign * i, obj.transform.rotation);
                objects.Add(clone);
            }

            if (dz)
            {
                var clone =Instantiate(obj, obj.transform.position + obj.transform.forward * Zoffset * sign * i, obj.transform.rotation);
                objects.Add(clone);
            }
            
        }
    }
}