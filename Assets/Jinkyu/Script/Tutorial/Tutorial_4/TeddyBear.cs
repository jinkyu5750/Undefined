using UnityEngine;

public class TeddyBear : ObjectBase
{
    private BoxCollider col;

    private float colCenterZ_Hard = 1;
    private void Start()
    {
        col= GetComponent<BoxCollider>();
    }
    public override void OnPropertyInjected_Static(StaticPropertyType property)
    {
        base.OnPropertyInjected_Static(property);

        if (property == StaticPropertyType.Hard)
        {
            Vector3 center = col.center;
            col.center = new Vector3(center.x,center.y,colCenterZ_Hard);
        }

    }


}
