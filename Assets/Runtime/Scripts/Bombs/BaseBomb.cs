using Collectible;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security;
using UnityEngine;
using UnityEngine.Rendering.Universal;

using WorldState = System.Collections.Generic.KeyValuePair<string, object>;


public class BaseBomb : MonoBehaviour, IGrid
{
    #region Declerations
    Rigidbody rb;
    TrailRenderer trailRenderer;

    [Header("Explosion")]
    [SerializeField] public float explosionRadius = 1;

    [SerializeField] public float explosionTime = 2;
    [HideInInspector]public float timeRemaining;
    [SerializeField] private int damage = 3;
    [SerializeField, Range(0,1)] private float timeSpeedWhileHeld = 1;

    [SerializeField, Particle] private string explosionParticle;

    [Header("UI and Decal")]
    [SerializeField]private BombHUD hud;
    [SerializeField] private DecalProjector decalProjection;
    private Quaternion startProjectionRot;

    [HideInInspector] public string bombName;
    [HideInInspector] public bool ignited;
    [HideInInspector] public bool exploded;
    [HideInInspector] public bool held;

    [Header("Layers")]
    public LayerMask combatantMask;
    public LayerMask bombMask;
    public LayerMask wallMask;
    public LayerMask itemMask;

    [HideInInspector]public CombatantActions bombHeldBy;
    CombatantActions thrower;
    [HideInInspector] public CombatantActions targetdBy;

    public delegate void ExpolsionCallback();
    public ExpolsionCallback explosionCallback;

    public delegate void OnPickUpCallback();
    public OnPickUpCallback onPickUp;

    public LevelGraph.Node node { get; set; }
    public LevelGraph.Node.NodeType previousType { get; set; }

    private LevelGraph.Node[,] nodesInDanger;
    #endregion

    #region MonoBehaviour Methods
    private void Awake()
    {
        //Bomb Compona=ent setup
        rb = GetComponent<Rigidbody>();
        trailRenderer = GetComponent<TrailRenderer>();

        //UI setup
        hud = Instantiate(hud, UIManager.instance.transform);
        hud.parent = transform;

        //Decal Setup
        decalProjection.size = new Vector3(explosionRadius * 2, explosionRadius * 2, decalProjection.size.z);
        startProjectionRot = decalProjection.transform.rotation;
        nodesInDanger = new LevelGraph.Node[(int)explosionRadius, (int)explosionRadius];
    }
    public void Update()
    {
        if (GameManager.instance.gameState != GameState.Playing) { return; }

        WarnAI();
        ExplosionTimer();
        SetNewNode();
        GetAllSurroundingNodes();
    }
    private void OnEnable()
    {
        ResetBomb();
        ResetDecal();
        trailRenderer.Clear();//Resets the trail renderer
        decalProjection.gameObject.SetActive(false);//Sets the projection to false
    }
    #endregion

    #region Timer
    /// <summary>
    /// Starts the ignition timer
    /// </summary>
    public void StartTimer()
    {
        if (ignited) { return; }
        //Starts showing the decal
        decalProjection.gameObject.SetActive(true);
        decalProjection.transform.SetParent(null);

        //Turns on the hud
        hud.gameObject.SetActive(true);
        hud.fillImage.fillAmount = 1;

        //Sets the time remaing to the explosion time
        timeRemaining = explosionTime;
        ignited = true;

    }
    /// <summary>
    /// Counts down till bomb explosion
    /// </summary>
    void ExplosionTimer()
    {
        if(timeRemaining >= 0)
        {
            timeRemaining -= (held) ? Time.deltaTime * timeSpeedWhileHeld : Time.deltaTime;
            hud.fillImage.fillAmount = ((explosionTime - timeRemaining) / explosionTime);
        }
        if (timeRemaining <= 0 && ignited)
        {
            Explosion();
        }

    }
    #endregion

