using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class FrozenDoor : ObjectScript
{
    [SerializeField]
    private bool isFrozen = true;
    public override void OnPropertyInjected(PropertyType property)
    {

        // 주입시 리액션 
    }

    
    public void Melt()
    {
        isFrozen = false;
        Debug.Log("문녹음");
    }
}
