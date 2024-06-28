namespace GoblinzMechanics.Game
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.Pool;

    public class RouteController : MonoBehaviour
    {
        [SerializeField] private bool _autoExpandPool = false;

        [SerializeField] private int _routeShowLength = 13;

        [SerializeField] private float _routePartLength = 1f;
        [SerializeField] private float _routeFloorThickness = 0.2f;
        [SerializeField] private float _routeSpeed = 10f;
        [SerializeField] private float _routeSpeedModificator = 1f;

        [SerializeField] private Transform _routeParent;
        [SerializeField] private Transform _routePool;

        [SerializeField] private List<RouteObject> _routeObjectPrefabs = new List<RouteObject>();
        [SerializeField] private List<RouteObject> _routeObjects = new List<RouteObject>();

        [SerializeField] private Dictionary<int, Queue<RouteObject>> _routeObjectsPool = new Dictionary<int, Queue<RouteObject>>();

        private Coroutine pathMovement;

        private float __totalChance = 0f;

        private void OnEnable()
        {
            if (_routeObjectPrefabs.Count < 1)
            {
                enabled = false;
                Debug.LogError("Route Object Prefabs is empty!");
                return;
            }
            __totalChance = _routeObjectPrefabs.Sum((pref) => pref.routeChance);

            ClearPool();
            FillPool();

            pathMovement = StartCoroutine(MovePath());
        }

        private IEnumerator MovePath()
        {
            while (GoblinGameManager.Instance.GameState == GoblinGameManager.GameStateEnum.Playing)
            {
                if (_routeObjects.Count < 1)
                {
                    AddFirst();
                }
                else if (_routeObjects.Count < _routeShowLength)
                {
                    while (_routeObjects.Count < _routeShowLength)
                    {
                        AddForward();
                    }
                }

                _routeParent.position += Vector3.back * _routeSpeed * _routeSpeedModificator * Time.deltaTime;
                yield return null;
            }
        }

        private void OnDisable()
        {
            if (pathMovement != null)
            {
                StopCoroutine(pathMovement);
            }
        }

        private void AddFirst()
        {
            RouteObject routeObject = TakeFromPool(GetByChance());
            routeObject.transform.position = Vector3.zero - Vector3.up * _routeFloorThickness + Vector3.back * _routePartLength;
            routeObject.transform.rotation = Quaternion.identity;

            _routeObjects.Add(routeObject);
        }

        private void AddForward()
        {
            RouteObject routeObject = TakeFromPool(GetByChance());
            routeObject.transform.position = _routeObjects[_routeObjects.Count - 1].transform.position + Vector3.forward * _routePartLength;
            routeObject.transform.rotation = Quaternion.identity;

            _routeObjects.Add(routeObject);
        }

        public void DestroyBehind()
        {
            if (_routeObjects.Count < 1) return;

            RouteObject routeObj = _routeObjects[0];
            ReleasePoolObject(routeObj);

            _routeObjects.RemoveAt(0);
        }

        private void DestroyAllInstance()
        {
            for (int i = 0; i < _routeObjects.Count; i++)
            {
                Destroy(_routeObjects[i].gameObject);
            }
            _routeObjects.Clear();
        }

        private void ClearPool()
        {
            for (int i = _routePool.childCount - 1; i > 0; i--)
            {
                Destroy(_routePool.GetChild(i).gameObject);
            }
            _routeObjectsPool.Clear();
        }

        private void FillPool()
        {
            foreach (var prefab in _routeObjectPrefabs)
            {
                for (int i = 0; i < _routeShowLength; i++)
                {
                    RouteObject routeObject = Instantiate(prefab, Vector3.zero, Quaternion.identity, _routePool);
                    routeObject.gameObject.SetActive(false);
                    if (_routeObjectsPool.ContainsKey(prefab.id))
                    {
                        _routeObjectsPool[prefab.id].Enqueue(routeObject);
                    } else
                    {
                        var queue = new Queue<RouteObject>();
                        queue.Enqueue(routeObject);
                        _routeObjectsPool.Add(prefab.id, queue);
                    }
                }
            }
        }

        private RouteObject TakeFromPool(RouteObject prefab)
        {
            if (_routeObjectsPool.Count > 0)
            {
                RouteObject routeObject = _routeObjectsPool[prefab.id].Dequeue();
                routeObject.gameObject.SetActive(true);
                routeObject.transform.parent = _routeParent;
                return routeObject;
            }
            else if (_autoExpandPool)
            {
                RouteObject routeObject = Instantiate(prefab, Vector3.zero, Quaternion.identity, _routePool);
                routeObject.gameObject.SetActive(false);
                return routeObject;
            }
            else
            {
                return null;
            }
        }

        private void ReleasePoolObject(RouteObject routeObject)
        {
            routeObject.gameObject.SetActive(false);
            routeObject.transform.parent = _routePool;
            if (_routeObjectsPool.ContainsKey(routeObject.id))
            {
                Debug.Log($"Object: {routeObject} Released");
                _routeObjectsPool[routeObject.id].Enqueue(routeObject);
            }
        }
        
        private RouteObject GetByChance()
        {
            float chance = Random.Range(0f, __totalChance);
            RouteObject result = _routeObjectPrefabs.FirstOrDefault(
                (route) =>
                {
                    chance -= route.routeChance;
                    return chance <= 0f;
                });
            
            if (result == null)
            {
                result = _routeObjectPrefabs[0];
                Debug.Log($"Result = GetDefault 0, Chance = {chance}");
            } else {
                Debug.Log($"Result = {result.name}, Chance = {chance}");
            }
            return result;
        }
    }
}