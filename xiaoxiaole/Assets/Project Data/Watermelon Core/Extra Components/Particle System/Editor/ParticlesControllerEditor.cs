using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Watermelon
{
    [CustomEditor(typeof(ParticlesController))]
    public class ParticlesControllerEditor : Editor
    {
        private Dictionary<int, Particle> registeredParticles;

        private void OnEnable()
        {
            object boxedRegisteredParticles = ReflectionUtils.GetStaticComponent<ParticlesController>("registerParticles");
            if(boxedRegisteredParticles != null)
            {
                registeredParticles = (Dictionary<int, Particle>)boxedRegisteredParticles; 
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if(Application.isPlaying)
            {
                if (registeredParticles != null)
                {
                    int registeredParticlesCount = registeredParticles.Count;
                    if (registeredParticlesCount > 0)
                    {
                        EditorGUILayout.BeginVertical(WatermelonEditor.Styles.box);
                        EditorGUILayout.LabelField(string.Format("Registered particles: {0}", registeredParticlesCount));

                        foreach (Particle registeredParticle in registeredParticles.Values)
                        {
                            EditorGUILayout.BeginHorizontal(WatermelonEditor.Styles.box);

                            EditorGUILayout.LabelField(registeredParticle.ParticleName, GUILayout.Width(80));

                            GUILayout.FlexibleSpace();

                            using (new EditorGUI.DisabledScope(true))
                            {
                                EditorGUILayout.ObjectField(GUIContent.none, registeredParticle.ParticlePrefab, typeof(GameObject), allowSceneObjects: false, GUILayout.Width(80));
                            }

                            EditorGUILayout.EndHorizontal();
                        }

                        EditorGUILayout.EndVertical();
                    }
                }
            }
        }
    }
}
