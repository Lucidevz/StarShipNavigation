using System.Collections;
using UnityEngine;

public class LerpManager : MonoBehaviour
{
    public enum eases {
        easeIn,
        easeOut,
        easeInAndOut,
        EaseOutBounce,
    }
    [HideInInspector]
    public eases ease;

    // Components to be affected by the lerp
    public enum effectTypes {
        position,
        rotation,
        scale,
    }
    [HideInInspector]
    public effectTypes effectType;

    // Public function to trigger a lerp between a float
    public void LerpBetweenFloat(GameObject lerpedObject, eases easeType, effectTypes component, float start, float end, float duration) {
        StartCoroutine(LerpFloat(lerpedObject, easeType, component, start, end, duration));  
    }

    // Public function to trigger a lerp between a Vector3
    public void LerpBetweenVector3(GameObject lerpedObject, eases easeType, effectTypes component, Vector3 start, Vector3 end, float duration) {
        StartCoroutine(LerpVector3(lerpedObject, easeType, component,  start, end, duration));
    }

    IEnumerator LerpFloat(GameObject lerpedObject, eases easeType, effectTypes component, float start, float end, float duration) {
        float lerpFloat = 0;
        float time = 0;
        while (time < 1) {
            float percentage = 0f;
            // Compare which ease to use enum and call appropriate function 
            if (easeType == eases.easeIn) {
                percentage = EaseIn(time);
            } else if (easeType == eases.easeOut) {
                percentage = EaseOut(time);
            } else if (easeType == eases.easeInAndOut) {
                percentage = EaseInAndOut(time);
            } else if (easeType == eases.EaseOutBounce) {
                percentage = EaseOutBounce(time);
            }

            // Lerp the values and store it into a variable
            lerpFloat = Lerp(start, end, percentage);
            time += Time.deltaTime / duration;
            // Return the result
            yield return lerpFloat;
        }
        // Snap lerp result to the end value
        lerpFloat = end;
        yield return lerpFloat;
    }

    IEnumerator LerpVector3(GameObject lerpedObject, eases easeType, effectTypes component, Vector3 start, Vector3 end, float duration) {
        Vector3 lerpVector3 = Vector3.zero;
        float time = 0;
        while (time < 1) {
            float percentage = 0f;
            // Compare which ease to use and call appropriate function
            if (easeType == eases.easeIn) {
                percentage = EaseIn(time);
            } else if (easeType == eases.easeOut) {
                percentage = EaseOut(time);
            } else if (easeType == eases.easeInAndOut) {
                percentage = EaseInAndOut(time);
            } else if (easeType == eases.EaseOutBounce) {
                percentage = EaseOutBounce(time);
            }

            // Store the lerped value into a variable
            lerpVector3 = Lerp(start.x, end.x, start.y, end.y, start.z, end.z, percentage);
            time += Time.deltaTime / duration;

            // Apply the lerp onto the required component
            switch (component) {
                case (effectTypes.position):
                    lerpedObject.transform.position = lerpVector3;
                    break;
                case (effectTypes.rotation):
                    lerpedObject.transform.rotation = Quaternion.Euler(lerpVector3);
                    break;
                case (effectTypes.scale):
                    lerpedObject.transform.localScale = lerpVector3;
                    break;
            }
            yield return lerpVector3;
        }

        // Snap the finished result to the end value
        lerpVector3 = end;

        switch (component) {
            case (effectTypes.position):
                lerpedObject.transform.position = lerpVector3;
                break;
            case (effectTypes.rotation):
                lerpedObject.transform.rotation = Quaternion.Euler(lerpVector3);
                break;
            case (effectTypes.scale):
                lerpedObject.transform.localScale = lerpVector3;
                break;
        }
        yield return lerpVector3;
    }

    // Lerp between one float to another
    public static float Lerp(float startValue, float endValue, float t) {
        return (startValue + (endValue - startValue) * t);
    }

    // Lerp between one Vector3 to another
    public static Vector3 Lerp(float startValueX, float endValueX, float startValueY, float endValueY, float startValueZ, float endValueZ, float t) {
        return new Vector3((startValueX + (endValueX - startValueX) * t), (startValueY + (endValueY - startValueY) * t), (startValueZ + (endValueZ - startValueZ) * t));
    }

    // Ease Functions
    public static float EaseIn(float t) {
        return t * t;
    }

    public static float EaseOut(float t) {
        return 1 - (1 - t) * (1 - t);
    }

    public static float EaseInAndOut(float t) {
        if (t < 0.5) { return 4 * t * t * t; } else { return 1 - Mathf.Pow(-2 * t + 2, 3) / 2; }
    }

    public static float EaseOutBounce(float t) {
        float n1 = 7.5625f;
        float d1 = 2.75f;

        if (t < 1 / d1) {
            return n1 * t * t;
        } else if (t < 2 / d1) {
            return n1 * (t -= 1.5f / d1) * t + 0.75f;
        } else if (t < 2.5f / d1) {
            return n1 * (t -= 2.25f / d1) * t + 0.9375f;
        } else {
            return n1 * (t -= 2.625f / d1) * t + 0.984375f;
        }
    }
}
