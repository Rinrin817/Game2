using UnityEngine;

public class BulkSortingLayer : MonoBehaviour
{
    [SerializeField] private string targetSortingLayer = "Stage";

    void Awake()
    {
        // 自分を含め、親配下の全MeshRendererを取得
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>(true);

        foreach (MeshRenderer r in renderers)
        {
            r.sortingLayerName = targetSortingLayer;
        }
    }
}