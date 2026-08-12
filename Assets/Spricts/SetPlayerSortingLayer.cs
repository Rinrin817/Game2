using UnityEngine;

public class SetPlayerSortingLayer : MonoBehaviour
{
    [SerializeField] private string targetSortingLayer = "Player";

    void Awake()
    {
        // SkinnedMeshRenderer (FBXアニメーションモデル用) を一括変更
        SkinnedMeshRenderer[] skinnedRenderers = GetComponentsInChildren<SkinnedMeshRenderer>(true);
        foreach (SkinnedMeshRenderer r in skinnedRenderers)
        {
            r.sortingLayerName = targetSortingLayer;
        }

        // 通常の MeshRenderer も念のため一括変更
        MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>(true);
        foreach (MeshRenderer r in meshRenderers)
        {
            r.sortingLayerName = targetSortingLayer;
        }
    }
}