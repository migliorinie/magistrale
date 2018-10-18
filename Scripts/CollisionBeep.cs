using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionBeep : MonoBehaviour {

	AudioSource m_MyAudioSource;
	
    void Start()
    {
        m_MyAudioSource = GetComponent<AudioSource>();
	}
	
	void OnCollisionEnter (Collision collision) {
		if (!m_MyAudioSource.isPlaying) {
			m_MyAudioSource.Play();
		}
	}
}
