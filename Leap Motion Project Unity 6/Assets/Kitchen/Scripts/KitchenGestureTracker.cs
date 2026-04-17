using System.Collections.Generic;
using Leap;
using Leap.Unity;
using UnityEngine;

namespace KitchenGame
{
    public class KitchenGestureTracker : MonoBehaviour
    {
        [SerializeField]
        private LeapProvider leapProvider;

        [SerializeField]
        private KitchenGameManager gameManager;

        [SerializeField]
        private float pinchThreshold = 0.8f;

        [SerializeField]
        private float chopVelocityThreshold = 0.9f;

        [SerializeField]
        private float stirDistanceThreshold = 0.12f;

        [SerializeField]
        private float stirVelocityThreshold = 0.2f;

        [SerializeField]
        private float flipVelocityThreshold = 1.1f;

        [SerializeField]
        private float heldItemDetectionRadius = 0.25f;

        [SerializeField]
        private float gestureCooldown = 0.2f;

        private readonly Dictionary<Chirality, HandGestureState> handStates = new();

        private void Reset()
        {
            leapProvider = FindFirstObjectByType<LeapProvider>();
            gameManager = FindFirstObjectByType<KitchenGameManager>();
        }

        private void Update()
        {
            if (leapProvider == null || gameManager == null || !gameManager.RoundActive)
            {
                return;
            }

            var frame = leapProvider.CurrentFrame;
            if (frame == null || frame.Hands == null)
            {
                return;
            }

            foreach (var hand in frame.Hands)
            {
                ProcessHand(hand);
            }
        }

        private void ProcessHand(Hand hand)
        {
            var chirality = hand.IsLeft ? Chirality.Left : Chirality.Right;
            if (!handStates.TryGetValue(chirality, out var state))
            {
                state = new HandGestureState
                {
                    LastPalmPosition = hand.PalmPosition
                };
            }

            var palmPosition = hand.PalmPosition;
            var palmVelocity = hand.PalmVelocity;
            var heldItem = FindHeldItemNear(palmPosition);
            var now = Time.time;

            if (!state.PinchActive && hand.PinchStrength >= pinchThreshold && now >= state.NextAllowedGestureTime)
            {
                EmitGesture(KitchenGestureType.Pinch, palmPosition, heldItem, hand.PinchStrength);
                state.PinchActive = true;
                state.NextAllowedGestureTime = now + gestureCooldown;
            }
            else if (state.PinchActive && hand.PinchStrength < pinchThreshold * 0.65f)
            {
                state.PinchActive = false;
            }

            var travel = Vector3.Distance(state.LastPalmPosition, palmPosition);
            var planarVelocity = new Vector2(palmVelocity.x, palmVelocity.z).magnitude;
            state.StirAccumulator = planarVelocity > stirVelocityThreshold ? state.StirAccumulator + travel : 0f;

            if (heldItem is KitchenTool knife &&
                knife.ToolType == KitchenToolType.Knife &&
                palmVelocity.y <= -chopVelocityThreshold &&
                now >= state.NextAllowedGestureTime)
            {
                EmitGesture(KitchenGestureType.Chop, palmPosition, heldItem, Mathf.Abs(palmVelocity.y));
                state.NextAllowedGestureTime = now + gestureCooldown;
                state.StirAccumulator = 0f;
            }
            else if (state.StirAccumulator >= stirDistanceThreshold && now >= state.NextAllowedGestureTime)
            {
                EmitGesture(KitchenGestureType.Stir, palmPosition, heldItem, Mathf.Max(1f, planarVelocity));
                state.NextAllowedGestureTime = now + gestureCooldown;
                state.StirAccumulator = 0f;
            }
            else if (heldItem is KitchenTool pan &&
                     pan.ToolType == KitchenToolType.Pan &&
                     palmVelocity.y >= flipVelocityThreshold &&
                     now >= state.NextAllowedGestureTime)
            {
                EmitGesture(KitchenGestureType.Flip, palmPosition, heldItem, palmVelocity.y);
                state.NextAllowedGestureTime = now + gestureCooldown;
                state.StirAccumulator = 0f;
            }

            state.LastPalmPosition = palmPosition;
            handStates[chirality] = state;
        }

        private KitchenItem FindHeldItemNear(Vector3 worldPosition)
        {
            var items = FindObjectsByType<KitchenItem>(FindObjectsSortMode.None);
            KitchenItem bestItem = null;
            var bestDistance = heldItemDetectionRadius;

            foreach (var item in items)
            {
                if (item == null || !item.IsHeld)
                {
                    continue;
                }

                var distance = item.DistanceTo(worldPosition);
                if (distance < bestDistance)
                {
                    bestItem = item;
                    bestDistance = distance;
                }
            }

            return bestItem;
        }

        private void EmitGesture(KitchenGestureType gesture, Vector3 worldPosition, KitchenItem heldItem, float intensity)
        {
            gameManager.RegisterGesture(gesture, worldPosition, heldItem, intensity);
        }

        private struct HandGestureState
        {
            public bool PinchActive;
            public Vector3 LastPalmPosition;
            public float StirAccumulator;
            public float NextAllowedGestureTime;
        }
    }
}
