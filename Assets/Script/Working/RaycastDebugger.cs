using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class RaycastDebugger : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            PointerEventData ped = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(ped, results);

            if (results.Count > 0)
                Debug.Log($"제일 앞: {results[0].gameObject.name}");

            //if (results.Count == 0)
            //{
            //    Debug.Log("아무것도 안 맞음");
            //}
            //else
            //{
            //    foreach (var r in results)
            //        Debug.Log($"맞은 오브젝트: {r.gameObject.name} / 레이어: {LayerMask.LayerToName(r.gameObject.layer)} / 거리: {r.distance}");
            //}
        }
    }
}