using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public void MoveToMeshPreview()
    {
        SceneManager.LoadSceneAsync("MeshPreview");
    }

    public void MoveToMain()
    {
        SceneManager.LoadSceneAsync("ARScene");
    }
}
