
// Copyright (c) Olivier Goguel 2024
// Licensed under the MIT License.

using UnityEngine;

public class MatrixFX : MonoBehaviour
{
    [SerializeField]
    protected Material matrixMaterial;

    public ComputeShader white_noise_generator;
    public float RainSpeed  = 1.5f;
    public float RainScale = 0.5f;
    public float RainWave  = 0.0f;
    public float RainFade = 5.0f;
    private RenderTexture white_noise;
    ComputeShader generator;

    Material material ;
    public Material Material => material;

    protected  void Awake()
    {
        white_noise = new RenderTexture(512, 512, 0)
        {
            name = "white_noise_",
            enableRandomWrite = true,
            useMipMap = false,
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Repeat

        };
        white_noise.Create();
        generator = Instantiate(white_noise_generator);
        generator.SetTexture(0, "_white_noise", white_noise);

        Shader.SetGlobalInt("_session_rand_seed", UnityEngine.Random.Range(0, int.MaxValue));
         material = new Material(matrixMaterial);


#if UNITY_EDITOR
        material.EnableKeyword("UNITY_EDITOR");
#else
        material.DisableKeyword("UNITY_EDITOR");
#endif
    }

    private void Start()
    {
        material.SetTexture("_White_Noise", white_noise);
        material.SetFloat("_Rain_Speed", RainSpeed);
        material.SetFloat("_Rain_Scale", RainScale);
        material.SetFloat("_Rain_Wave", RainWave);
        material.SetFloat("_Rain_Fade", RainFade);
    }

    private void Update()
    {
        generator.SetInt("_session_rand_seed", Mathf.CeilToInt(Time.time * 6.0f));
        generator.Dispatch(0, 512 / 8, 512 / 8, 1);
    }

}
