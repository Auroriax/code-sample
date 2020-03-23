using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InstancePool : MonoBehaviour
{
    public GameObject objectToPool;
    [Range(0,1000)]
    public int poolSize = 100;

    [Header("Overflow Behavior")]
    [Tooltip("If the pool limit is reached, create more instances. These will not be destroyed once the pool falls back to its old limit.")]
    [Range(0, 1000)]
    public int additiveOverflowAmount;
    [Tooltip("If the additive overflow is set, this ensures a maximum cap of instances, after which it falls back to descructive overflow (if enabled).")]
    [Range(0, 1000)]
    public int additiveOverflowMax;
    [Tooltip("How to behave if no unused objects remain in the pool when an activation request is made. If false, activating more instances is not allowed. If true, the oldest activated object will be recycled instead.")]
    public bool destructiveOverflow = false;

    [Header("Events")]
    public UnityEvent onSizeChange;
    public UnityEvent onActivate;
    public UnityEvent onDeactivate;

    private List<GameObject> objectPool = new List<GameObject>();

    void Start()
    {
        ResizePool(poolSize);

        if (additiveOverflowMax < poolSize)
        {
            additiveOverflowMax = poolSize;
        }
    }

    private void Update()
    {
        if (poolSize != objectPool.Count)
        {
            onSizeChange.Invoke();
        }

        if (additiveOverflowAmount < 0)
        {
            additiveOverflowAmount = 0;
        }
    }

    private void OnEnable()
    {
        onSizeChange.AddListener(() => ResizePool(poolSize));
    }

    private void OnDisable()
    {
        onSizeChange.RemoveListener(() => ResizePool(poolSize));
    }

    /// <summary>
    /// Resizes the pool. Instantiates more objects if pool became bigger, will destroy upon shrinking (with unactivated objects first, then the least recently activated objects.
    /// </summary>
    /// <param name="amount"></param>
    public void ResizePool(int amount)
    {
        poolSize = Mathf.Max(0, amount);

        if (objectPool.Count == poolSize)
        {
            return;
        }
        else
        {
            if (objectPool.Count < poolSize)
            {
                for (var i = objectPool.Count; i != poolSize; i += 1)
                {
                    var createdObject = Instantiate(objectToPool);
                    objectPool.Add(createdObject);
                    createdObject.name = objectToPool.name + " (" + i.ToString() + ")";
                    createdObject.transform.SetParent(this.transform);
                    createdObject.SetActive(false); //References of disabled objects are hard to get, so we add first and disable afterwards
                }
            }
            else
            {
                while (objectPool.Count != poolSize)
                {
                    var objectToDestroy = objectPool.Find(x => x.activeSelf == false);

                    Destroy(objectPool[0]);
                    objectPool.RemoveAt(0);
                }
            }
        }
    }

    /// <summary>
    /// Activate an instance from the pool at the local origin. If this causes the pool to overflow, the enabled overflow handlers are excecuted.
    /// </summary>
    /// <returns></returns>
    public GameObject ActivateFromPool()
    {
        return ActivateFromPool(Vector3.zero);
    }

    /// <summary>
    /// Activate an instance from the pool on a specific position. If this causes the pool to overflow, the enabled overflow handlers are excecuted.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public GameObject ActivateFromPool(Vector3 position)
    {
        var objectToActivate = objectPool.Find(x => x.activeSelf == false);

        if (objectToActivate == null)
        {
            if (poolSize < additiveOverflowMax)
            {
                ResizePool(Mathf.Min(poolSize + additiveOverflowAmount, additiveOverflowMax));
                objectToActivate = objectPool.Find(x => x.activeSelf == false);
            }
            else if (destructiveOverflow)
            {
                objectToActivate = DeactivateFromPool();
            }
            else
            {
                return null;
            }
        }

        objectToActivate.SetActive(true);
        objectToActivate.transform.position = position;

        onActivate.Invoke();
        return objectToActivate;
    }

    /// <summary>
    /// Deactivates the least recently activated instance from pool.
    /// </summary>
    /// <returns>The game object that was deactivated, or null on fail.</returns>
    public GameObject DeactivateFromPool()
    {
        var objectToDeactivate = objectPool.Find(x => x.activeSelf == true);
        if (objectToDeactivate != null)
        {
            DeactivateFromPool(objectToDeactivate);
            return objectToDeactivate;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Deactivates the passed object, if it's in the pool.
    /// </summary>
    /// <returns>The game object that was deactivated upon success, or null on fail.</returns>
    public GameObject DeactivateFromPool(GameObject objectToDeactivate)
    {
        if (objectPool.Find(x => x == objectToDeactivate))
        {
            objectToDeactivate.SetActive(false);
        }
        else
        {
            return null;
        }

        //Move object to the back of the pool, to ensure that (when using destructive overflow) always the least recently initiated object will be destroyed.
        objectPool.Remove(objectToDeactivate);
        objectPool.Add(objectToDeactivate); 

        onDeactivate.Invoke();
        return objectToDeactivate;
    }
}
