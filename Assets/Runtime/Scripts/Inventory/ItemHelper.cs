using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Collectible
{
    [RequireComponent(typeof(Rigidbody))]
    public class ItemHelper : MonoBehaviour, IGrid
    {
        public ItemType type = ItemType.Ingredient;
        public string itemName;
        public Sprite sprite;
        private GameManager gameManager => GameManager.instance;
        public float timeTillDissapear = 5f;
        private float startTime;

        public UnityEvent onPickUp;
        public CombatantActions targetdBy = null;

        public LevelGraph.Node node { get; set; }
        public LevelGraph.Node.NodeType previousType { get; set; }

        [HideInInspector] public Vector3 endNodeLoctation;

        private Rigidbody rb;

        #region MonoBehaviour Methods
        public void Start()
        {
            SetNewNode();
            SetSprite();
            ResetTime();
            rb = GetComponent<Rigidbody>();
            startTime = 0f;
        }
        private void Update()
        {
            if (GameManager.instance.gameState != GameState.Playing) { return; }

            SetNewNode();
            DisappearTimer();
        }
        private void OnEnable()
        {
            SetNewNode();
            ResetTime();
            startTime = 0f;
            targetdBy = null;
        }
        public void OnDestroy()
        {
            ChangeGirdPosition();
        }
        public void OnDisable()
        {
            ChangeGirdPosition();
        }
        #endregion

        #region Sprite
        private void SetSprite()
        {
            sprite = ItemManager.instance.GetItemSprite(type, itemName);
        }
        public Sprite GetSprite()
        {
            return (sprite == null) ? ItemManager.instance.GetItemSprite(type, itemName) : sprite;
        }
        #endregion

        #region Grid Methods
        public void ChangeGirdPosition()
        {
            //Not necceary for this object
        }
        public void SetNewNode()
        {
            node = gameManager.playableArea.grid.GetNodeFromWorldPosition(transform.position);
            
        }
        #endregion

        #region Disappear Timer
        public void ResetTime()
        {
            startTime = 0;
        }
        private void DisappearTimer()
        {

            if (startTime == 0)
            {
                startTime = Time.time;
            }
            if (Time.time - startTime > timeTillDissapear)
            {
                onPickUp?.Invoke();
            }
        }
        #endregion
        
        /// <summary>
        /// Throws item in a random direction
        /// </summary>
        /// <param name="releasePosition"></param>
        /// <param name="power"></param>
        /// <param name="direction"></param>
        public void ThrowObject(Transform releasePosition, float power, Vector3? direction = null)
        {
            if(direction == null)
            {
                direction = releasePosition.forward;
            }
            transform.forward = releasePosition.forward;
            transform.position = releasePosition.position + transform.forward;
            if(rb == null)
            {
                rb = GetComponent<Rigidbody>();
            }
            rb.AddForce((Vector3)direction * power, ForceMode.Impulse);
        }

    }
}
