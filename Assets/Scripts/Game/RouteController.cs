namespace GoblinzMechanics.Game
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Linq;
    using GoblinzMechanics.Utils;
    using UnityEngine;
    using UnityEngine.Pool;

    public class RouteController : Singleton<RouteController>
    {
        [SerializeField] private bool _autoExpandPool = false;

        [SerializeField] private int _startLength = 6;
        [SerializeField] private int _routeShowLength = 13;

        [SerializeField] private float _routeFloorThickness = 0.2f;
        [SerializeField] private float _routeSpeed = 10f;

        public float routeSpeedModificator = 1f;

        [SerializeField] private Transform _routeParent;
        [SerializeField] private Transform _routePool;

        [SerializeField] private MathRouteObject _mathRouteObjectPrefab;
        [SerializeField] private MathRouteObject _mathRouteObjectInstance;

        [SerializeField] private List<RouteObject> _routeObjectPrefabs = new List<RouteObject>();
        [SerializeField] private List<RouteObject> _routeObjects = new List<RouteObject>();

        [SerializeField] private Dictionary<int, Queue<RouteObject>> _routeObjectsPool = new Dictionary<int, Queue<RouteObject>>();

        public int routeCounter = 0;

        private Coroutine pathMovement;

        private float __totalChance = 0f;

        private bool _mathAdd = false;

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

            routeCounter = 0;

            pathMovement = StartCoroutine(MovePath());
        }

        private IEnumerator MovePath()
        {
            while (GoblinGameManager.Instance.GameState != GoblinGameManager.GameStateEnum.Ended)
            {
                if (_routeObjects.Count < 1)
                {
                    AddFirst();
                    for (int i = 0; i < _startLength; i++)
                    {
                        AddForward(true);
                    }
                }
                else if (_routeObjects.Count < _routeShowLength)
                {
                    while (_routeObjects.Count < _routeShowLength)
                    {
                        AddForward();
                    }
                }
                if (GoblinGameManager.Instance.GameState != GoblinGameManager.GameStateEnum.Playing)
                {
                    yield return null;
                    continue;
                }
                _routeParent.position += _routeSpeed * routeSpeedModificator * Time.deltaTime * Vector3.back;
                _mathAdd = _mathRouteObjectInstance == null && routeCounter > 0 && (int)(routeCounter % (12 * routeSpeedModificator)) == 0;
                GoblinGameManager.Instance.stats.maxSpeed = _routeSpeed * routeSpeedModificator;
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
            RouteObject routeObject = TakeFromPool(_routeObjectPrefabs[0]);
            routeObject.transform.SetPositionAndRotation(Vector3.zero - Vector3.up * _routeFloorThickness + Vector3.back * routeObject.length, Quaternion.identity);
            _routeObjects.Add(routeObject);
        }

        private void AddForward(bool asFirst = false)
        {
            RouteObject routeObject, _prevRouteObject;
            if (asFirst || !_mathAdd)
            {
                if (asFirst)
                {
                    routeObject = TakeFromPool(_routeObjectPrefabs[0]);
                }
                else
                {
                    routeObject = TakeFromPool(GetByChance());
                }
                _prevRouteObject = _routeObjects[^1];
                routeObject.Init(!_prevRouteObject.isRotate);
                routeObject.transform.SetPositionAndRotation(_prevRouteObject.transform.position + Vector3.forward * _prevRouteObject.length, Quaternion.identity);
                _routeObjects.Add(routeObject);
            }
            else if (_mathAdd)
            {
                _mathRouteObjectInstance = Instantiate(_mathRouteObjectPrefab, _routeParent);
                _prevRouteObject = _routeObjects[^1];
                _mathRouteObjectInstance.transform.SetPositionAndRotation(_prevRouteObject.transform.position + Vector3.forward * _prevRouteObject.length, Quaternion.identity);
                _routeObjects.AddRange(_mathRouteObjectInstance.routeObjects);
                _mathAdd = false;
            }
        }

        private int _prevRemovedId;
        public void DestroyBehind()
        {
            if (_routeObjects.Count < 1) return;

            RouteObject routeObj = _routeObjects[0];
            if (routeObj.id != 898) // removing ID
            {
                ReleasePoolObject(routeObj);
                routeCounter++;
                if (_prevRemovedId == 898)
                {
                    Destroy(_mathRouteObjectInstance.gameObject);
                }
                _prevRemovedId = routeObj.id;
            }
            else
            {
                _prevRemovedId = routeObj.id;
                Destroy(routeObj.gameObject);
            }

            _routeObjects.RemoveAt(0);
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
                    }
                    else
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
            }
            return result;
        }
    }
}