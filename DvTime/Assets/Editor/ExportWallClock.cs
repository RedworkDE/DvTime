using System.IO;
using UnityEditor;
using UnityEngine;

public class ExportWallClock 
{
	[MenuItem("Build/Wall Clock Bundle")]
	static void BuildWallClock()
	{
		AssetBundleBuild[] build = new AssetBundleBuild[1];
		build[0] = new AssetBundleBuild();
		build[0].assetBundleName = "wallclock";
		build[0].assetNames = new [] { "Assets/wallclock.fbx" };
		Directory.CreateDirectory("build/AssetBundles");
		BuildPipeline.BuildAssetBundles("build/AssetBundles", build, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
		File.Copy("build/AssetBundles/wallclock", "../RedworkDE.DvTime/wallclock", true);

		Debug.Log("Build Successful");
	}
}
