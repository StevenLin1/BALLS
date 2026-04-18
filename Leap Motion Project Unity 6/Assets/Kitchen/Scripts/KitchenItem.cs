using System;
using Leap.Unity.Interaction;
using UnityEngine;
using Leap.Unity;

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

        public bool IsHeldBy(Chirality chirality)
        {
            if (interactionBehaviour == null || !interactionBehaviour.isGrasped)
            {
                return false;
            }

            foreach (var hand in interactionBehaviour.graspingHands)
            {
                if (hand == null || hand.leapHand == null)
                {
                    continue;
                }

                if (hand.leapHand.IsLeft == (chirality == Chirality.Left))
                {
                    return true;
                }
            }

            return false;
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
