// filepath: Assets/Editor/BuildProcessor.cs
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class BuildProcessor : IPreprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }

    public void OnPreprocessBuild(BuildReport report)
    {
        // 创建一个临时的GameObject，并为每个层添加一个子对象
        // 这会强制Unity认为所有层都在使用中，从而防止它们在打包时被剥离
        GameObject tempGo = new GameObject("TempLayerHolder");
        for (int i = 0; i < 32; i++)
        {
            if (!string.IsNullOrEmpty(LayerMask.LayerToName(i)))
            {
                GameObject child = new GameObject(LayerMask.LayerToName(i));
                child.transform.SetParent(tempGo.transform);
                child.layer = i;
            }
        }

        // 将这个临时对象放置在一个不会被打包的路径下，或者在构建后删除它
        // 这里我们选择在构建后销毁它
        EditorApplication.delayCall += () =>
        {
            if (tempGo != null)
            {
                Object.DestroyImmediate(tempGo);
            }
        };
    }
}