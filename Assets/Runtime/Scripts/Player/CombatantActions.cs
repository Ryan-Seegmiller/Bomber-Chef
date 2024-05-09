using Collectible;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

using WorldState = System.Collections.Generic.KeyValuePair<string, object>;


[RequireComponent(typeof(Health))]
public class CombatantActions : MonoBehaviour, IGrid
{
    #region Declerations

    [Header("Name")]
    public string playerName;
    private GameManager gameManager => GameManager.instance;
    private MeshRenderer currentMesh;

    #region AI
    [Header("AI")]
    public bool isAI = false;
    [HideInInspector] public GoapAgent agent;

    #endregion


    #region Movement
    [HideInInspector]public Vector2 movementDirection;
    
    [Header("Movement")]
    [SerializeField] private CombatantSpeed speed;
    [HideInInspector]public bool isMoving;
    private bool halfwayUp;//Determines if the combanta is elevated
    private float movementTimer;
    private float startY = 0;//Start y of the player
    #endregion

    #region Rotation

    [HideInInspector]public Vector2 aimDirection;
    public enum LookDirection
    {
        up,
        down,
        left,
        right
    }
    [HideInInspector]public LookDirection lookDirection;

    Dictionary<LookDirection, Vector3> playerRotation = new Dictionary<LookDirection, Vector3>()
    {
        {LookDirection.up, new Vector3(0,0,0)},
        {LookDirection.down, new Vector3(0,180,0)},
        {LookDirection.left, new Vector3(0,-90,0)},
        {LookDirection.right, new Vector3(0,90,0)},
    };
    #endregion

    #region Aim
    [Header("Aiming and Throwing Referneces")]
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Transform releasePosition;

    [Header("Display Controls")]
    [SerializeField, Range(10, 100)] private int linePoints = 25;
    [SerializeField, Range(0.01f, 0.25f)] float timeBetweenPoints = 0.1f;
    private bool _isAiming;
    public bool isAiming
    {
        get { return _isAiming; }
        set
        {
            if (_isAiming != value)
            {
                if (!value)
                {
                    lineRenderer.enabled = false;
                    GoapBlackboard.LogState(new WorldState($"IsAiming: {playerName}", false));
                }
                _isAiming = value;
            }
        }
    }
    #endregion

    #region Bomb
    [Header("Bomb")]
    [HideInInspector] public float bombThrowStrength = 20;
    private float startStrength;
    [SerializeField] private float itemThrowStrength = 20;
    [SerializeField] public Vector2 stregnthClamp = new Vector2(2, 10);
    private int _killCount;
    public int killCount 
    {
        get { return _killCount; }
        set
        {
            hud.KillCounter(value);
            _killCount = value;
        }
    }
    
    
    [HideInInspector] public bool hasBomb;
    public Transform bombPos;
    public LayerMask bombLayer;
    private bool _bombInArea;
    public bool bombInArea
    {
        get {return _bombInArea; }
        set
        {
            if( _bombInArea != value)
            {
                if (!value)
                {
                    onItemExit.Invoke();
                }
                _bombInArea = value;
            }

        }
    }
    [HideInInspector]public Vector3? bombLandPos = null;

    [HideInInspector]public BaseBomb closestBomb;
    [HideInInspector]public BaseBomb bombHeld;

    #endregion

    #region Grid Values
    ObjectPoolManager poolManager => ObjectPoolManager.instance;

    public LevelGraph.Node node { get; set; }

    public LevelGraph.Node.NodeType previousType { get; set; }

    #endregion

    #region Items
    [Header("Items")]
    [SerializeField] public float ineractRadius = 1;
    [SerializeField] private LayerMask itemLayer;
    [SerializeField] private LayerMask ovenLayer;
    private bool _itemInArea;
    public bool itemInArea
    {
        get { return _itemInArea; }
        set
        {
            if (_itemInArea != value)
            {
                if (!value)
                {
                    if (!isAI)
                    {
                        onItemExit.Invoke();
                    }
                    else
                    {
                        GoapBlackboard.LogState(new WorldState($"ItemInArea: {playerName}", false));
                    }
                }
                _itemInArea = value;
            }
        }
    }
    private bool _ovenInArea;
    public bool ovenInArea
    {
        get {return _ovenInArea;}
        set
        {
            if(_ovenInArea != value)
            {
                if(!value && isAI)
                {
                    GoapBlackboard.LogState(new WorldState($"AtOven: {playerName}", false));
                }
                _ovenInArea = value;
            }
        }
    }
    [HideInInspector]public ItemHelper closestItem;
    Oven _closestOven;
    Oven closestOven
    {
        get { return _closestOven; }
        set 
        {
            if(_closestOven != null)
            {
                _closestOven.SetUIInActive();
            }
            if(value != null)
            {
                value.SetUIActive();
            }
            _closestOven = value; 
        }
    }
    Collider[] colidersInArea;

