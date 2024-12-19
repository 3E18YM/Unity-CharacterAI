using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Sentis;
using System.IO;
using TMPro;
using Unity.Mathematics;
using UnityEngine.InputSystem;
using UnityEngine.UI;
    public class RunJets : MonoBehaviour
    {   
        public ModelAsset modelAsset ;
   
        public string inputText = "Once upon a time, there lived a girl called Alice. She lived in a house in the woods.";

        const int samplerate = 22050;
        
        Worker woker;

        AudioClip clip;

        void Start()
        {
            LoadModel();
           
            DoInference(inputText);
        }

        void LoadModel()
        {
            var model = ModelLoader.Load(modelAsset);
            woker = new Worker(model,BackendType.GPUCompute);
        }
       

        int[] GetTokens(string ptext)
        {
            string[] p = ptext.Split();
            var tokens = new int[p.Length];
            for (int i = 0; i < tokens.Length; i++)
            {
                //tokens[i] = Mathf.Max(0, System.Array.IndexOf(phonemes, p[i]));
            }
            return tokens;
        }
        

        public void DoInference(string ptext)
        {
            long[] inputValues = new long[inputText.Length];
            for (int i = 0; i < inputText.Length ;i++)
            {
                inputValues[i] = (long)inputText[i];
            }
            var inputShape = new TensorShape(inputText.Length);
            using var input = new Tensor<Int64>(inputShape, inputValues);
            woker.Schedule(input);

            var output = woker.PeekOutput("wav") as Tensor<float>;
            var samples = output.DownloadToArray();

            Debug.Log($"Audio size = {samples.Length / samplerate} seconds");

            clip = AudioClip.Create("voice audio", samples.Length, 1, samplerate, false);
            clip.SetData(samples, 0);

            Speak();
        }

        private void Speak()
        {
            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.clip = clip;
                audioSource.Play();
            }
            else
            {
                Debug.Log("There is no audio source");
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                DoInference(inputText);
            }
        }

        private void OnDestroy()
        {
            woker?.Dispose();
        }
    }
