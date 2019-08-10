using System.Collections;
using System.Collections.Generic;
using Helper;
using Unity.Entities;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private EntityManager manager;
    private StringHelper stringHelper;

    // Start is called before the first frame update
    void Start()
    {
        manager = World.Active.EntityManager;
        stringHelper = StringHelper.GetInstance();
    }

    // Update is called once per frame
    void Update()
    {

    }
}