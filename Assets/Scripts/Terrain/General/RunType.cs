using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunType : MonoBehaviour
{
    public GameObject managerPrefab;
    public Run.Level[] levels;

    private void Start()
    {
        Run.StartRun(gameObject, levels);
        Run.current.managerPrefab = managerPrefab;
    }
}
