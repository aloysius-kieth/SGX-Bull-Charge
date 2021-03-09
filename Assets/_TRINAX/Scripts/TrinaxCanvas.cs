using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrinaxCanvas : MonoBehaviour
{
    public TrinaxAdminPanel adminPanel;
    Reporter reporter;

    #region SINGLETON
    public static TrinaxCanvas Instance { get; set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    #endregion

    private void Start()
    {
        reporter = GameObject.FindObjectOfType<Reporter>();
    }

    private void Update()
    {
        ToggleAdminPanel(adminPanel);
    }

    void ToggleAdminPanel(TrinaxAdminPanel _aP)
    {
        if (Input.GetKeyDown(KeyCode.F12))
        {
            _aP.gameObject.SetActive(!_aP.gameObject.activeSelf);
            if (reporter.show)
            {
                reporter.show = !reporter.show;
            }
        }
        if (Input.GetKeyDown(KeyCode.F10) && _aP.gameObject.activeSelf)
        {
            reporter.show = !reporter.show;
            if (reporter.show)
            {
                reporter.doShow();
            }
            else return;
        }
    }

}
