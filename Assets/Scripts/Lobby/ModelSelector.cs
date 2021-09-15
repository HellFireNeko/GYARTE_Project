using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class ModelSelector : MonoBehaviour
{
    public UnityEvent<ModelPick> OnSelectionChanged;
    [SerializeField] private PlayerModelSelector Selector;
    [SerializeField] private Button NextButton;
    [SerializeField] private Button PreviousButton;
    [SerializeField] private TMP_Text ModelNameText;
    [SerializeField] private RawImage ModelViewImage;

    private int SelectedModelIndex = 0;

    void Start()
    {
        var x = Selector.GetItemById(SelectedModelIndex);
        ModelNameText.text = x.Value.Model.name;
        ModelViewImage.texture = x.Value.Texture;

        NextButton.onClick.AddListener(() =>
        {
            SelectedModelIndex++;
            if (SelectedModelIndex >= Selector.PlayerModelObjects.Count)
            {
                SelectedModelIndex = 0;
            }
            var model = Selector.GetItemById(SelectedModelIndex);
            if (model.HasValue)
            {
                ModelNameText.text = model.Value.Model.name;
                ModelViewImage.texture = model.Value.Texture;
                OnSelectionChanged.Invoke(new ModelPick(model.Value.Model.name, SelectedModelIndex, model.Value.Texture));
            }
        });

        PreviousButton.onClick.AddListener(() =>
        {
            SelectedModelIndex--;
            if (SelectedModelIndex < 0)
            {
                SelectedModelIndex = Selector.PlayerModelObjects.Count - 1;
            }
            var model = Selector.GetItemById(SelectedModelIndex);
            if (model.HasValue)
            {
                ModelNameText.text = model.Value.Model.name;
                ModelViewImage.texture = model.Value.Texture;
                OnSelectionChanged.Invoke(new ModelPick(model.Value.Model.name, SelectedModelIndex, model.Value.Texture));
            }
        });
    }

    void OnEnable()
    {
        SelectedModelIndex = 0;
        var x = Selector.GetItemById(SelectedModelIndex);
        ModelNameText.text = x.Value.Model.name;
        ModelViewImage.texture = x.Value.Texture;
    }
}

[System.Serializable]
public struct ModelPick
{
    public string Name;
    public int ID;
    public RenderTexture Texture;

    public ModelPick(string name, int iD, RenderTexture texture)
    {
        Name = name;
        ID = iD;
        Texture = texture;
    }
}
