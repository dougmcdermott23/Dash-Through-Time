using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationFramePickerSystem : MonoBehaviour
{
    public string sheetname;
    private Sprite[] sprites;
    private SpriteRenderer spriteRenderer;
    private string[] names;

    void Awake()
    {
        sprites = Resources.LoadAll<Sprite>(sheetname);
        spriteRenderer = GetComponent<SpriteRenderer>();

        names = new string[sprites.Length];

        for (int i = 0; i < names.Length; i++)
        {
            names[i] = sprites[i].name;
        }
    }

    public void ChangeSprite(int index)
    {
        Sprite sprite = sprites[index];
        spriteRenderer.sprite = sprite;
    }

    public void ChangeSpriteByName(string name)
    {
        Sprite sprite = sprites[Array.IndexOf(names, name)];
        spriteRenderer.sprite = sprite;
    }

    public void RandomSprite()
    {
        int index = UnityEngine.Random.Range(0, sprites.Length);
        spriteRenderer.sprite = sprites[index];
    }
}
