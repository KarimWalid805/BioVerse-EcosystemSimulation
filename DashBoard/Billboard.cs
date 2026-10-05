using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera activeCamera;

    void LateUpdate()
    {
    
        if (activeCamera == null || !activeCamera.gameObject.activeInHierarchy)
        {
            activeCamera = Camera.main;
        }

      
        if (activeCamera == null) return;

        
        transform.LookAt(transform.position + activeCamera.transform.rotation * Vector3.forward,
                         activeCamera.transform.rotation * Vector3.up);
    }
}