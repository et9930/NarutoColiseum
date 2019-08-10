using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Entities;
using UnityEngine;

namespace Helper
{
    sealed class WrappedComponentDataAttribute : PropertyAttribute {}

    [ExecuteAlways]
    [RequireComponent(typeof(GameObjectEntity))]
    public abstract class ComponentStringDataProxy<T> : MonoBehaviour, ISerializationCallbackReceiver
        where T : struct, IComponentData
    {
        private readonly StringHelper stringHelper = StringHelper.GetInstance();

        [SerializeField, WrappedComponentData] 
        protected T m_ComponentData;

        private FieldInfo[] m_ComponentFields;

        private Type m_ComponentType;
        private T m_LastComponentData;
        private List<string> m_LastStringTable;
        private List<string> m_StringIndexList;

        [SerializeField]
        private List<string> m_StringTable;

        public T Value
        {
            get { return m_ComponentData; }
            set
            {
                ValidateSerializedData(ref value);
                m_ComponentData = value;

                EntityManager entityManager;
                Entity entity;

                if (CanSynchronizeWithEntityManager(out entityManager, out entity))
                {
                    UpdateComponentData(entityManager, entity);
                }
            }
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            EntityManager entityManager;
            Entity entity;
            if (CanSynchronizeWithEntityManager(out entityManager, out entity))
            {
                UpdateSerializedData(entityManager, entity);
            }
        }


        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
        }

        protected virtual void ValidateSerializedData(ref T serializedData)
        {
        }

        internal ComponentType GetComponentType()
        {
            return ComponentType.ReadWrite<T>();
        }

        internal void UpdateComponentData(EntityManager manager, Entity entity)
        {
            if (!ComponentType.ReadWrite<T>().IsZeroSized)
                manager.SetComponentData(entity, m_ComponentData);
        }

        internal void UpdateSerializedData(EntityManager manager, Entity entity)
        {
            if (!ComponentType.ReadWrite<T>().IsZeroSized)
                m_ComponentData = manager.GetComponentData<T>(entity);
        }

        internal void ValidateSerializedData()
        {
            ValidateSerializedData(ref m_ComponentData);
        }

        protected virtual void OnEnable()
        {
            EntityManager entityManager;
            Entity entity;
            if (
                World.Active != null
                && TryGetEntityAndManager(out entityManager, out entity)
                && !entityManager.HasComponent(entity, GetComponentType()) // in case GameObjectEntity already added
            )
                entityManager.AddComponent(entity, GetComponentType());
            InitializeDictionary();
        }

        protected virtual void OnDisable()
        {
            if (!gameObject.activeInHierarchy) // GameObjectEntity will handle removal when Entity is destroyed
                return;
            EntityManager entityManager;
            Entity entity;
            if (CanSynchronizeWithEntityManager(out entityManager, out entity))
                entityManager.RemoveComponent(entity, GetComponentType());
        }

        internal bool TryGetEntityAndManager(out EntityManager entityManager, out Entity entity)
        {
            entityManager = null;
            entity = Entity.Null;
            // gameObject is not initialized yet in native when OnBeforeSerialized() is called via SmartReset()
            if (gameObject == null)
                return false;
            var gameObjectEntity = GetComponent<GameObjectEntity>();
            if (gameObjectEntity == null)
                return false;
            if (gameObjectEntity.EntityManager == null)
                return false;
            if (!gameObjectEntity.EntityManager.Exists(gameObjectEntity.Entity))
                return false;
            entityManager = gameObjectEntity.EntityManager;
            entity = gameObjectEntity.Entity;
            return true;
        }

        internal bool CanSynchronizeWithEntityManager(out EntityManager entityManager, out Entity entity)
        {
            return TryGetEntityAndManager(out entityManager, out entity)
                   && entityManager.HasComponent(entity, GetComponentType());
        }

        void OnValidate()
        {
            ValidateSerializedData();
            EntityManager entityManager;
            Entity entity;
            if (CanSynchronizeWithEntityManager(out entityManager, out entity))
            {
                UpdateComponentData(entityManager, entity);
                if (CheckComponentChange())
                {
                    UpdateStringData();
                }
                else if (CheckStringChange())
                {
                    UpdateIndexData();
                }
            }
        }

        void Reset()
        {
            OnValidate();
        }

        private void UpdateStringData()
        {
            foreach (var field in m_ComponentFields)
            {
                if (m_StringIndexList.Contains(field.Name))
                {
                    var index = m_StringIndexList.IndexOf(field.Name);
                    m_StringTable[index] = stringHelper.GetStringByIndex((int) field.GetValue(m_ComponentData));
                }
            }
        }

        private void UpdateIndexData()
        {
            T t = new T();
            object obj = t;
            foreach (var field in m_ComponentFields)
            {
                if (m_StringIndexList.Contains(field.Name))
                {
                    var index = m_StringIndexList.IndexOf(field.Name);
                    var value = stringHelper.SetString(m_StringTable[index]);

                    field.SetValue(obj, value);
                }
            }
            t = (T) obj;
            Value = t;
            SetLastComponentValue();
        }

        private bool CheckComponentChange()
        {
            foreach (var field in m_ComponentFields)
            {
                if (!field.GetValue(m_LastComponentData).Equals(field.GetValue(m_ComponentData)))
                {
                    SetLastComponentValue();
                    return true;
                }
            }

            return false;
        }

        private bool CheckStringChange()
        {
            if (!m_LastStringTable.Equals(m_StringTable))
            {
                SetLastStringValue();
                return true;
            }

            return false;
        }

        private void SetLastComponentValue()
        {
            m_LastComponentData = m_ComponentData;
        }

        private void SetLastStringValue()
        {
            for (int i = 0; i < m_StringTable.Count; i++)
            {
                m_LastStringTable[i] = m_StringTable[i];
            }
        }

        private void InitializeDictionary()
        {
            m_StringIndexList = new List<string>();

            m_ComponentType = typeof(T);
            m_ComponentFields = m_ComponentType.GetFields();

            foreach (var field in m_ComponentFields)
            {
                if (field.Name.Contains("StringIndex"))
                {
                    m_StringIndexList.Add(field.Name);
                }
            }

            m_StringTable = new List<string>(new string[m_StringIndexList.Count]);
            m_LastStringTable = new List<string>(new string[m_StringIndexList.Count]);

            UpdateStringData();
        }
    }
}