    #region Explosion
    public void Explosion()
    {
        exploded = true;
        //Check for colliders within the explosion radius
        Collider[] cols = Physics.OverlapSphere(transform.position, explosionRadius, wallMask | combatantMask | bombMask | itemMask);
        for (int i = 0; i < cols.Length; i++)
        {   //Makes sure it dosent get itself
            if (cols[i].gameObject == gameObject) { continue; }

            //Dont hate me for this lmao
            if (ChainReaction(cols[i])){ continue; }

            if (DamageWalls(cols[i])) { continue; }

            if (DamageCombatant(cols[i])) { continue; }

            if (DestroyItem(cols[i])) { continue; }

        }
        //Plays the particle system
        ParticleSystemHelper.instance.Play(transform, explosionParticle);

        ResetBomb();
        ResetDecal();

        //Deactivates the UI
        hud.fillImage.fillAmount = 1;
        hud.gameObject.SetActive(false);//Deactivates the timer
       
        thrower = null;

        ResetDangerNodes();

        //Must happpen last
        explosionCallback.Invoke();
    }

    #region Explosion Behaviours
    private bool ChainReaction(Collider col)
    {
        if (1 << col.gameObject.layer != bombMask) { return false; }
        //Chain reaction with other bombs
        BaseBomb bombInArea = col.gameObject.GetComponent<BaseBomb>();
        //Makes sure it dosent infintley explode
        if (bombInArea == this || bombInArea.exploded) { return true; }
        bombInArea.thrower = thrower;
        bombInArea.Explosion();
        return true;
    }
    private bool DamageWalls(Collider col)
    {
        if (1 << col.gameObject.layer != wallMask) { return false; }
        //Takes the health off the walls
        Health health = col.gameObject.GetComponent<Health>();
        health.SubtractHealth(damage);
        ExplosionEffectWall();
        return true;
    }
    private bool DamageCombatant(Collider col)
    {
        if (1 << col.gameObject.layer != combatantMask) { return false; }
        //Takes the health off the combatant and adds to the kill count
        Health health = col.gameObject.GetComponent<Health>();
        if (col.gameObject.GetComponent<CombatantActions>() != thrower)
        {
            thrower.killCount++;
        }
        health.SubtractHealth(damage);
        ExplosionEffectCombatant();
        return true;
    }
    private bool DestroyItem(Collider col)
    {
        if (1 << col.gameObject.layer != itemMask) { return false; }
        //Destroys the items
        ItemHelper item = col.gameObject.GetComponent<ItemHelper>();
        item.onPickUp.Invoke();
        ExplosionEffectItem();
        return true;
    }
    #endregion

    /// <summary>
    /// Warns any ai in the area to run
    /// </summary>
    public void WarnAI()
    {
        if (ignited && timeRemaining <= explosionTime/3)
        {
            Collider[] cols = Physics.OverlapSphere(transform.position, explosionRadius, combatantMask);
            for (int i = 0; i < cols.Length; i++)
            {
                cols[i].SendMessage("OnFlee", SendMessageOptions.DontRequireReceiver);
                //TODO: test to see if this is faster
                /*CombatantActions combatant = cols[i].GetComponent<CombatantActions>();
                if (held || thrower == combatant) { continue; }
                if (combatant.isAI && !combatant.isDead && !combatant.isDamaged)
                {
                    if (combatant.agent.currentState == AIStates.Flee) { continue; }
                    GoapBlackboard.LogState(new WorldState($"Flee: {combatant.playerName}", true));
                    combatant.agent.stateMachine.SetState(AIStates.Flee);
                }*/
                
            }
        }
    }

    protected virtual void ExplosionEffectCombatant() { }
    protected virtual void ExplosionEffectWall() { }
    protected virtual void ExplosionEffectItem() { }

    #endregion

    #region Reset
    private void ResetBomb()
    {
        ignited = false;
        exploded = false;
        rb.Stop();
        rb.isKinematic = true;
        transform.rotation = Quaternion.identity;
        trailRenderer.Clear();
    }
    private void ResetDecal()
    {
        decalProjection.transform.SetParent(transform);
        decalProjection.transform.position = transform.position;
        decalProjection.transform.localRotation = startProjectionRot;
    }
    #endregion

