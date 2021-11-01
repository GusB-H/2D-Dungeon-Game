using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public GameObject healthBarPrefab;
    public Entity entity;
    public bool isPlayerHealthbar;
    public RectTransform healthBar;
    public Slider healthBarSlider;
    public Image healthBarSliderImage;
    public RectTransform secondaryTransform;
    public Slider secondarySlider;
    public Image secondarySliderImage;
    public RectTransform bleedTransform;
    public Slider bleedSlider;
    public Image bleedSliderImage;
    public RectTransform bleedOverlayTransform;
    public Slider bleedOverlaySlider;
    public Image bleedOverlaySliderImage;
    public Color healthColor;
    public Color healColor;
    public Color damageColor;
    public int maxPipsPerRow;
    public List<GameObject> injurySprites;
    public GameObject bleedPrefab;

    private void Start()
    {
        if (isPlayerHealthbar)
        {
            entity = GameObject.Find("Player").GetComponent<PlayerController>();
            healthBar = Instantiate(healthBarPrefab, transform).GetComponent<RectTransform>();
            healthBar.anchoredPosition = new Vector2(64, -4);
        }
        else
        {
            if (transform.parent.GetComponent<Entity>())
            {
                entity = transform.parent.GetComponent<Entity>();
                healthBar = Instantiate(healthBarPrefab, transform).GetComponent<RectTransform>();
            }
            else
            {
                Debug.LogWarning("Attempted to create a health bar whose parent did not have an entity component. What would it even display?!");
                Destroy(gameObject);
                return;
            }
        }

        healthBar.sizeDelta = new Vector2(16 * entity.maxHealth + 4, 20);


        healthBarSlider = healthBar.GetComponent<Slider>();
        healthBarSliderImage = healthBar.Find("Fill Area").GetChild(0).GetComponent<Image>();
        secondaryTransform = healthBar.Find("Secondary Slider").GetComponent<RectTransform>();
        bleedTransform = healthBar.Find("Bleed Display").GetComponent<RectTransform>();
        bleedOverlayTransform = healthBar.Find("Bleed Overlay").GetComponent<RectTransform>();
        secondarySlider = secondaryTransform.GetComponent<Slider>();
        secondarySliderImage = secondarySlider.transform.GetChild(0).GetChild(0).GetComponent<Image>();
        bleedSlider = bleedTransform.GetComponent<Slider>();
        bleedSliderImage = bleedTransform.GetChild(0).GetChild(0).GetComponent<Image>();
        bleedOverlaySlider = bleedOverlayTransform.GetComponent<Slider>();
        bleedOverlaySliderImage = bleedOverlayTransform.GetChild(0).GetChild(0).GetComponent<Image>();


        healthBarSlider.maxValue = entity.maxHealth;
        secondarySlider.maxValue = entity.maxHealth;
        healthBarSlider.value = entity.health;
        secondarySlider.value = entity.health;
        UpdateHealthBar();

        //if (isPlayerHealthbar) healthBarBackground.transform.localScale *= 2;
       
    }

    private void UpdateHealthBar()
    {
        if (!entity)
        {
            return;
        }


        healthBarSliderImage.color = healthColor;
        healthBar.sizeDelta = new Vector2(16 * entity.maxHealth + 4, 20);
        healthBarSlider.maxValue = entity.maxHealth;

        secondaryTransform.sizeDelta = new Vector2(16 * entity.maxHealth + 4, 20);
        secondarySlider.maxValue = entity.maxHealth;

        bleedTransform.sizeDelta = new Vector2(16 * entity.maxHealth + 4, 20);
        bleedSlider.maxValue = entity.maxHealth;
        bleedSlider.value = entity.bleed;

        bleedOverlayTransform.sizeDelta = new Vector2(16 * entity.health + 4, 20);
        bleedOverlaySlider.maxValue = entity.health;
        bleedOverlaySlider.value = entity.health - (entity.maxHealth - entity.bleed);


        if (entity.health > healthBarSlider.value)
        {
            secondarySliderImage.color = healColor;
            healthBarSlider.value = Mathf.Lerp(healthBarSlider.value, entity.health, 0.015f) + 0.01f;
            if (healthBarSlider.value > entity.health) healthBarSlider.value = entity.health;
            secondarySlider.value = entity.health;
        }
        else if(entity.health < secondarySlider.value)
        {
            secondarySliderImage.color = damageColor;
            healthBarSlider.value = entity.health;
            if (entity.hitstop <= 0)
            {
                secondarySlider.value = Mathf.Lerp(secondarySlider.value, entity.health, 0.015f) - 0.01f;
                if (secondarySlider.value < entity.health) secondarySlider.value = entity.health;
            }
        }

        if(entity.health > entity.maxHealth - entity.bleed)
        {
            secondarySliderImage.color = damageColor;
            healthBarSlider.value = entity.health - 1 + entity.bleedCooldown / (entity.bleedRate * 60f);
        }
    }

    private void FixedUpdate()
    {
        UpdateHealthBar();   
    }
}