    [HideInInspector] public bool inventoryFull => inventoryManager.itemNames.Count >= inventoryManager.invertoryCapacity;

    public InventoryManager inventoryManager;
    public UIManager uIManager => UIManager.instance;
    public PlayerHud hud;


    #endregion

    #region Health
    [HideInInspector]public Health health;
    public RectTransform heartTr;
    public Vector3 heartsOffset;
    public float respawnDuration;
    private float respawnTime;
    private Camera mainCam;
    [HideInInspector] public bool isDamaged;
    [HideInInspector] public bool isDead = false;
   

    #endregion

    #region Events

    [HideInInspector] public UnityEvent onItemEnter;
    [HideInInspector] public UnityEvent onItemExit;
    [HideInInspector]public UnityEvent onItemPickup;

    #endregion

    #region UI
    public WorldHearts worldHearts;
    #endregion

    #endregion

    #region MonoBehaviour methods
    private void Start()
    {
        ItemSetup();
        HealthSetup();
        WorldUISetup();
        SetNewNode();

        respawnDuration = gameManager.gameRules.respawnTime;
        killCount = 0;//Resets the kill count
        currentMesh = GetComponent<MeshRenderer>();//Gets the mesh
        hud.SetNames(playerName, (!isAI)? Color.yellow : Color.white);//Sets the name on the hud

        startY = transform.position.y;//sets teh orignal Y
        startStrength = bombThrowStrength;//Sets the orignal strength
    }
    private void Update()
    {
        RespawnTimer();
        if (isDamaged) return;
        SetHeartsPosition();
    }
    private void FixedUpdate()
    {
        if (GameManager.instance.gameState != GameState.Playing) { return; }

        if (isDamaged) return;
        CheckAreaForObjects();
        Move();
        Rotate();
    }
    #endregion

    #region Setup
    private void HealthSetup()
    {
        //Health Setup
        health = GetComponent<Health>();
        health.maxHealth = GameManager.instance.gameRules.health;
        health.removeHealth += hud.RemoveHealth;
        health.removeHealth += TakeDamage;
        health.death += Death;

        //Health UI setup
        heartsOffset *= (int)(health.maxHealth / 3);
        hud.SetupLives(health.maxHealth);
        
        mainCam = Camera.main;//Cahches the camera so that the ui knows what the veiwport is
    }
    private void WorldUISetup()
    {
        WorldHearts worldHearts = Instantiate(this.worldHearts, hud.transform.parent);
        worldHearts.SetupWorldLives(health.maxHealth, hud.heart);
        worldHearts.SetName(playerName);
        health.removeHealth += worldHearts.RemoveHearts;
        health.death += worldHearts.Death;
        heartTr = worldHearts.rectTR;
        if (!isAI)
        {
            worldHearts.SetTextColor(Color.yellow);
        }
        this.worldHearts = worldHearts;

    }
    private void ItemSetup()
    {
        //On Item Enter setuo
        onItemEnter.AddListener(() =>
        {
            uIManager.SetItemInteract((bombInArea) ? closestBomb.bombName : closestItem.itemName);
        });

        //On item pickup setup
        onItemPickup.AddListener(() =>
        {
            inventoryManager.addIngredient.Invoke(closestItem);
            closestItem.onPickUp.Invoke();
        });

        inventoryManager = new InventoryManager(hud);
    }
    #endregion

