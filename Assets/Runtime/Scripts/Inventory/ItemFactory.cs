using Collectible;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemFactory : MonoBehaviour, IGrid
{
    public Transform itemSpawnLocation;
    public float timeTillStart = 3;
    [Range(2f, 20f)]public float factorySpeed = 2f;
    public float conveyourSpeed = 3;
    [ObjectPool] public string[] items;

    ObjectPoolManager objectPool => ObjectPoolManager.instance;
    GameManager gameManager => GameManager.instance;
    public LevelGraph grid => gameManager.grid;
    private ItemManager itemManager => ItemManager.instance;

    public LevelGraph.Node node { get; set; }
  
    public LevelGraph.Node.NodeType previousType { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(StartTimer());
        SetNewNode();
        conveyourSpeed = gameManager.conveyourSpeed;
    }
    void SpawnItem()
    {
        if (GameManager.instance.gameState != GameState.Playing) { return; }
        int randomIndex = Random.Range(0, items.Length);
        objectPool.GetGameobjectFromPool(items[randomIndex], (GameObject obj) => {
            obj.transform.forward = transform.forward;
            obj.transform.position = itemSpawnLocation.position;
            obj.GetComponent<Rigidbody>().velocity = transform.forward * conveyourSpeed;
            ItemHelper itemHelperForObject = obj.GetComponent<ItemHelper>();
            itemManager.AddItem(itemHelperForObject);


            itemHelperForObject.onPickUp.AddListener(() =>
            {
                objectPool.ReturnGameObject(items[randomIndex], obj);
                obj.SetActive(false);
                itemManager.RemoveItem(itemHelperForObject);
                itemHelperForObject.onPickUp.RemoveAllListeners();
            });
           
        });
    }
    IEnumerator StartTimer()
    {
        yield return new WaitUntil( () => { return GameManager.instance.gameState != GameState.Playing; });
        yield return new WaitForSeconds(timeTillStart);

        GoapBlackboard.LogState(new KeyValuePair<string, object>("ItemAvailable", true));

        StartCoroutine(FactoryTimer());
    }
    IEnumerator FactoryTimer()
    {
        yield return new WaitForSeconds(factorySpeed);
        SpawnItem();
        StartCoroutine(FactoryTimer());
    }

    public void ChangeGirdPosition()
    {
        if (node == null) { return; }
        node.SetOccupied(previousType);
    }

    public void SetNewNode()
    {
        ChangeGirdPosition();
        node = grid.GetNodeFromWorldPosition(transform.position);
        previousType = node.typeOfOccupaition;
        node.direction = transform.forward;
        node.SetOccupied(LevelGraph.Node.NodeType.Unbreakable);
    }
   

    private void OnDestroy()
    {
        ChangeGirdPosition();
    }
    private void OnDisable()
    {
        ChangeGirdPosition();
    }
}
