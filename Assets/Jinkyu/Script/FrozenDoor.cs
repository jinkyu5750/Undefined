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

        switch(property)
        {
            case DynamicPropertyType.Fever:
                Melt();
                break;
            default:
                Debug.Log("∂ﬂ∞Ã¡ˆæ æ∆");
                break;
        }

    }


    public void Melt()
    {
        isFrozen = false;
        Debug.Log("πÆ≥Ï¿Ω");
    }
}
