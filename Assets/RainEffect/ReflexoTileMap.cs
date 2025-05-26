using UnityEngine;
using UnityEngine.Tilemaps;

public class RippleDropsGreenRainEffect : MonoBehaviour
{
    [Header("Efeito de Chuva")]
    [Range(0, 0.2f)]
    public float distortionAmount = 0.08f;

    [Range(0.1f, 10f)]
    public float rippleSpeed = 0.8f;

    [Range(0, 1f)]
    public float greenTint = 0.5f;

    [Range(0, 0.5f)]
    public float darkness = 0.2f;

    [Range(0, 1f)]
    public float waterBlend = 0.7f;

    [Header("Controle de Movimento")]
    [Range(0, 1f)]
    public float horizontalMovement = 0.2f;

    [Header("Tamanho e Força das Gotas")]
    [Range(0.001f, 0.1f)]
    public float dropSize = 0.035f; // Tamanho do impacto inicial

    [Range(0, 0.5f)]
    public float dropBrightness = 0.15f;

    [Header("Efeito de Ondulação (Ripple)")]
    [Range(0.001f, 0.05f)]
    public float rippleWidth = 0.01f; // Largura da ondulação

    [Range(0, 1f)]
    public float rippleIntensity = 0.5f; // Intensidade da ondulação

    [Header("Texturas")]
    public Texture2D waterTexture;

    private Material reflectionMaterial;
    private TilemapRenderer tilemapRenderer;

    void Start()
    {
        // Obter o componente TilemapRenderer
        tilemapRenderer = GetComponent<TilemapRenderer>();

        if (tilemapRenderer != null)
        {
            // Criar uma instância do material com o shader
            reflectionMaterial = new Material(Shader.Find("Custom/RippleDropsGreenRainEffect"));

            // Aplicar o material ao renderer
            tilemapRenderer.material = reflectionMaterial;

            // Configurar as propriedades iniciais
            UpdateShaderProperties();
        }
        else
        {
            Debug.LogError("Componente TilemapRenderer não encontrado!");
        }
    }

    void Update()
    {
        // Atualizar propriedades do shader em tempo real
        if (reflectionMaterial != null)
        {
            UpdateShaderProperties();
        }
    }

    void UpdateShaderProperties()
    {
        // Definir a textura principal (a textura atual do tilemap)
        if (tilemapRenderer.material.mainTexture != null)
        {
            reflectionMaterial.SetTexture("_MainTex", tilemapRenderer.material.mainTexture);
        }

        // Definir a textura de água verde
        if (waterTexture != null)
        {
            reflectionMaterial.SetTexture("_WaterTex", waterTexture);
        }

        // Atualizar os parâmetros do shader
        reflectionMaterial.SetFloat("_DistortionAmount", distortionAmount);
        reflectionMaterial.SetFloat("_RippleSpeed", rippleSpeed);
        reflectionMaterial.SetFloat("_GreenTint", greenTint);
        reflectionMaterial.SetFloat("_Darkness", darkness);
        reflectionMaterial.SetFloat("_WaterBlend", waterBlend);
        reflectionMaterial.SetFloat("_HorizontalMovement", horizontalMovement);
        reflectionMaterial.SetFloat("_DropSize", dropSize);
        reflectionMaterial.SetFloat("_DropBrightness", dropBrightness);
        reflectionMaterial.SetFloat("_RippleWidth", rippleWidth);
        reflectionMaterial.SetFloat("_RippleIntensity", rippleIntensity);
    }
}
