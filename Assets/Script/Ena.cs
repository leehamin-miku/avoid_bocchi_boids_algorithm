using UnityEngine;

public class Ena : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] float speed;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 vec = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        // 경계 처리 (Simple wrapping으로 변경하는 것이 일반적이지만, 현재 bounce 로직 유지)
        vec.x = Mathf.Abs(transform.position.x + vec.normalized.x * speed) >= AgentManager.Instance.boundaryX ? 0 : vec.x;
        vec.y = Mathf.Abs(transform.position.y + vec.normalized.y * speed) >= AgentManager.Instance.boundaryY ? 0 : vec.y;
        transform.position += vec;
    }
}
