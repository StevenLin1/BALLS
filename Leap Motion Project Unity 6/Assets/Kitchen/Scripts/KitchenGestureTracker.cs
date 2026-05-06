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
            state.LastPalmPosition = palmPosition;
            handStates[chirality] = state;
        }

        private struct HandGestureState
        {
            public Vector3 LastPalmPosition;
        }
    }
}
