using System.Collections;
using System.Collections.Generic;
using Components;
using Helper;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //public GameObject GameObject;
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
        if (Input.GetKeyDown("space"))
        {
            //NativeArray<Entity> entities = new NativeArray<Entity>(1, Allocator.Temp);
            var entity = manager.CreateEntity();
            var word = new Words();
            word.SetString("aaa");
            manager.AddComponentData(entity, new Name{name = word});
//            foreach (var entity in entities)
//            {
//                manager.SetComponentData(entity, new Destroy());
//            }

            //entities.Dispose();
        }
    }
}