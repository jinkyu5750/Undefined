using UnityEngine;

public class Ivy : MonoBehaviour
{

    [SerializeField] private GameObject leafPrefab;
    private Vector3 leafSpawnPos;
    private bool isSpawned = false;
    public void Start()
    {

        leafSpawnPos = transform.GetChild(0).position;

    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.name.Equals("0019_basketball"))
            if (leafPrefab != null && isSpawned == false)
            {
                isSpawned = true;
                Instantiate(leafPrefab, leafSpawnPos, leafPrefab.transform.rotation);
            }
    }
}