    #region Item Methods
    /// <summary>
    /// Checks for all the items in area
    /// </summary>
    private void CheckAreaForObjects() 
    { 
        if(isDead || isDamaged) { return; }
        //Gets all tge interactables in a certain radius
        colidersInArea = Physics.OverlapSphere(transform.position, ineractRadius, itemLayer | ovenLayer | bombLayer);
       
        //Sorts by distance if needed
        if(colidersInArea.Length > 1)
        {
            colidersInArea = colidersInArea.SortCollidersArray(transform);
        }

        CheckForItems();
        CheckForOven();
        CheckForBombs();

        if (isAI) { return; }
        //Logic for the players use only
        //ACtivates or deactivates the text accordingly
        if((bombInArea || (itemInArea && !inventoryFull)) && !hasBomb)
        {
            onItemEnter.Invoke();
        }
        else
        {
            uIManager.DeactivateItemInteractText();
        }
    }
    /// <summary>
    /// Checks for nearby items
    /// </summary>
    public void CheckForItems()
    {
        //SHows the inventory full text to let teh player know
        if (!isAI && inventoryFull)
        {
            UIManager.instance.ShowIventoryFullText();
            itemInArea = false;
            return;
        }

        //Makes sure an item can even be in teh area
        if (hasBomb) { itemInArea = false; return; }
        
        //Gets the first item in the area
        for (int i = 0; i < colidersInArea.Length; i++)
        {
            if (itemLayer == 1 << colidersInArea[i].gameObject.layer)
            {
                itemInArea = true;
                closestItem = colidersInArea[i].GetComponent<ItemHelper>();
                    
                //Adds to the AI world state
                if (isAI) { GoapBlackboard.LogState(new WorldState($"ItemInArea: {playerName}", true)); }

                return;
            }
        }
        itemInArea = false;

    }
    /// <summary>
    /// Checks for nearvy bombs
    /// </summary>
    public void CheckForBombs()
    {
        if (hasBomb) { return; }

        for (int i = 0; i < colidersInArea.Length; i++)
        {
            if (bombLayer == 1 << colidersInArea[i].gameObject.layer)
            {
                bombInArea = true;
                closestBomb = colidersInArea[i].GetComponent<BaseBomb>();
                return;
            }
        }
        bombInArea = false;
        closestBomb = null;
    }
    /// <summary>
    /// Checks for neaby ovens
    /// </summary>
    public void CheckForOven()
    {
        //Gets the first oven in the area
        for (int i = 0; i < colidersInArea.Length; i++)
        {
            if (ovenLayer == 1 << colidersInArea[i].gameObject.layer)
            {
                //Makes sure the oven isnt the same
                Oven closestOven = colidersInArea[i].GetComponent<Oven>();
                if (closestOven.ovenLocked) { continue; }
                if (this.closestOven == closestOven) {
                    if (!this.closestOven.uiActive)
                    {
                        this.closestOven.SetUIActive();
                    }
                    return; 
                }
                ovenInArea = true;
               
                this.closestOven = closestOven;
                return;
            }
        }
        closestOven = null;
        ovenInArea = false;
    }

    /// <summary>
    /// Picks up the closest item if can
    /// </summary>
    public void PickupItem()
    {
        if (isDamaged || hasBomb) { return; }
        if (!itemInArea && !bombInArea) { return; }
        onItemPickup.Invoke();
    }
    /// <summary>
    /// Picks up the closest bomb if there is one
    /// </summary>
    public void PickUpBomb()
    {
        //Sets up the bomb values
        closestBomb.OnPickup(bombPos, this);
        closestBomb.explosionCallback += ResetBombValues;

        //Sets the player bomb held values
        bombHeld = closestBomb;
        hasBomb = true;

        //Resets the closest bomb values
        closestBomb = null;
        bombInArea = false;
    }
    /// <summary>
    /// Places item inside the oven
    /// </summary>
    public void PlaceItem()
    {
        if (isDamaged) return;
        if (!(inventoryManager.itemNames.Count > 0) || !ovenInArea) { return; }
        if (closestOven.isFull) { return; }
        closestOven.combatantInUse = this;
        GameObject item = poolManager.GetGameobject(inventoryManager.itemNames.Peek());
        closestOven.AddItem(item.GetComponent<ItemHelper>());
        inventoryManager.subtractIngredient.Invoke(); //Removes item from the list
    }
    /// <summary>
    /// Throws the item on the top of the stack if unwanted
    /// </summary>
    public void GetRidOfItem()
    {
        if(isDamaged) return;
        if((inventoryManager.itemNames.Count <= 0)){ return; }
        
        string itemName = inventoryManager.itemNames.Peek();//Gets the items name
        inventoryManager.subtractIngredient.Invoke(); //Removes item from the list

        //Gets the item form the object pool and trows it
        poolManager.GetGameobjectFromPool(itemName, (GameObject obj) => {
            ItemHelper itemHelperForObject = obj.GetComponent<ItemHelper>();
            itemHelperForObject.ThrowObject(releasePosition, itemThrowStrength);

            //Adds an on pick up event
            itemHelperForObject.onPickUp.AddListener(() =>
            {
                poolManager.ReturnGameObject(itemName, obj);
                obj.SetActive(false);
                itemHelperForObject.onPickUp.RemoveAllListeners();
            });
        });
    }
   
