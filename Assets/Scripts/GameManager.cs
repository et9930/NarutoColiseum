using Components;
using Google.Protobuf;
using Helper;
using Protobuf;
using Unity.Collections;
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
        m_Manager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    // Update is called once per frame
    private void Update()
    {
    }
}