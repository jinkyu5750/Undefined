using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class FrozenDoor : ObjectScript
{
    [SerializeField]
    private bool isFrozen = true;
    public override void OnPropertyInjected_Static(StaticPropertyType property)
    {
      
    }

    public override void OnPropertyInjected_Dynamic(DynamicPropertyType property)
    {

        base.OnPropertyInjected_Dynamic(property);

        if (property == DynamicPropertyType.Fever)
            Melt();

    }


    public void Melt()
    {
        isFrozen = false;
        Debug.Log("πÆ≥Ï¿Ω");
    }
}
