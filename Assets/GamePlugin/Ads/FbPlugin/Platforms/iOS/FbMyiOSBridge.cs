#if UNITY_IOS

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace mygame.sdk
{
	public class FbMyiOSBridge
	{
		[DllImport("__Internal")] private static extern void FbInitializeNative();
		public static void Initialize()
		{
			FbInitializeNative();
		}
		//----------------------------------------------
		[DllImport("__Internal")] private static extern void FbSetCfNtFullNative(int v1, int v2, int v3, int v4, int v5, bool v6);
		public static void setCfNtFull(int v1, int v2, int v3, int v4, int v5, int v6)
		{
			FbSetCfNtFullNative(v1, v2, v3, v4, v5, v6 > 0);
		}
		[DllImport("__Internal")] private static extern void FbSetCfNtFullFbExcluseNative(int rows, int columns, string areaExcluse);
		public static void setCfNtFullFbExcluse(int rows, int columns, string areaExcluse)
		{
			FbSetCfNtFullFbExcluseNative(rows, columns, areaExcluse);
		}
		//----------------------------------------------
		[DllImport("__Internal")] private static extern void FbLoadNtFullNative(string placement, string adsId, int orient);
		public static void loadNtFull(string placement, string adsId, int orient)
		{
			FbLoadNtFullNative(placement, adsId, orient);
		}
		[DllImport("__Internal")] private static extern bool FbShowNtFullNative(string placement, int timeShowBtClose, int timeDelay, bool isAutoCloseWhenClick);
		public static bool showNtFull(string placement, int timeShowBtClose, int timeDelay, bool isAutoCloseWhenClick)
		{
			return FbShowNtFullNative(placement, timeShowBtClose, timeDelay, isAutoCloseWhenClick);
		}
		[DllImport("__Internal")] private static extern bool FbReCountCurrShowNative();
		public static bool reCountCurrShow()
		{
			return FbReCountCurrShowNative();
		}
		//----------------------------------------------
		[DllImport("__Internal")] private static extern void FbLoadNtIcFullNative(string placement, string adsId, int orient);
		public static void loadNtIcFull(string placement, string adsId, int orient)
		{
			FbLoadNtIcFullNative(placement, adsId, orient);
		}
		[DllImport("__Internal")] private static extern bool FbShowNtIcFullNative(string placement, int timeShowBtClose, int timeDelay, bool isAutoCloseWhenClick);
		public static bool showNtIcFull(string placement, int timeShowBtClose, int timeDelay, bool isAutoCloseWhenClick)
		{
			return FbShowNtIcFullNative(placement, timeShowBtClose, timeDelay, isAutoCloseWhenClick);
		}
	}
}

#endif
