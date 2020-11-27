﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    List<RoomManager> worldLevels;
    RoomManager currentLevel;

    void Start()
    {
        worldLevels = new List<RoomManager>();

        foreach (RoomManager child in gameObject.GetComponentsInChildren<RoomManager>())
        {
            worldLevels.Add(child);
        }

        worldLevels.Sort(CompareWorldLevels);

        currentLevel = worldLevels[0];
    }

    void Update()
    {

    }

    private static int CompareWorldLevels(RoomManager x, RoomManager y)
    {
        if (x == null)
        {
            if (y == null)
            {
                // If x is null and y is null, they're
                // equal.
                return 0;
            }
            else
            {
                // If x is null and y is not null, y
                // is greater.
                return -1;
            }
        }
        else
        {
            // If x is not null...
            //
            if (y == null)
            // ...and y is null, x is greater.
            {
                return 1;
            }
            else
            {
                // ...and y is not null, compare the
                // lengths of the two strings.
                //
                int retval = x.transform.name.CompareTo(y.transform.name);

                if (retval != 0)
                {
                    // If the strings are not of equal length,
                    // the longer string is greater.
                    //
                    return retval;
                }
                else
                {
                    // If the strings are of equal length,
                    // sort them with ordinary string comparison.
                    //
                    return x.transform.name.CompareTo(y.transform.name);
                }
            }
        }
    }
}
