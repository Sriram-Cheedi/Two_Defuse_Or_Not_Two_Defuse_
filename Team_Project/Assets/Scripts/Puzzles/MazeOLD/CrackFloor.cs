// using System;
// using System.Collections;
// using UnityEngine;
//
// public class CrackFloor : MonoBehaviour
// {
//     public bool willBreak = true;
//     private float delay;
//     public Collider floorCollider;
//     public bool hasBroken = false;
//     public int gridX;
//     public int gridY;
//     Material originalMaterial;
//     GridManager gridManager;
//     
//
//     public void Awake()
//     {
//         floorCollider = GetComponent<Collider>();
//         originalMaterial  = this.GetComponent<Renderer>().material;
//         willBreak = true;
//         delay = 0.5f;
//         gridManager = GameObject.Find("GridManager").GetComponent<GridManager>();
//     }
//     
//
//     public void OnCollisionEnter(Collision floor)
//     {
//         if (hasBroken)
//         {
//             return;
//         }
//
//         if (!willBreak)
//         {
//            return; 
//         }
//         
//         if (floor.collider.CompareTag("Player"))
//         {
//             hasBroken = true;
//             this.GetComponent<Renderer>().material = gridManager.myMat;
//         }
//         
//         Invoke("floorBreak", delay);
//     }
//
//     public void OnCollisionExit(Collision other)
//     {
//         if (other.collider.CompareTag("Player"))
//         {
//             GetComponent<Renderer>().material = originalMaterial;
//         }
//         
//     }
//
//
//     public void floorBreak()
//     {
//         if (floorCollider != null)
//         {
//             floorCollider.enabled = false;
//             GetComponent<Renderer>().material = originalMaterial;
//             // gameObject.SetActive(false);
//         }
//     }
//     public void ResetFloor()
//     {
//         hasBroken = false;
//
//         if (floorCollider == null)
//         {
//             floorCollider = GetComponent<Collider>();
//         }
//
//         if (floorCollider != null)
//         {
//             floorCollider.enabled = true;
//         }
//     }
// }
//
//
//
//
//