    /// <summary>
    /// Drops all the items on damaged or dead
    /// </summary>
    private void DropItems()
    {   //If there are no items dont bother
        if(inventoryManager.itemNames.Count <= 0) { return; }

        for (int i = 0; i <= inventoryManager.itemNames.Count; i++)
        {
            string itemName = inventoryManager.itemNames.Peek();//Gets the items name
            inventoryManager.subtractIngredient.Invoke(); //Removes item from the list
            poolManager.GetGameobjectFromPool(itemName, (GameObject obj) =>
            {
                ItemHelper itemHelperForObject = obj.GetComponent<ItemHelper>();
                //Spits the item out in a ranom direction
                itemHelperForObject.ThrowObject(releasePosition, itemThrowStrength, Random.insideUnitSphere);

                //Adds an on pick up event
                itemHelperForObject.onPickUp.AddListener(() =>
                {
                    poolManager.ReturnGameObject(itemName, obj);
                    obj.SetActive(false);
                    itemHelperForObject.onPickUp.RemoveAllListeners();
                });
            });
        }        
    }
        
    #endregion

    #region Bombs
    /// <summary>
    /// Draws a trajectory line
    /// </summary>
    public void DrawProjection()
    {
        if (!hasBomb) { return; }
        //Line renderer values
        lineRenderer.enabled = true;
        lineRenderer.positionCount = Mathf.CeilToInt(linePoints / timeBetweenPoints) + 1;
        //Sets up the start values for position and velocity
        Vector3 startPosition = releasePosition.position;
        Vector3 startVelocity = bombThrowStrength * releasePosition.forward / bombHeld.GetComponent<Rigidbody>().mass; //divide by mass if using mass

        int i = 0;
        lineRenderer.SetPosition(i, startPosition);//Sets the intial start position of the line renderer
        for (float time = 0; time < linePoints; time += timeBetweenPoints)
        {
            i++;
            Vector3 point = startPosition + time * startVelocity;
            point.y = startPosition.y + startVelocity.y * time + (Physics.gravity.y / 2f * time * time);//Physics calculation for finding the trajectory of the next point 
            //y = intialVelocity(Vo) * Time(T) + 1/2 Gravity(1/2G) * Time Squared(T^2) 

            lineRenderer.SetPosition(i, point);

            Vector3 lastPosition = lineRenderer.GetPosition(i - 1);

            //If it hits an object stop the line renderer
            if (Physics.Raycast(lastPosition, (point - lastPosition).normalized, out RaycastHit hit, (point - lastPosition).magnitude))
            {
                if (!(bombLayer == 1 << hit.collider.gameObject.layer))
                {
                    lineRenderer.SetPosition(i, hit.point);
                    bombLandPos = hit.point;
                    lineRenderer.positionCount = i + 1;
                    return;
                }
            }
        }
    }
    private void Rotate()
    {
        //Rotates the comabatnt to the correct direction they should be facing while moving
        if (!isAiming && movementDirection != Vector2.zero)
        {
            transform.eulerAngles = playerRotation[lookDirection];
            lineRenderer.enabled = false;
            bombThrowStrength = startStrength;
        }
        else if (isAiming && !isAI) 
        {   //Draw The line
            DrawProjection();
            
            //Uses the sticks dirction to determine where to throw the bomb
            //Uses the magnitude of the distance in which the stick is pressed forwrd for the strength
            bombThrowStrength = (aimDirection.magnitude >= .05f) ? stregnthClamp.y * aimDirection.magnitude : stregnthClamp.y * .2f;
            bombThrowStrength = Mathf.Clamp(bombThrowStrength, stregnthClamp.x, stregnthClamp.y);

            float angleToRotateTo = Mathf.Rad2Deg * Mathf.Atan2(-aimDirection.x, -aimDirection.y);//Converts the radian of the direction to a degree
            transform.eulerAngles = new Vector3(0, angleToRotateTo, 0);//Roatates the combatant accrodingly
        }
    }
    /// <summary>
    /// Throws the bomb
    /// </summary>
    public void Throw()
    {
        if (!hasBomb) { return; }

        bombHeld.OnThrow(releasePosition, bombThrowStrength);
        
        ResetBombValues();
    }
    /// <summary>
    /// Drops the bomb
    /// </summary>
    public void DropBomb()
    {
        if (!hasBomb) { return; }

        bombHeld.OnThrow(releasePosition);
        
        ResetBombValues();
    }
    /// <summary>
    /// Resets the bomb values
    /// </summary>
    public void ResetBombValues()
    {
        hasBomb = false;
        isAiming = false;
        bombHeld = null;
    }
    #endregion

