using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 使用されなくなったリソースを削除する
/// </summary>
public class MeshDestroy : MonoBehaviour 
{
    public void OnDestroy()
    {
        Resources.UnloadUnusedAssets();
    }

}
