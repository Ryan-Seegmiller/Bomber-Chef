using UnityEditor;


[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    GameManager m_GameManager;
    private void OnSceneGUI()
    {
        m_GameManager = (GameManager)target;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}
