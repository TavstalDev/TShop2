using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Allows Unity events to broadcast Event NPC rewards.
	/// </summary>
	[AddComponentMenu("Unturned/NPC Global Event Messenger")]
	public class NpcGlobalEventMessenger : MonoBehaviour
	{
		/// <summary>
		/// Event ID to use when SendDefaultEventId is invoked.
		/// </summary>
		public string DefaultEventId = null;

		/// <summary>
		/// The event messenger can only be triggered on the authority (server).
		/// If true, the server will replicate the event to clients.
		/// </summary>
		public bool ShouldReplicate = false;

		public void SendEventId(string eventId)
		{
#if GAME
			if (Provider.isServer && !string.IsNullOrEmpty(eventId))
			{
				NPCEventManager.broadcastEvent(null, eventId, shouldReplicate: ShouldReplicate);
			}
#endif // GAME
		}

		public void SendDefaultEventId()
		{
			SendEventId(DefaultEventId);
		}
	}
}
