using UnityEngine;
using System.Runtime.InteropServices;

public static class Haptics
{
    private static bool enabled = true;

    // iOS bridge
#if UNITY_IOS
    [DllImport("__Internal")]
    private static extern void HapticsBridge_TriggerImpactLight();
#endif

    public static void SetEnabled(bool value)
    {
        enabled = value;
    }

    public static void Vibrate()
    {
        if (!enabled) return;

#if UNITY_ANDROID
        Handheld.Vibrate();
#elif UNITY_IOS
        HapticsBridge_TriggerImpactLight();
#endif
    }
}
