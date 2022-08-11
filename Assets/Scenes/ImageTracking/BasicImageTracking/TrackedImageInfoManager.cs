using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;

namespace UnityEngine.XR.ARFoundation.Samples
{
    /// This component listens for images detected by the <c>XRImageTrackingSubsystem</c>
    /// and overlays some information as well as the source Texture2D on top of the
    /// detected image.
    /// </summary>
    [RequireComponent(typeof(ARTrackedImageManager))]
    public class TrackedImageInfoManager : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The camera to set on the world space UI canvas for each instantiated image info.")]
        Camera m_WorldSpaceCanvasCamera;

        /// <summary>
        /// The prefab has a world space UI canvas,
        /// which requires a camera to function properly.
        /// </summary>
        public Camera worldSpaceCanvasCamera
        {
            get { return m_WorldSpaceCanvasCamera; }
            set { m_WorldSpaceCanvasCamera = value; }
        }

        [SerializeField]
        [Tooltip("If an image is detected but no source texture can be found, this texture is used instead.")]
        Texture2D m_DefaultTexture;

        /// <summary>
        /// If an image is detected but no source texture can be found,
        /// this texture is used instead.
        /// </summary>
        public Texture2D defaultTexture
        {
            get { return m_DefaultTexture; }
            set { m_DefaultTexture = value; }
        }

        ARTrackedImageManager m_TrackedImageManager;

        [SerializeField]
        private GameObject[] _ARPrefabsToPlace;

        private Dictionary<string, GameObject> _ARPrefabs = new Dictionary<string, GameObject>();

        void Awake()
        {
            m_TrackedImageManager = GetComponent<ARTrackedImageManager>();
            // setup all game objects in dictionary
            foreach(GameObject defaultPrefab in _ARPrefabsToPlace)
            {
                GameObject newPrefab = Instantiate(defaultPrefab, Vector3.zero, Quaternion.identity);
                newPrefab.name = defaultPrefab.name;
                newPrefab.SetActive(false);
                _ARPrefabs.Add(defaultPrefab.name, newPrefab);
            }
        }

        void OnEnable()
        {
            m_TrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
        }

        void OnDisable()
        {
            m_TrackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
        }

        #region superceded - UpdateInfo (from sample)
        void UpdateInfo(ARTrackedImage trackedImage)
        {
            // Set canvas camera
            var canvas = trackedImage.GetComponentInChildren<Canvas>();
            canvas.worldCamera = worldSpaceCanvasCamera;

            // Update information about the tracked image
            var text = canvas.GetComponentInChildren<Text>();
            text.text = string.Format(
                "{0}\ntrackingState: {1}\nGUID: {2}\nReference size: {3} cm\nDetected size: {4} cm",
                trackedImage.referenceImage.name,
                trackedImage.trackingState,
                trackedImage.referenceImage.guid,
                trackedImage.referenceImage.size * 100f,
                trackedImage.size * 100f);

            var planeParentGo = trackedImage.transform.GetChild(0).gameObject;
            var planeGo = planeParentGo.transform.GetChild(0).gameObject;

            // Disable the visual plane if it is not being tracked
            if (trackedImage.trackingState != TrackingState.None && trackedImage.trackingState != TrackingState.Limited)
            {
                trackedImage.gameObject.SetActive(true);

                // The image extents is only valid when the image is being tracked
                trackedImage.transform.localScale = new Vector3(trackedImage.size.x, 1f, trackedImage.size.y);

                // Set the texture
                var material = planeGo.GetComponentInChildren<MeshRenderer>().material;
                material.mainTexture = (trackedImage.referenceImage.texture == null) ? defaultTexture : trackedImage.referenceImage.texture;
            }
            else
            {
                trackedImage.gameObject.SetActive(false);
            }
        }
        #endregion

        void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
        {
            ARTrackedImage trackedImage = null;

            // for new images detected
            for(int i = 0; i < eventArgs.added.Count; i++)
            {
                trackedImage = eventArgs.added[i];
                Debug.Log("NEW Image detected: " + trackedImage.referenceImage.name);
                UpdateARImage(trackedImage);
            }

            //foreach (var trackedImage in eventArgs.added)
            //{    
            //    //UpdateInfo(trackedImage);

            //    UpdateARImage(trackedImage);
            //}

            for(int i = 0; i < eventArgs.updated.Count; i++)
            {
                trackedImage = eventArgs.updated[i];
                if(trackedImage.trackingState == TrackingState.Tracking)
                {
                    UpdateARImage(trackedImage);
                } else
                {
                    // hide non-tracked object
                    _ARPrefabs[trackedImage.referenceImage.name].SetActive(false);
                }
            }

            //foreach (var trackedImage in eventArgs.updated)
            //{
            //    //UpdateInfo(trackedImage);

            //    UpdateARImage(trackedImage);
            //}

            //foreach(var trackedImage in eventArgs.removed)
            //{
            //    _ARPrefabs[trackedImage.name].SetActive(false);
            //}
        }

        void UpdateARImage(ARTrackedImage trackedImage)
        {
            //assign and place game object
            AssignGameObject(trackedImage.referenceImage.name, trackedImage.transform.position);
        }

        void AssignGameObject(string name, Vector3 newPosition)
        {
            if(_ARPrefabsToPlace != null)
            {
                _ARPrefabs[name].SetActive(true);
                _ARPrefabs[name].transform.position = newPosition;
                foreach(GameObject go in _ARPrefabs.Values)
                {
                    if(go.name != name)
                    {
                        go.SetActive(false);
                    }
                }
            }
        }

        public void AddDownloadedModels(GameObject[] downloadedModels)
        {
            foreach(GameObject downloadedModel in downloadedModels)
            {
                GameObject newPrefab = Instantiate(downloadedModel, Vector3.zero, Quaternion.identity);
                newPrefab.name = downloadedModel.name;
                newPrefab.SetActive(false);
                _ARPrefabs.Add(downloadedModel.name, newPrefab);
            }
        }
    }
}