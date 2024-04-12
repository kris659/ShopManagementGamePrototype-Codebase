using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBuildable
{
    public bool CanBuildHere(Vector3 position, Quaternion rotation);

    public void Build(int typeIndex, Vector3 position, Quaternion rotation);

    public void Destroy();
}

public interface IBuildableSO: IListable
{
    public GameObject Prefab { get; }
    public GameObject PreviewPrefab { get; }
}
