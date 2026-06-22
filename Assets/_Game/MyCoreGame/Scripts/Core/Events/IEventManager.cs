using System.Collections;
using System.Collections.Generic;
using Spine;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace EventGame
{
 
    public interface IEventManager
    {
        /// <summary>
        /// Determines whether the current event is active
        /// </summary>
        /// <returns></returns>
        public bool IsEventActive();

        /// <summary>
        /// Checks if the player has joined the event
        /// </summary>
        /// <returns></returns>
        public bool HasPlayerJoinedEvent();

        /// <summary>
        /// Checks whether the player has joined the previous event
        /// </summary>
        /// <returns></returns>
        public bool HasPlayerJoinedPreviousEvent();

        /// <summary>
        /// Check all config to active event
        /// </summary>
        public bool CanActiveEvent();

        /// <summary>
        /// Activates the event if either all conditions are met and previous events have been closed,
        /// or if the player has claimed rewards in the previous event.
        /// </summary>
        public void ActiveEvent();

        /// <summary>
        /// Show a popup join to the event if available
        /// </summary>
        /// <param name="onClose"></param>
        /// <returns></returns>
        public void ShowPopupJoin(System.Action onClose);
        
        public bool HasPopupJoinEvent();
        public EEventConfig GetEventName();
        public Sprite GetEventIcon();
        /// <summary>
        /// Events can add points at different difficulties in the level 
        /// </summary>
        /// <returns></returns>
        public bool CanMultiPointEvent();
        
        public AssetLabelReference GetAssetLabelReference();
        public EventDataConfig GetEventDataConfig();
        public string[] GetPopupsCache();
        public bool IsCachePopup();
    }
}