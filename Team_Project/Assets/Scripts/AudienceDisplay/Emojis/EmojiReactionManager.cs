using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EmojiReactionManager : MonoBehaviour
{
    [System.Serializable]
    public class EmojiSet
    {
        public string setName;
        public Sprite[] sprites;
    }

    [SerializeField] private RectTransform spawnArea;
    [SerializeField] private Image emojiPrefab;

    [Header("Emoji Sets")]
    [SerializeField] private EmojiSet deathSet;
    [SerializeField] private EmojiSet puzzleSet;
    [SerializeField] private EmojiSet smokeSet;
    [SerializeField] private EmojiSet gravitySet;

    [Header("Burst Settings")]
    [SerializeField] private int emojiCount = 8;
    [SerializeField] private float lifeTime = 1.2f;
    [SerializeField] private float moveUpAmount = 120f;
    [SerializeField] private float sidewaysAmount = 80f;
    [SerializeField] private Vector2 sizeRange = new Vector2(60f, 100f);

    public void PlayDeathBurst()
    {
        PlayBurst(deathSet.sprites);
    }

    public void PlayPuzzleBurst()
    {
        PlayBurst(puzzleSet.sprites);
    }

    public void PlaySmokeBurst()
    {
        PlayBurst(smokeSet.sprites);
    }

    public void PlayGravityBurst()
    {
        PlayBurst(gravitySet.sprites);
    }

    private void PlayBurst(Sprite[] sprites)
    {
        if (spawnArea == null || emojiPrefab == null || sprites == null || sprites.Length == 0)
            return;

        for (int i = 0; i < emojiCount; i++)
        {
            Image emoji = Instantiate(emojiPrefab, spawnArea);
            emoji.gameObject.SetActive(true);
            emoji.sprite = sprites[Random.Range(0, sprites.Length)];

            RectTransform rt = emoji.rectTransform;

            float width = spawnArea.rect.width;
            float height = spawnArea.rect.height;

            float x = Random.Range(0f, width);
            float y = Random.Range(0f, height);

            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(0f, 0f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(x, y);

            float size = Random.Range(sizeRange.x, sizeRange.y);
            rt.sizeDelta = new Vector2(size, size);
            rt.localRotation = Quaternion.Euler(0f, 0f, Random.Range(-20f, 20f));

            Vector2 drift = new Vector2(
                Random.Range(-sidewaysAmount, sidewaysAmount),
                Random.Range(moveUpAmount * 0.6f, moveUpAmount)
            );

            StartCoroutine(AnimateEmoji(emoji, rt, rt.anchoredPosition, rt.anchoredPosition + drift));
        }
    }

    private IEnumerator AnimateEmoji(Image emoji, RectTransform rt, Vector2 startPos, Vector2 endPos)
    {
        float time = 0f;
        Color startColor = emoji.color;
        Color endColor = startColor;
        endColor.a = 0f;

        Vector3 startScale = Vector3.one * 0.7f;
        Vector3 endScale = Vector3.one * 1.2f;

        while (time < lifeTime)
        {
            time += Time.deltaTime;
            float t = time / lifeTime;

            rt.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            rt.localScale = Vector3.Lerp(startScale, endScale, t);
            emoji.color = Color.Lerp(startColor, endColor, t);

            yield return null;
        }

        Destroy(emoji.gameObject);
    }
}