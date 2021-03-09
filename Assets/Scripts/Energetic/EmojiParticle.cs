using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmojiParticle : MonoBehaviour
{
    private void Start()
    {
        ParticleSystemRenderer psRenderer = GetComponent<ParticleSystemRenderer>();
        psRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.Object;
    }

}
