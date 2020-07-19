using System.IO;
using UnityEditor;
using UnityEngine;

public class ExportWallClock 
{
	[MenuItem("Build/Clocks Bundle")]
	static void BuildWallClock()
	{
		AssetBundleBuild[] build = new AssetBundleBuild[1];
		build[0] = new AssetBundleBuild();
		build[0].assetBundleName = "clocks";
		build[0].assetNames = AssetDatabase.GetAssetPathsFromAssetBundle("clocks");
		Directory.CreateDirectory("build/AssetBundles");
		BuildPipeline.BuildAssetBundles("build/AssetBundles", build, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
		File.Copy("build/AssetBundles/clocks", "../RedworkDE.DvTime/clocks", true);

		Debug.Log("Build Successful");
	}
}
