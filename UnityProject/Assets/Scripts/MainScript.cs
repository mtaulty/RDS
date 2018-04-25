using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class MainScript : MonoBehaviour
{
    public Transform textMeshObject;

    private void Start()
    {
        var lookupTable = new Dictionary<string, Action>
        {
            { "scan", this.OnScan },
            { "reset", this.OnReset }
        };

        this.recognizer = new KeywordRecognizer(lookupTable.Keys.ToArray());

        this.recognizer.OnPhraseRecognized += (e) =>
        {
            if ((e.confidence == ConfidenceLevel.Medium) ||
                (e.confidence == ConfidenceLevel.High))
            {
                var text = e.text.Trim().ToLower();

                if (lookupTable.ContainsKey(text))
                {
                    lookupTable[text]();
                }
            }
        };
        this.recognizer.Start();

        this.textMesh = this.textMeshObject.GetComponent<TextMesh>();
        this.OnReset();
    }
    public void OnScan()
    {
        this.textMesh.text = "scanning for 30s";

#if !UNITY_EDITOR
        MediaFrameProcessing.Wrappers.OcrRegexScanner.ScanFirstCameraForRegex(
            "^[0-9]{6}$",
            result =>
            {
                this.textMesh.text = result?.ToString() ?? "not found";
            },
            TimeSpan.FromSeconds(30));
#endif
    }
    public void OnReset()
    {
        this.textMesh.text = "say scan to start";
    }
    KeywordRecognizer recognizer;
    TextMesh textMesh;
}
