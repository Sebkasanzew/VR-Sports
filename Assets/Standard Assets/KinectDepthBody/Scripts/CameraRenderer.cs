using System.Linq;
using UnityEngine;
using System.Collections;

public class CameraRenderer : MonoBehaviour {

    void OnRenderObject()
    {
        if (DepthParticle.GetAllInstances() != null)
        {
            DepthParticle.GetAllInstances().ToList().ForEach(x => {
                x.Render();
                Debug.Log("found one");
                });
        }
    }
}