    #region Behaviours
    /// <summary>
    /// Picks up the bomb and attaches it to the pickerupper
    /// </summary>
    /// <param name="startTR"></param>
    /// <param name="combatant"></param>
    public void OnPickup(Transform startTR, CombatantActions combatant)
    {
        //Clears combatant stuff if they have it
        if (held)
        {
            bombHeldBy.hasBomb = false;
            bombHeldBy.bombHeld = null;
        }
        //Sets position data
        transform.position = startTR.position;
        transform.SetParent(startTR);

        rb.isKinematic = true;//prevents the bomb from having weird behaviour while held

        //Sets combant values
        thrower = combatant;
        held = true;
        bombHeldBy = combatant;
        targetdBy = null;

        StartTimer();//Starts the explosion timer
        
        onPickUp?.Invoke();
        
    }
    /// <summary>
    /// Determines where the bomb is thrown
    /// </summary>
    /// <param name="startTR"></param>
    /// <param name="power"></param>
    public void OnThrow(Transform startTR, float power = 0f)
    {   //Sets the positional data
        transform.parent = null;
        transform.position = startTR.position;
        transform.forward = startTR.forward;
        
        held = false;//Sets the held data false
        bombHeldBy = null;

        //Adds physics behaviours
        rb.isKinematic = false;
        rb.AddForce(transform.forward * power, ForceMode.Impulse);//Adds force to the bomb in the direction in which its thrown
    }
    #endregion

    public CombatantActions GetThrower()
    {
        return thrower;
    }

    #region Grid
    public void ChangeGirdPosition()
    {
        throw new NotImplementedException();
    }

    public void SetNewNode()
    {
        node = GameManager.instance.grid.GetNodeFromWorldPosition(transform.position);
    }

    public void GetAllSurroundingNodes()
    {
        if (!ignited) { return; }
        float diameter = explosionRadius * 2;//diameter

        //Gets the center of the radius
        float centerPosX = transform.position.x;
        float centerPosY = transform.position.z;

        //Gets the top left corener oof the square to search through
        Vector2 topLeftCorner = new Vector2(centerPosX - explosionRadius, centerPosY - explosionRadius);        

        for (int i = 0; i < diameter; i += (int)GameManager.instance.grid.cellSize)
        {
            float xPos = topLeftCorner.x + (float)i;
            for (int j = 0; j < diameter; j += (int)GameManager.instance.grid.cellSize)
            {
                float zPos = topLeftCorner.y + (float)j;
                //Sets the world position to the postion of 2D Array
                Vector3 worldPos = new Vector3(xPos, 0, zPos);
                LevelGraph.Node node = GameManager.instance.grid.GetNodeFromWorldPosition(worldPos);

                int xIndex = i / 2;
                int yIndex = j / 2;

                //If the node is already in danger
                if (nodesInDanger[xIndex, yIndex] == node) 
                {
                    continue;
                }
                //If there is no node set the node at that index to null
                else if(node == null)
                {
                    nodesInDanger[xIndex, yIndex].inDanger = false;
                    nodesInDanger[xIndex, yIndex] = null;
                }
                //If the node is alredy null set the node to in danger
                else if (nodesInDanger[xIndex, yIndex] == null)
                {
                    node.inDanger = true;
                    nodesInDanger[xIndex, yIndex] = node;
                }
                else
                {
                    //Set the current node to false
                    nodesInDanger[xIndex, yIndex].inDanger = false;
                    node.inDanger = true;
                    nodesInDanger[xIndex, yIndex] = node;
                }
                
            }
        }
    }
    public void ResetDangerNodes()
    {
        foreach (LevelGraph.Node node in nodesInDanger)
        {
            if (node != null)
            {
                node.inDanger = false;
            }
        }
    }

    #endregion

    #region EDITOR ONLY
    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (GameManager.instance.debugManager.showBombEsplosionRadius)
        {
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }

    }
    #endif
    #endregion
}
