using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public Text text;
    public Button btn;

    public Color32 colorA;
    public Color32 colorB;
    public Color32 colorC;
    public Color32 outColor;
    public Material mat;
    [Range(0f, 1f)]
    public float t;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        btn.onClick.AddListener(OnBtnClick);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateText();
    }

    void UpdateText()
    {
        if (ConditionManager.Instance == null)
            return;
        if (UserStudyManager.Instance == null)
            return;
        if (DockingManager.Instance == null)
            return;

        text.text = string.Format("Condition: {0}\nCondition: {1} / 8\nTrial: {2} / 10\n\nCompletion Time: {3:F2}s",
            ConditionManager.Instance.curCondition.ToString(),
            UserStudyManager.Instance.autoConditionCounter + 1,
            UserStudyManager.Instance.currentTrial + 1,
            DockingManager.Instance.timer
            );
    }

    public void SetText(string s = "")
    {
        text.text = s;
    }

    void OnBtnClick()
    {
        UserStudyManager.Instance.PracticeMode();
    }

    private void OnValidate()
    {
        UpdateColor();
    }

    public void SetColor(float tt)
    {
        t = tt;
        UpdateColor();
    }

    void UpdateColor()
    {
        if (t < 0.5f)
            outColor = Color32.Lerp(colorA, colorB, 2f * t);
        else
            outColor = Color32.Lerp(colorB, colorC, 2f * t - 1f);

        mat.SetColor(Shader.PropertyToID("g_vOutlineColor"), outColor);
    }
}
