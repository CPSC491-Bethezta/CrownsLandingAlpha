using UnityEngine;

public class PlayerControllerNew : MonoBehaviour
{
    public static PlayerControllerNew  Instance{
        get{
            return s_Instance;
        }
    }

    private static PlayerControllerNew  s_Instance;

    private void Awake(){
        s_Instance = this;
    }


}
