using System;
using Leap.Unity.Interaction;
using UnityEngine;

namespace KitchenGame
{
    public class KitchenItem : MonoBehaviour
    {
        [SerializeField]
        private string displayName = "Kitchen Item";

        [SerializeField]
        private InteractionBehaviour interactionBehaviour;

        public event Action<KitchenItem> PickedUp;
        public event Action<KitchenItem> Released;

        public string DisplayName => displayName;
        public InteractionBehaviour InteractionBehaviour => interactionBehaviour;
        public bool IsHeld => interactionBehaviour != null && interactionBehaviour.isGrasped;

        protected virtual void Reset()
        {
            interactionBehaviour = GetComponent<InteractionBehaviour>();
        }

        protected virtual void Awake()
        {
            if (interactionBehaviour == null)
            {
                interactionBehaviour = GetComponent<InteractionBehaviour>();
            }
        }

        protected virtual void OnEnable()
        {
            if (interactionBehaviour == null)
            {
                return;
            }

            interactionBehaviour.OnGraspBegin += HandleGraspBegin;
            interactionBehaviour.OnGraspEnd += HandleGraspEnd;
        }

        protected virtual void OnDisable()
        {
            if (interactionBehaviour == null)
            {
                return;
            }

            interactionBehaviour.OnGraspBegin -= HandleGraspBegin;
            interactionBehaviour.OnGraspEnd -= HandleGraspEnd;
        }

        public float DistanceTo(Vector3 worldPosition)
        {
            return Vector3.Distance(transform.position, worldPosition);
        }

        private void HandleGraspBegin()
        {
            PickedUp?.Invoke(this);
        }

        private void HandleGraspEnd()
        {
            Released?.Invoke(this);
        }
    }
}