    #region Movement
    private Vector3? _locationToMoveTo = null;
    private Vector3? locationToMoveTo
    {
        get { return _locationToMoveTo; }
        set
        {   //Makes sure it dosent perform any of the logic more than once
            if (locationToMoveTo == value) { return; }
            isMoving = value != null; //will only be moving if there is a location to move to
            _locationToMoveTo = value;
            if (isMoving) { StartMovement(); } //Sets up the values to start the movemnt
        }
    }
    private Vector3 previousPosition;
    /// <summary>
    /// Sets up the movemnt values
    /// </summary>
    private void StartMovement()
    {
        previousPosition = transform.position;
        halfwayUp = false;
        movementTimer = 0;
        transform.forward = ((Vector3)locationToMoveTo - previousPosition).normalized;
    }
    /// <summary>
    /// Moves the combatant
    /// </summary>
    private void Move()
    {
        //If the player isnt moving or is aiming return
        if(isAiming && isAI) { return; }
        if (!isMoving && movementDirection != Vector2.zero) { locationToMoveTo = GetNextNode(); }
        MoveNodes();

    }
    #region Gird Values
    public void ChangeGirdPosition(){ }
    /// <summary>
    /// Sets the node position to below the player
    /// </summary>
    public void SetNewNode()
    {
        node = gameManager.grid.GetNodeFromWorldPosition(transform.position);
    }
    /// <summary>
    /// Gets the next node basewd on the movement of the combatant
    /// </summary>
    /// <returns>Nullable vector3 </returns>
    Vector3? GetNextNode()
    {
        Vector3 actualMoveDirection = new Vector3(movementDirection.x, 0, movementDirection.y) * gameManager.grid.cellSize;

        LevelGraph.Node nextNode = gameManager.grid.GetNodeFromWorldPosition(transform.position + actualMoveDirection);
        
        if (nextNode == null || nextNode.occupied)
        {
            return null;
        }

        Vector3 newNodePos = gameManager.grid.GetWorldPositionCenter(nextNode.location);
        newNodePos.y = transform.position.y;
        return newNodePos;

    }
    #endregion

    /// <summary>
    /// Gets the direction for the ai to be going to find their next node
    /// </summary>
    /// <param name="nodes"></param>
    /// <returns></returns>
    public bool GetAIDirection(ref List<LevelGraph.Node> nodes)
    {
        //Returns true when movement has finished
        if (nodes.Count <= 0) { return true; }

        //Prevents the AI from getting stuck
        if (isAI && movementTimer > 2)
        {
            isMoving = false;
            agent.PickState();
        }

        if (isMoving) { return false; }
        
        //Gets the next node
        LevelGraph.Node nextNode = nodes[nodes.Count - 1];
        //Detects if nex node is not a neighbor of node
        if (!node.CheckIfNeighbor(nextNode) && nextNode != node)
        {
            //Ensures the ai is not telepoting to nodes through other nodes
            //Finds a path to the node that wanst a neighbor and add it to the path
            List<LevelGraph.Node> newNodes =  Pathfinding.instance.FindPath(transform.position, gameManager.grid.GetWorldPositionCenter(nextNode.location), true);
            foreach (LevelGraph.Node node in newNodes)
            {
                nodes.Add(node);
            }
            nextNode = nodes[nodes.Count - 1];
        }
        nodes.Remove(nextNode);

        Vector3 direction = (gameManager.grid.GetWorldPositionCenter(nextNode.location) - gameManager.grid.GetWorldPositionCenter(node.location)).normalized;
        movementDirection = new Vector2(direction.x, direction.z);

        return false;
    }
    
