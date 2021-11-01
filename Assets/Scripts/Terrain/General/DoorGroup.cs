using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorGroup : IDoorInterface
{
    public List<Door> doors;
    public bool isOpen;

    public DoorGroup()
    {
        doors = new List<Door>();
    }

    public bool Open()
    {
        if (isOpen) return false;

        foreach(Door door in doors) 
        {
            door.Open();
        }
        isOpen = true;
        return true;
    }

    public bool Close()
    {
        if (!isOpen) return false;

        foreach (Door door in doors) 
        {
            door.Close();
        }
        isOpen = false;
        return true;
    }

    public void Toggle()
    {
        if (isOpen)
        {
            Close();
            return;
        }

        Open();
        return;
    }
}
