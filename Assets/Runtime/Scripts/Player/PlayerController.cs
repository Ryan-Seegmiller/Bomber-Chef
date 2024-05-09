using Collectible;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

[RequireComponent(typeof(CombatantActions))]
public class PlayerController : MonoBehaviour
{
    #region Declearations

    private CombatantActions combatantActions;
    #endregion

    private void Start()
    {
        combatantActions = GetComponent<CombatantActions>();
    }


    #region InputSystem

    #region Movement
    void OnMove(InputValue value)
    {
        if (GameManager.instance.gameState != GameState.Playing) { return; }
        Vector2 input = value.Get<Vector2>();
       //Sets the up direction
        if (input.y < -.5f)
        {
            switch (GameManager.instance.cameraSwivel.camAngle)
            {
                case CameraAngles.Back:
                    MoveDown();
                    break;
                case CameraAngles.Left:
                    MoveRight();
                    break;
                case CameraAngles.Right:
                    MoveLeft();
                    break;
                default:
                    MoveUp();
                    break;
            }
        }
        //Sets the Down direction
        else if (input.y > .5f)
        {
            switch (GameManager.instance.cameraSwivel.camAngle)
            {
                case CameraAngles.Back:
                    MoveUp();
                    break;
                case CameraAngles.Left:
                    MoveLeft();
                    break;
                case CameraAngles.Right:
                    MoveRight();
                    break;
                default:
                    MoveDown();
                    break;
            }
        }
        //Sets the right direction
        else if (input.x < -.5f)
        {
            switch (GameManager.instance.cameraSwivel.camAngle)
            {
                case CameraAngles.Back:
                    MoveLeft();
                    break;
                case CameraAngles.Left:
                    MoveDown();
                    break;
                case CameraAngles.Right:
                    MoveUp();
                    break;
                default:
                    MoveRight();
                    break;
            }
        }
        //Sets the left direction
        else if (input.x > .5f)
        {
            switch (GameManager.instance.cameraSwivel.camAngle)
            {
                case CameraAngles.Back:
                    MoveRight();
                    break;
                case CameraAngles.Left:
                    MoveUp();
                    break;
                case CameraAngles.Right:
                    MoveDown();
                    break;
                default:
                    MoveLeft();
                    break;
            }
        }
        //Turns them off
        else
        {
            combatantActions.movementDirection = Vector2.zero;
        }
    }
    void MoveRight()
    {
        combatantActions.lookDirection = CombatantActions.LookDirection.right;
        combatantActions.movementDirection = new Vector2(1, 0);
    }
    void MoveLeft()
    {
        combatantActions.lookDirection = CombatantActions.LookDirection.left;
        combatantActions.movementDirection = new Vector2(-1, 0);
    }
    void MoveUp()
    {
        combatantActions.lookDirection = CombatantActions.LookDirection.up;
        combatantActions.movementDirection = new Vector2(0, 1);
    }
    void MoveDown()
    {
        combatantActions.movementDirection = new Vector2(0, -1);
        combatantActions.lookDirection = CombatantActions.LookDirection.down;
    }
    #endregion

    void OnFire(InputValue value)
    {
        if (GameManager.instance.gameState != GameState.Playing) { return; }

        if (combatantActions.isAiming)
        {
            combatantActions.Throw();
        }
        else
        {
            combatantActions.DropBomb();
        }
    }
    void OnAim(InputValue value)
    {

        if (GameManager.instance.gameState != GameState.Playing) { return; }

        combatantActions.isAiming = (value.Get<float>() == 1f);
    }
    void OnLook(InputValue value)
    {
        if (GameManager.instance.gameState != GameState.Playing) { return; }

        combatantActions.aimDirection = value.Get<Vector2>();
    }
    void OnInteract(InputValue value)
    {
        if (GameManager.instance.gameState != GameState.Playing) { return; }

        if (combatantActions.bombInArea)
        {
            combatantActions.PickUpBomb();
        }
        else
        {
            combatantActions.PickupItem();
        }
    }
    void OnThrowItem()
    {
        if (GameManager.instance.gameState != GameState.Playing) { return; }

        if (combatantActions.ovenInArea)
        {
            combatantActions.PlaceItem();
        }
        else
        {
            combatantActions.GetRidOfItem();
        }
    }
    
    void OnSwivelCamera(InputValue value)
    {
        GameManager.instance.cameraSwivel.CycleCameraAngle();
    }
    #region Dev Command
    public void OnToggleDebug(InputValue value)
    {
        DevController.instace.showConsole = !DevController.instace.showConsole;
    }

    public void OnReturn(InputValue value)
    {
        if (DevController.instace.showConsole)
        {
            DevController.instace.HandleInput();
            DevController.instace.input = "";
        }
    }
    #endregion
    #endregion
}
