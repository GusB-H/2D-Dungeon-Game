using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Switch : Interactable
{
    public enum SwitchType { DoorOpen, DoorClose, DoorToggle };
    public SwitchType switchType;
    public IDoorInterface door;

    public static GameObject CreateSwitch(Vector3 position, SwitchType switchType = SwitchType.DoorToggle)
    {
        GameObject prefabSwitch = Resources.Load("Switch") as GameObject;
        GameObject newSwitch = Instantiate(prefabSwitch, position, Quaternion.identity);
        return newSwitch;
    }
    public static GameObject CreateSwitch(Vector3 position, IDoorInterface door, SwitchType switchType = SwitchType.DoorToggle)
    {
        GameObject prefabSwitch = Resources.Load("Switch") as GameObject;
        GameObject newSwitch = Instantiate(prefabSwitch, position, Quaternion.identity);
        newSwitch.GetComponent<Switch>().door = door;
        return newSwitch;
    }

    public override void Interact(Creature user)
    {
        switch (switchType)
        {
            case SwitchType.DoorOpen:
                door.Open();
                break;

            case SwitchType.DoorClose:
                door.Close();
                break;

            case SwitchType.DoorToggle:
                door.Toggle();
                break;
        }
    }
}
