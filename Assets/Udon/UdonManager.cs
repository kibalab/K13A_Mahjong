using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRC.Udon.ClientBindings;
using VRC.Udon.ClientBindings.Interfaces;
using VRC.Udon.Common.Interfaces;

namespace VRC.Udon
{
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    public class UdonManager : MonoBehaviour, IUdonClientInterface
    {
        public UdonBehaviour currentlyExecuting;
        
        private static UdonManager _instance;
        private static bool _isUdonEnabled = true;
        private static Dictionary<GameObject, List<UdonBehaviour>> _sceneBehaviours = new Dictionary<GameObject, List<UdonBehaviour>>();

        public static UdonManager Instance
        {
            get
            {
                if(_instance != null)
                {
                    return _instance;
                }

                GameObject udonManagerGameObject = new GameObject("UdonManager");
                if(Application.isPlaying)
                {
                    DontDestroyOnLoad(udonManagerGameObject);
                }

                _instance = udonManagerGameObject.AddComponent<UdonManager>();
                return _instance;
            }
        }

        private IUdonClientInterface _udonClientInterface;

        private IUdonClientInterface UdonClientInterface
        {
            get
            {
                if(_udonClientInterface != null)
                {
                    return _udonClientInterface;
                }

                _udonClientInterface = new UdonClientInterface();

                return _udonClientInterface;
            }
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode _)
        {
            _sceneBehaviours.Clear();
            
            if (!_isUdonEnabled)
            {
                VRC.Core.Logger.LogWarning("Udon is disabled globally, Udon components will be removed from the scene.");
            }
            
            GameObject[] sceneRootGameObjects = scene.GetRootGameObjects();
            List<UdonBehaviour> udonBehavioursWorkingList = new List<UdonBehaviour>();
            foreach(GameObject rootGameObject in sceneRootGameObjects)
            {
                rootGameObject.GetComponentsInChildren(true, udonBehavioursWorkingList);
                foreach(UdonBehaviour udonBehaviour in udonBehavioursWorkingList)
                {
                    if (_isUdonEnabled)
                    {
                        if (!_sceneBehaviours.TryGetValue(udonBehaviour.gameObject,
                            out List<UdonBehaviour> behavioursOnObject))
                        {
                            behavioursOnObject = new List<UdonBehaviour>();
                            _sceneBehaviours.Add(udonBehaviour.gameObject, behavioursOnObject);
                        }
                        behavioursOnObject.Add(udonBehaviour);
                    }
                    else
                    {
                        Destroy(udonBehaviour);
                    }
                }
            }
        }

        public void Awake()
        {
            if(_instance == null)
            {
                _instance = this;
            }

            DebugLogging = Application.isEditor;

            if(this == Instance)
            {
                return;
            }

            if(Application.isPlaying)
            {
                Destroy(this);
            }
            else
            {
                DestroyImmediate(this);
            }

            PrimitiveType[] primitiveTypes = (PrimitiveType[])Enum.GetValues(typeof(PrimitiveType));
            foreach(PrimitiveType primitiveType in primitiveTypes)
            {
                GameObject go = GameObject.CreatePrimitive(primitiveType);
                Mesh primitiveMesh = go.GetComponent<MeshFilter>().sharedMesh;
                Destroy(go);
                Blacklist(primitiveMesh);
            }
        }

        [PublicAPI]
        public static void SetUdonEnabled(bool isEnabled)
        {
            _isUdonEnabled = isEnabled;
        }

        public IUdonVM ConstructUdonVM()
        {
            return !_isUdonEnabled ? null : UdonClientInterface.ConstructUdonVM();
        }

        public void FilterBlacklisted<T>(ref T objectToFilter) where T : class
        {
            UdonClientInterface.FilterBlacklisted(ref objectToFilter);
        }


        public void Blacklist(UnityEngine.Object objectToBlacklist)
        {
            UdonClientInterface.Blacklist(objectToBlacklist);
        }

        public void Blacklist(IEnumerable<UnityEngine.Object> objectsToBlacklist)
        {
            UdonClientInterface.Blacklist(objectsToBlacklist);
        }

        public void FilterBlacklisted(ref UnityEngine.Object objectToFilter)
        {
            UdonClientInterface.FilterBlacklisted(ref objectToFilter);
        }

        public void ClearBlacklist()
        {
            UdonClientInterface.ClearBlacklist();
        }

        public IUdonWrapper GetWrapper()
        {
            return UdonClientInterface.GetWrapper();
        }

        //Run an udon event on all objects in the scene
        [PublicAPI]
        public void RunEvent(string eventName, params (string symbolName, object value)[] programVariables)
        {
            foreach (List<UdonBehaviour> udonBehaviourList in _sceneBehaviours.Values)
            {
                foreach (UdonBehaviour udonBehaviour in udonBehaviourList)
                {
                    udonBehaviour.RunEvent(eventName, programVariables);    
                }
            }
        }
        
        //Run an udon event on a specific gameObject
        [PublicAPI]
        public void RunEvent(GameObject eventReceiverObject, string eventName, params (string symbolName, object value)[] programVariables)
        {
            if (_sceneBehaviours.TryGetValue(eventReceiverObject, out List<UdonBehaviour> eventReceiverBehaviourList))
            {
                foreach (UdonBehaviour udonBehaviour in eventReceiverBehaviourList)
                {
                    udonBehaviour.RunEvent(eventName, programVariables);    
                }
            }
        }

        public bool DebugLogging
        {
            get => UdonClientInterface.DebugLogging;
            set => UdonClientInterface.DebugLogging = value;
        }
    }
}
