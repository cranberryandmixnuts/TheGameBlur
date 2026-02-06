using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class SlotMachineView : MonoBehaviour
{
    [SerializeField] private List<SlotMachineSlotView> slotViews;
    [SerializeField] private SlotSprites slotSprites;
    [SerializeField] private float slotPlayDelay;

    private List<Sprite> lastSprites = new();

    private void Start()
    {
        List<Sprite> sprites = new List<Sprite>();

        foreach(SlotMachineSlotView slotView in slotViews)
        {
            var sprite = slotSprites.GetSprite(EnumExtension.GetRandomEnum(exceptionType: ImageType.None));
            sprites.Add(sprite);

            slotView.SetSlot(
                sprite,
                sprite, 
                slotSprites.ToList()
                );
        }

        lastSprites = sprites;
    }

    public void PlaySlot(List<ImageType> imageTypes)
    {
        if(imageTypes.Count != 3)
            throw new ArgumentOutOfRangeException($"Slot Count : {imageTypes.Count}");

        List<Sprite> sprites = new List<Sprite>();

        for (int index = 0; index < imageTypes.Count; index++)
        {
            var sprite = slotSprites.GetSprite(imageTypes[index]);
            sprites.Add(sprite);

            slotViews[index].SetSlot(
                lastSprites[index],
                sprite,
                slotSprites.ToList()
                );
        }

        lastSprites = sprites;
        StartCoroutine(PlaySlot());
    }

    private IEnumerator PlaySlot()
    {
        for (int index = 0; index < slotViews.Count; index++)
        {
            slotViews[index].PlaySlot();
            yield return new WaitForSeconds(slotPlayDelay);
        }
    }

    [Serializable]
    public class SlotSprites
    {
        [SerializeField] private Sprite sevenSprite;
        [SerializeField] private Sprite cherrySprite;
        [SerializeField] private Sprite lemonSprite;
        [SerializeField] private Sprite orangeSprite;
        [SerializeField] private Sprite poopSprite;

        public List<Sprite> ToList() =>
            new() { sevenSprite, cherrySprite, lemonSprite, orangeSprite, poopSprite };

        public Sprite GetSprite(ImageType imageType)
        {
            if (imageType == ImageType.None)
                return null;

            var spriteList = ToList();
            foreach(var sprite in spriteList)
            {
                if(sprite.name.ToLower().Contains(imageType.ToString().ToLower()))
                {
                    return sprite;
                }
            }

            throw new InvalidCastException($"Invalid ImageType : {imageType}");
        }
    }

    public struct SlotInfo
    {
        public SlotInfo(ImageType startImage, ImageType resultImage)
        {
            StartImage = startImage;
            ResultImage = resultImage;
        }

        public SlotInfo(ImageType resultImage)
        {
            StartImage = ImageType.None;
            ResultImage = resultImage;
        }

        public ImageType StartImage;
        public ImageType ResultImage;
    }

    public enum ImageType
    {
        None,
        Seven,
        Cherry,
        Lemon,
        Orange,
        Poop
    }
}
