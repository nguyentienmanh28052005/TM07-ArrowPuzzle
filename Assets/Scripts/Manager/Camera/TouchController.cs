using Solo.MOST_IN_ONE;
using UnityEngine;

public class TouchController : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            //MOST_HapticFeedback.Generate(MOST_HapticFeedback.HapticTypes.LightImpact);
        }
    }
}
