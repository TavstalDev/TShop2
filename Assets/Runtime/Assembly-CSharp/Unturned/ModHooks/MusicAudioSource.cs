using UnityEngine;

namespace SDG.Unturned
{
	[RequireComponent(typeof(AudioSource))]
	[Tooltip("Reassigns AudioSource's outputAudioMixerGroup to the vanilla Music group")]
	public class MusicAudioSource : MonoBehaviour
	{
		private void Awake()
		{
#if GAME && !DEDICATED_SERVER
			AudioSource audioSource = GetComponent<AudioSource>();
			if (audioSource != null)
			{
				audioSource.outputAudioMixerGroup = UnturnedAudioMixer.GetMusicGroup();
			}
#endif // GAME && !DEDICATED_SERVER
		}
	}
}
