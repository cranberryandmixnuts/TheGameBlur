using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Animator))]
public class SlotMachineSlotView : MonoBehaviour
{
    [SerializeField] private Image startImage;
    [SerializeField] private Image resultImage;
    [SerializeField] private List<Image> images;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void SetSlot(Sprite startSprite, Sprite resultSprite, List<Sprite> sprites)
    {
        animator.Rebind();

        if(startSprite != null) startImage.sprite = startSprite;
        if (resultSprite != null) resultImage.sprite = resultSprite;

        for (int index = 0; index < images.Count; index++)
        {
            images[index].sprite = sprites[Random.Range(0, sprites.Count)];
        }
    }

    public void PlaySlot()
    {
        animator.Play("SlotAnimation");
    }
}
