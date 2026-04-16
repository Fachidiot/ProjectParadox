/// <summary>메시지 포함 씬 전환을 간편하게 호출하는 헬퍼</summary>
public static class SceneChangeHelper
{
    private const string TRANSITION_ANI = "Default";

    public static void Change(string _scene, string _message = null)
    {
        var ani = SceneChangeManager.instance.GetAni(TRANSITION_ANI);
        if (ani is SceneChangeAni_Transition transition)
            transition.SetMessage(_message);

        SceneChangeManager.instance.SceneChange(_scene, TRANSITION_ANI);
    }
}
