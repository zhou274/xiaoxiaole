﻿using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

namespace Watermelon
{
    public class Vibration
    {
        public static readonly long LIGHT_INTENSITY_DURATION = 15;
        public static readonly long MEDIUM_INTENSITY_DURATION = 15;
        public static readonly long HARD_INTENSITY_DURATION = 15;

#if !UNITY_EDITOR
#if UNITY_IOS
        [DllImport ( "__Internal" )]
        private static extern bool _HasVibrator ();

        [DllImport ( "__Internal" )]
        private static extern void _Vibrate ();

        [DllImport ( "__Internal" )]
        private static extern void _VibratePop ();

        [DllImport ( "__Internal" )]
        private static extern void _VibratePeek ();

        [DllImport ( "__Internal" )]
        private static extern void _VibrateNope ();
#endif

#if UNITY_ANDROID
        public static AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        public static AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        public static AndroidJavaObject vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
        public static AndroidJavaObject context = currentActivity.Call<AndroidJavaObject>("getApplicationContext");
#endif
#endif

        public static void Vibrate(VibrationIntensity intensity)
        {
            Vibrate(IntencityToMs(intensity));
        }

        private  static long IntencityToMs(VibrationIntensity intensity)
        {
            if(intensity == VibrationIntensity.Light)
            {
                return LIGHT_INTENSITY_DURATION;
            }
            else if (intensity == VibrationIntensity.Medium)
            {
                return MEDIUM_INTENSITY_DURATION;
            }
            else
            {
                return HARD_INTENSITY_DURATION;
            }
        }
        
        public static void Vibrate(long milliseconds)
        {
            if (!IsOnMobile()) return;

            if (!AudioController.IsVibrationEnabled())
                return;

#if !UNITY_EDITOR
#if UNITY_ANDROID
            vibrator.Call("vibrate", milliseconds);
#elif UNITY_IOS
            _Vibrate();
#endif
#endif
        }
        
        public static void Vibrate(long[] pattern, int repeat)
        {
            if (!IsOnMobile()) return;

            if (!AudioController.IsVibrationEnabled())
                return;

#if !UNITY_EDITOR
#if UNITY_ANDROID
            vibrator.Call("vibrate", pattern, repeat);
#elif UNITY_IOS
            _Vibrate();
#endif
#endif
        }

        public static bool HasVibrator()
        {
            if (!IsOnMobile()) return false;

#if !UNITY_EDITOR
#if UNITY_ANDROID
            AndroidJavaClass contextClass = new AndroidJavaClass("android.content.Context");
            string Context_VIBRATOR_SERVICE = contextClass.GetStatic<string>("VIBRATOR_SERVICE");
            AndroidJavaObject systemService = context.Call<AndroidJavaObject>("getSystemService", Context_VIBRATOR_SERVICE);
            if (systemService.Call<bool>("hasVibrator"))
                return true;
            else
                return false;
#elif UNITY_IOS
            return _HasVibrator();
#else
            return false;
#endif
#else
            return false;
#endif
        }

        public static void Cancel()
        {
            if (!IsOnMobile()) return;

#if !UNITY_EDITOR
#if UNITY_ANDROID
            vibrator.Call("cancel");
#endif
#endif
        }

        private static bool IsOnMobile()
        {
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
                return true;

            return false;
        }
    }
}