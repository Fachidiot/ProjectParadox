using TMPro;
using UnityEngine;

public class Popup_ItemGet : PopupBase
{
    public static Popup_ItemGet instance { get; private set; }

    #region Type
    public struct SOption
    {
        public string id;
        public SOption(string _id) { id = _id; }
    }
    #endregion

    #region Inspector
    [SerializeField] private TMP_Text m_NameText;
    [SerializeField] private TMP_Text m_DescText;
    #endregion

    #region Value
    private GameObject m_CurrentModel;
    private Vector3 m_RotationSpeed;
    #endregion

    #region Event
    public override void InitSingleton()
    {
        instance = this;
        base.InitSingleton();
    }

    public override void OnOpen(object _option = null)
    {
        base.OnOpen(_option);
        if (_option is SOption o)
        {
            var data = TableManager.instance.Get<Table_Item.Data>(o.id);
            if (data != null)
                Apply(data);
        }
    }

    public override void OnClose(object _option = null)
    {
        base.OnClose(_option);
        ClearModel();
    }

    private void Update()
    {
        if (m_CurrentModel != null)
            m_CurrentModel.transform.Rotate(m_RotationSpeed * Time.deltaTime);
    }
    #endregion

    #region Function
    private void Apply(Table_Item.Data _data)
    {
        var lang = LanguageManager.instance.Language.v;
        m_NameText.text = _data.Name.Translate(lang, false);
        m_DescText.text = _data.Description.Translate(lang, false);

        ClearModel();

        var cam = LocalPopupManager.instance.CurCam;
        if (_data.ItemModel != null && cam != null)
        {
            var spawnPos = cam.transform.position + cam.transform.forward * _data.PopupDistance;
            m_CurrentModel = Instantiate(_data.ItemModel, spawnPos, Quaternion.identity, cam.transform);
            m_CurrentModel.transform.LookAt(cam.transform.position);
            m_CurrentModel.transform.Rotate(0, 180, 0);

            m_RotationSpeed = new Vector3(
                Random.Range(10f, 30f),
                Random.Range(20f, 40f),
                Random.Range(5f, 15f));

            m_CurrentModel.transform.Rotate(
                Random.Range(0, 360),
                Random.Range(0, 360),
                Random.Range(0, 360));

            SetLayerRecursive(m_CurrentModel, LayerMask.NameToLayer("UI"));
        }
    }

    private static void SetLayerRecursive(GameObject _obj, int _layer)
    {
        _obj.layer = _layer;
        foreach (Transform child in _obj.transform)
            SetLayerRecursive(child.gameObject, _layer);
    }

    private void ClearModel()
    {
        if (m_CurrentModel != null)
        {
            Destroy(m_CurrentModel);
            m_CurrentModel = null;
        }
    }
    #endregion
}
