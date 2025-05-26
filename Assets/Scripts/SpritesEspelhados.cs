// Script para fazer o reflexo ondular suavemente
using UnityEngine;

public class SimpleReflectionWave : MonoBehaviour
{
    public float waveSpeed = 2.0f;
    public float waveAmount = 0.005f;

    private Material material;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Cria uma inst�ncia do material para n�o afetar outros objetos
        material = new Material(spriteRenderer.material);
        spriteRenderer.material = material;
    }

    void Update()
    {
        // Aplica uma pequena distor��o no eixo X baseada no tempo
        Vector2 offset = new Vector2(
            Mathf.Sin(Time.time * waveSpeed) * waveAmount,
            0
        );

        material.mainTextureOffset = offset;
    }
}
