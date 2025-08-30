using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MechanicGames.BlockBlast
{
    public class BlockBlastObjectPool : MonoBehaviour
    {
        [System.Serializable]
        public class PoolItem
        {
            public string tag;
            public GameObject prefab;
            public int poolSize;
            public bool expandable = true;
        }
        
        [Header("Pool Configuration")]
        [SerializeField] private List<PoolItem> poolItems = new List<PoolItem>();
        [SerializeField] private Transform poolParent;
        
        [Header("Mobile Optimization")]
        [SerializeField] private bool enablePooling = true;
        [SerializeField] private int maxPoolSize = 100;
        [SerializeField] private bool autoCleanup = true;
        [SerializeField] private float cleanupInterval = 30f;
        
        private Dictionary<string, Queue<GameObject>> pools;
        private Dictionary<string, GameObject> prefabLookup;
        private Dictionary<GameObject, string> objectTags;
        
        private float lastCleanupTime = 0f;
        private int totalPooledObjects = 0;
        
        public static BlockBlastObjectPool Instance { get; private set; }
        public bool EnablePooling => enablePooling;
        public int TotalPooledObjects => totalPooledObjects;
        
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializePool();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        void Start()
        {
            if (poolParent == null)
            {
                poolParent = transform;
            }
        }
        
        void Update()
        {
            if (autoCleanup && Time.time - lastCleanupTime > cleanupInterval)
            {
                CleanupUnusedObjects();
                lastCleanupTime = Time.time;
            }
        }
        
        void InitializePool()
        {
            if (!enablePooling) return;
            
            pools = new Dictionary<string, Queue<GameObject>>();
            prefabLookup = new Dictionary<string, GameObject>();
            objectTags = new Dictionary<GameObject, string>();
            
            foreach (var item in poolItems)
            {
                if (item.prefab == null || string.IsNullOrEmpty(item.tag)) continue;
                
                prefabLookup[item.tag] = item.prefab;
                pools[item.tag] = new Queue<GameObject>();
                
                // Pre-populate pool
                for (int i = 0; i < item.poolSize; i++)
                {
                    CreatePooledObject(item.tag);
                }
            }
        }
        
        GameObject CreatePooledObject(string tag)
        {
            if (!prefabLookup.ContainsKey(tag)) return null;
            
            GameObject obj = Instantiate(prefabLookup[tag], poolParent);
            obj.SetActive(false);
            
            // Add pool component for tracking
            var poolComponent = obj.GetComponent<PooledObject>();
            if (poolComponent == null)
            {
                poolComponent = obj.AddComponent<PooledObject>();
            }
            poolComponent.Initialize(tag);
            
            pools[tag].Enqueue(obj);
            objectTags[obj] = tag;
            totalPooledObjects++;
            
            return obj;
        }
        
        public GameObject GetPooledObject(string tag, Vector3 position, Quaternion rotation)
        {
            if (!enablePooling || !pools.ContainsKey(tag))
            {
                // Fallback to instantiation
                if (prefabLookup.ContainsKey(tag))
                {
                    return Instantiate(prefabLookup[tag], position, rotation);
                }
                return null;
            }
            
            GameObject obj = null;
            
            // Try to get from pool
            if (pools[tag].Count > 0)
            {
                obj = pools[tag].Dequeue();
            }
            else
            {
                // Check if we can expand the pool
                var poolItem = poolItems.Find(item => item.tag == tag);
                if (poolItem != null && poolItem.expandable && totalPooledObjects < maxPoolSize)
                {
                    obj = CreatePooledObject(tag);
                }
            }
            
            if (obj != null)
            {
                obj.transform.position = position;
                obj.transform.rotation = rotation;
                obj.SetActive(true);
                
                var poolComponent = obj.GetComponent<PooledObject>();
                if (poolComponent != null)
                {
                    poolComponent.OnSpawn();
                }
            }
            
            return obj;
        }
        
        public T GetPooledObject<T>(string tag, Vector3 position, Quaternion rotation) where T : Component
        {
            GameObject obj = GetPooledObject(tag, position, rotation);
            return obj != null ? obj.GetComponent<T>() : null;
        }
        
        public void ReturnToPool(GameObject obj)
        {
            if (!enablePooling || obj == null) return;
            
            var poolComponent = obj.GetComponent<PooledObject>();
            if (poolComponent == null) return;
            
            string tag = poolComponent.Tag;
            
            if (pools.ContainsKey(tag))
            {
                obj.SetActive(false);
                obj.transform.SetParent(poolParent);
                
                // Reset object state
                poolComponent.OnReturn();
                
                pools[tag].Enqueue(obj);
            }
        }
        
        public void ReturnToPool(Component component)
        {
            if (component != null)
            {
                ReturnToPool(component.gameObject);
            }
        }
        
        public void CleanupUnusedObjects()
        {
            if (!enablePooling) return;
            
            int cleanedCount = 0;
            
            foreach (var pool in pools.Values)
            {
                while (pool.Count > 0)
                {
                    GameObject obj = pool.Dequeue();
                    if (obj != null)
                    {
                        DestroyImmediate(obj);
                        cleanedCount++;
                        totalPooledObjects--;
                    }
                }
            }
            
            if (cleanedCount > 0)
            {
                Debug.Log($"ObjectPool: Cleaned up {cleanedCount} unused objects");
            }
        }
        
        public void ClearAllPools()
        {
            if (!enablePooling) return;
            
            foreach (var pool in pools.Values)
            {
                while (pool.Count > 0)
                {
                    GameObject obj = pool.Dequeue();
                    if (obj != null)
                    {
                        DestroyImmediate(obj);
                        totalPooledObjects--;
                    }
                }
            }
            
            pools.Clear();
            objectTags.Clear();
        }
        
        public int GetPoolSize(string tag)
        {
            return pools.ContainsKey(tag) ? pools[tag].Count : 0;
        }
        
        public void SetPoolingEnabled(bool enabled)
        {
            enablePooling = enabled;
        }
        
        void OnDestroy()
        {
            ClearAllPools();
        }
    }
    
    // Component for tracking pooled objects
    public class PooledObject : MonoBehaviour
    {
        private string tag;
        private bool isPooled = false;
        
        public string Tag => tag;
        public bool IsPooled => isPooled;
        
        public void Initialize(string objectTag)
        {
            tag = objectTag;
            isPooled = true;
        }
        
        public void OnSpawn()
        {
            isPooled = false;
        }
        
        public void OnReturn()
        {
            isPooled = true;
            
            // Reset common components
            var rectTransform = GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.localScale = Vector3.one;
                rectTransform.localRotation = Quaternion.identity;
            }
            
            var image = GetComponent<Image>();
            if (image != null)
            {
                image.color = Color.white;
            }
            
            var text = GetComponent<Text>();
            if (text != null)
            {
                text.text = "";
                text.color = Color.white;
            }
        }
        
        void OnDisable()
        {
            // Auto-return to pool when disabled
            if (isPooled && BlockBlastObjectPool.Instance != null)
            {
                BlockBlastObjectPool.Instance.ReturnToPool(this);
            }
        }
    }
}
