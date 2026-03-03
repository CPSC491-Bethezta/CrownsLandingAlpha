using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Weapon")]
public class WeaponDefinition : ScriptableObject
{
   public string weaponName;
   public AnimatorOverrideController overrideController;
}
