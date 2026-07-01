using UnityEngine;

public class Ivy : ObjectBase
{

    [SerializeField] private GameObject leafPrefab;
    private Vector3 leafSpawnPos;

    public override void Start()
    {
        base.Start();
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.name.Equals("0019_basketball"))
            Instantiate(leafPrefab,leafSpawnPos,Quaternion.identity);
    }
}
