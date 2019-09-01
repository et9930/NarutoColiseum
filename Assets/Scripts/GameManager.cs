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

        Person p = new Person();
        p.Name = "IongX";
        p.Age = 22;
        p.NameList.Add("熊");
        p.NameList.Add("棒");
        p.NameList.Add("棒");
        //将对象转换成字节数组
        byte[] databytes = p.ToByteArray();
        Debug.Log(p.ToString().Length + p.ToString());
        Debug.Log(databytes.Length + " " + databytes.ToString());
        //将字节数据的数据还原到对象中
        IMessage IMperson = new Person();
        Person p1 = new Person();
        p1 = (Person)IMperson.Descriptor.Parser.ParseFrom(databytes);

        //输出测试
        Debug.Log(p1.Name);
        Debug.Log(p1.Age);
        for (int i = 0; i < p1.NameList.Count; i++)
        {
            Debug.Log(p1.NameList[i]);
        }
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