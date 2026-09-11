using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement; // 씬 관리를 위해 필수!

public class MainMenu : MonoBehaviour
{
    // 게임 시작 버튼을 누르면 호출될 함수
    public void GameStart()
    {
        // "SampleScene"이라는 이름의 씬을 불러옵니다.
        // 만약 스테이지 1 씬의 이름이 다르다면 그 이름으로 바꿔주세요.
        SceneManager.LoadScene("SampleScene");
    }

    // 게임 종료 버튼을 누르면 호출될 함수
    public void GameQuit()
    {
        Debug.Log("게임을 종료합니다."); // 테스트용 로그
        Application.Quit();

        // 유니티 에디터에서 테스트할 때 종료되도록 하는 코드
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}