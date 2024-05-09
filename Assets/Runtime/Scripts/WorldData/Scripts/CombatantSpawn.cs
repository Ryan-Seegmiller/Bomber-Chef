using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class CombatantSpawn : MonoBehaviour, IGrid
{

    #region GridPoint
    public LevelGraph.Node node { get; set; }
    public LevelGraph.Node.NodeType previousType { get; set; }

    public void ChangeGirdPosition()
    {
        //Nothing needs to happen
    }

    public void SetNewNode()
    {
        node = GameManager.instance.grid.GetNodeFromWorldPosition(transform.position);
    }
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        SetNewNode();
    }
    /// <summary>
    /// Resets the position back to where the spawn is
    /// </summary>
    /// <param name="tr"></param>
    public void Respawn(Transform tr)
    {
        tr.position = GameManager.instance.grid.GetWorldPosition(node.location);
        tr.position = new Vector3(tr.position.x, 1f, tr.position.z);
    }
    public void SpawnPlayer(GameObject obj, PlayerHud hud)
    {
        GameObject player = Instantiate(obj);
        CombatantActions combatant = player.GetComponent<CombatantActions>();
        PlayerHud playerHud = Instantiate(hud, UIManager.instance.playerHudHolder);
        combatant.hud = playerHud;
        float yValue = player.transform.position.y;
        player.transform.position = new Vector3(transform.position.x, yValue, transform.position.z);

    }
    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) { return; }
        if (GameManager.instance.debugManager.showSpawnPoints)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(transform.position, 1);
        }

    }
    #endif
}
