using Components;
using Google.Protobuf;
using Helper;
using Protobuf;
using Unity.Entities;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //public GameObject GameObject;
    private EntityManager m_Manager;
    //private StringHelper m_StringHelper;

    // Start is called before the first frame update
    private void Start()
    {
        m_Manager = World.Active.EntityManager;
        var entity = m_Manager.CreateEntity();
        var word = new Words();
        word.SetString("aaa");
        m_Manager.AddComponentData(entity, new Name { name = word });
        //m_StringHelper = StringHelper.GetInstance();
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            //NativeArray<Entity> entities = new NativeArray<Entity>(1, Allocator.Temp);
            var entity = m_Manager.CreateEntity();
            var word = new Words();
            word.SetString("aaa");
            m_Manager.AddComponentData(entity, new Name {name = word});
//            foreach (var entity in entities)
//            {
//                manager.SetComponentData(entity, new Destroy());
//            }

            //entities.Dispose();
        }
    }
}