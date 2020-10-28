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

    public List<Color32> lineColors;
    
    public Material mat;
    public Material connectionMat;
    public Material cursorMat;
    [Range(0f, 1f)]
    public float t;

    public bool updateCamPos = false;
    [Range(0f, 1f)]
    public float camPosT;
    public Transform origin;
    public Transform obj;
    public Transform cam;
    public Transform camRoot;
    public float camDist = 1f;

    public GameObject pointer;
    public ConnectionLine line;
    public Transform wand;

    Color32 outColor;
    bool camZoom = false;


    public void CamZoom(bool on)
    {
        if (on == camZoom)
            return;

        camZoom = on;

        if (!camZoom)
        {
            camPosT = 1f;
        }
        else
        {
            camPosT = 0.3333f;
            //float d = Vector3.Distance(origin.position, obj.position);
            //camPosT = Mathf.Clamp01(camDist / d);
        }

        camRoot.position = Vector3.Lerp(obj.position + (camRoot.position - cam.position), origin.position, camPosT);
    }


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

        //UpdateCamPos();
    }

    void UpdateText()
    {
        if (ConditionManager.Instance == null)
            return;
        if (UserStudyManager.Instance == null)
            return;
        if (DockingManager.Instance == null)
            return;

        var cc = UserStudyManager.Instance.autoConditionCounter + 1;
        var nc = UserStudyManager.Instance.numOfConditions;
        var ct = UserStudyManager.Instance.currentTrial + 1;
        var nt = UserStudyManager.Instance.numOfTrials;

        text.text = string.Format("Condition:\t{0} [{1}]\nTrial:\t{2} / {3}\nProgress:\t{4} / {5}\n\nCompletion Time:\t{6:F2}s",
            ConditionManager.Instance.GetCurrentConditionName(),
            cc,
            ct,
            nt,
            (cc-1) * nt + ct,
            nc * nt,
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

    public void SetCursorColor(Color c)
    {
        if (cursorMat == null)
            return;

        cursorMat.SetColor(Shader.PropertyToID("g_vOutlineColor"), c);
    }

    public void SetLineColor(int i)
    {
        if (lineColors == null || i >= lineColors.Count)
            return;

        if (connectionMat == null)
            return;

        connectionMat.SetColor(Shader.PropertyToID("_Color"), lineColors[i]);
    }

    public void SetupPointer(Transform objTransform, bool pointerActive)
    {
        if (pointer != null)
            pointer.SetActive(pointerActive);

        if (line != null)
        {
            List<Transform> t = new List<Transform>();

            if (!pointerActive)
            {
                t.Add(wand);
                t.Add(objTransform);
            }

            line.Setup(t);
        }
    }
}