    /// <summary>
    /// Moves the Combatant to the next node
    /// </summary>
    private void MoveNodes()
    {
        if(this.locationToMoveTo == null) { return; }
        Vector3 movePosition = (Vector3)this.locationToMoveTo;
        movePosition.y = startY;
        float speedMultiplier = (isAiming) ? .5f : 1f;
        if (Vector3.Distance(transform.position, movePosition) <= 0.05f)
        {
            transform.position = movePosition;
            SetNewNode();
            this.locationToMoveTo = null;
            return;
        }
        // Elevates the player to give an illusuion of walking
        //TODO: Remove when animations added
        if (!halfwayUp)
        {
            halfwayUp = transform.position.y > startY + .2f;
        }
        movementTimer += Time.deltaTime ;

        Vector3 elevatedPos = (!halfwayUp) ? movePosition + new Vector3(0, .5f, 0) : movePosition;

        transform.position = Vector3.Lerp(previousPosition, elevatedPos, movementTimer * (speed.Speed) * speedMultiplier);//Lerps the position of the combatant towards the next location
    }

    #endregion

    #region Health
    /// <summary>
    /// Sets the hearts position on the UI front
    /// </summary>
    private void SetHeartsPosition()
    {
        heartTr.position = mainCam.WorldToScreenPoint(transform.position + heartsOffset);
    }
    private void TakeDamage(int damage)
    {
        if (isDead) { return; }
       
        DropBomb();//Drops any bomb that is currently held
        DropItems();//Drops all the items currently held
        
        currentMesh.enabled = false;//sets the mesh inactive
        worldHearts.transform.gameObject.SetActive(false);//Disables the hearts UI

        ResetAreaItems();

        //Sets the state of the AI
        if (isAI)
        {
            agent.currentState = AIStates.Damaged;
        }

        locationToMoveTo = null;//Stops any current movement
        isDamaged = true;//sets damaged to true
        respawnTime = 0;
    }
    private void ResetAreaItems()
    {
        #region Object Detection Reset
        if (ovenInArea)
        {
            closestOven.SetUIInActive();
            closestOven = null;
            ovenInArea = false;
        }
        if (bombInArea)
        {
            onItemExit.Invoke();
            bombInArea = false;
            closestBomb = null;
        }
        if (itemInArea)
        {
            onItemExit.Invoke();
            itemInArea = false;
            closestItem = null;
        }
        #endregion
    }
    private void RespawnTimer()
    {
        if (!isDamaged || gameManager.gameState != GameState.Playing) { return; }
        if(respawnTime < respawnDuration)
        {
            respawnTime += Time.deltaTime;
            hud.FillRespawnBar(respawnTime, respawnDuration);
        }
        else
        {
            Respawn();
        }

    }
    private void Respawn()
    {
        if(isDead) return;
        isDamaged = false;//Sets damaged to false

        //Respwans the player and resets the current node
        gameManager.GetRandomSpawnPoint().Respawn(transform);//Sets a random spawn location
        SetNewNode();

        //Reactivates the player visuals
        currentMesh.enabled = true;//Enables the mesh the mesh
        worldHearts.transform.gameObject.SetActive(true);//sets the heart transform inactve

        SetHeartsPosition();//Reset the hearts position immiediely

        //AI reset
        if (isAI)
        {
            agent.PickState();
        }
    }
   
    public void Death()
    {
        isDead = true;
        
        hud.Death();
        gameManager.EndCheck();//Checks if the game is over

        ResetAreaItems();

        //AI state change 
        if (isAI)
        {
            agent.stateMachine.SetState(AIStates.Dead);
        }
    }
    #endregion

    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (gameManager.debugManager.showInteractRadius)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, ineractRadius);
        }
    }
    #endif
}
