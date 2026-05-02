using UnityEngine;
using UnityEngine.SceneManagement; // Diperlukan untuk mengelola Scene

public class GameExitManager : MonoBehaviour
{
    private void Update()
    {
        // Mendengarkan apakah tombol Escape (ESC) ditekan pada frame ini
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitGame();
        }

        // Mendengarkan apakah tombol R ditekan pada frame ini
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
    }

    private void ExitGame()
    {
        Debug.Log("Menutup permainan...");

        // Jika permainan sedang dijalankan di dalam Unity Editor
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        // Jika permainan sudah di-build menjadi aplikasi nyata (.exe, .apk, dll)
        Application.Quit();
        #endif
    }

    private void RestartGame()
    {
        Debug.Log("Memulai ulang permainan...");
        
        // Mengambil indeks dari scene yang sedang berjalan dan memuatnya kembali dari awal
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }
}