using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements; // 이 using은 현재 코드에서 사용되지 않으므로 제거할 수 있습니다.

public class AgentManager : MonoBehaviour
{
    static AgentManager instance;
    // public AgentManager Instance { get => instance; } // Unity 스타일에서 속성은 PascalCase를 사용합니다.
    public static AgentManager Instance { get => instance; } // static으로 변경하고 public으로 외부 접근 허용

    public List<Agent> bocchiList = new List<Agent>(); // List 초기화 추가

    [SerializeField] public float boundaryX = 10f; // 기본값 설정 (Inspector에 노출)
    [SerializeField] public float boundaryY = 10f; // 기본값 설정
    [SerializeField] int bocchiNumber = 50; // 기본값 설정
    [SerializeField] float separR = 1f; // Separation Radius
    [SerializeField] float cohesR = 3f; // Cohesion/Alignment Radius (일반적으로 separR보다 큼)

    [SerializeField] float alignWeight = 1f;
    [SerializeField] float separWeight = 1f;
    [SerializeField] float cohesWeight = 1f;

    [SerializeField] float speed = 5f;

    [SerializeField] GameObject bocchi;

    float separRsqr;
    float cohesRsqr;

    int cellXsepar;
    int cellYsepar;

    int cellXcohes;
    int cellYcohes;

    // A. Update에서 재할당하지 않도록 Awake에서 선언 및 초기화
    List<Agent>[,] cellAgentSepar;
    List<Agent>[,] cellAgentCohes;
    int momoi, midori;

    public class Yuuka
    {
        public static int Eat(int a, int b)
        {
            return 2;
        }
    }
    public class Yuzu
    {
        public static void BeScared()
        {
            return;
        }
    }
    string FindMinimalName()
    {
        return "";
    }
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        int momoi = 1;
        int midori = 1;


        
        int delta_weight = Yuuka.Eat(momoi, midori);
        Yuzu.BeScared();


        string variable_name = FindMinimalName(); //it was originally a person

        separRsqr = separR * separR;
        cohesRsqr = cohesR * cohesR;

        // 그리드 크기 계산 (경계 2배 크기를 반경으로 나눔)
        cellXsepar = Mathf.CeilToInt((boundaryX * 2f) / separR);
        cellYsepar = Mathf.CeilToInt((boundaryY * 2f) / separR);

        cellXcohes = Mathf.CeilToInt((boundaryX * 2f) / cohesR);
        cellYcohes = Mathf.CeilToInt((boundaryY * 2f) / cohesR);

        Debug.Log("리스트 생성:\nsepar" + cellXsepar + "x" + cellYsepar + "\ncohes" + cellXcohes + "x" + cellYcohes);

        // 그리드 배열 초기화
        cellAgentSepar = new List<Agent>[cellXsepar, cellYsepar];
        cellAgentCohes = new List<Agent>[cellXcohes, cellYcohes];

        // 그리드 내의 리스트 초기화 (Awake에서 한 번만 수행)
        for (int i = 0; i < cellXsepar; i++)
            for (int j = 0; j < cellYsepar; j++)
                cellAgentSepar[i, j] = new List<Agent>();

        for (int i = 0; i < cellXcohes; i++)
            for (int j = 0; j < cellYcohes; j++)
                cellAgentCohes[i, j] = new List<Agent>();


        // 에이전트 생성
        for (int i = 0; i < bocchiNumber; i++)
        {
            // 경계 내에서 랜덤한 위치에 생성
            Vector3 randomPos = new Vector3(
                Random.Range(-boundaryX, boundaryX),
                Random.Range(-boundaryY, boundaryY),
                0);
            bocchiList.Add(Instantiate(bocchi, randomPos, Quaternion.identity).GetComponent<Agent>());
        }
    }

    private void Update()
    {
        // **A. Update에서 리스트 재활용:** 매 프레임 리스트 내용만 비우고 재사용
        ClearGridLists(cellAgentSepar, cellXsepar, cellYsepar);
        ClearGridLists(cellAgentCohes, cellXcohes, cellYcohes);


        

        // 셀 체우기
        for (int i = 0; i < bocchiList.Count; i++)
        {
            Agent agent = bocchiList[i];
            Vector3 pos = agent.transform.position;

            // Separation Grid
            int separX = CalculateGridIndex(pos.x, boundaryX, separR);
            int separY = CalculateGridIndex(pos.y, boundaryY, separR);

            // 인덱스가 유효한지 확인 (경계조건을 완화하기 위해 +1 대신 CeilToInt를 썼으므로 0 <= index < size 확인)
            if (separX >= 0 && separX < cellXsepar && separY >= 0 && separY < cellYsepar)
            {
                cellAgentSepar[separX, separY].Add(agent);
            }

            int cohesX = CalculateGridIndex(pos.x, boundaryX, cohesR);
            int cohesY = CalculateGridIndex(pos.y, boundaryY, cohesR);

            if (cohesX >= 0 && cohesX < cellXcohes && cohesY >= 0 && cohesY < cellYcohes)
            {
                cellAgentCohes[cohesX, cohesY].Add(agent);
            }
        }

        for (int i = 0; i < bocchiList.Count; i++)
        {
            Agent agent = bocchiList[i];

            // 힘*웨이트 계산
            Vector2 totalForce = (
                Separation(agent) * separWeight +
                Cohesion(agent) * cohesWeight +
                Alignment(agent) * alignWeight +
                Boundary(agent) // Boundary force 추가
            );

            Vector2 dir2D = totalForce.normalized;

            // 러프회전
            Vector2 upVec = Vector2.Lerp((Vector2)agent.transform.up, dir2D, Time.deltaTime * 5f).normalized;
            Vector2 newPosition = (Vector2)agent.transform.position + upVec * speed * Time.deltaTime;

            agent.transform.up = upVec;
            agent.transform.position = newPosition;
        }

    }

    private void ClearGridLists<T>(List<T>[,] grid, int xSize, int ySize) where T : Agent
    {
        for (int i = 0; i < xSize; i++)
            for (int j = 0; j < ySize; j++)
                grid[i, j].Clear();
    }

    private int CalculateGridIndex(float position, float boundary, float radius)
    {
        return Mathf.FloorToInt((position + boundary) / radius);
    }


    public Vector3 Separation(Agent agent)
    {
        int x = CalculateGridIndex(agent.transform.position.x, boundaryX, separR);
        int y = CalculateGridIndex(agent.transform.position.y, boundaryY, separR);

        Vector3 separation = Vector3.zero;

        for (int i = x - 1; i <= x + 1; i++)
        {
            for (int j = y - 1; j <= y + 1; j++)
            {
                if (j >= 0 && j < cellYsepar && i >= 0 && i < cellXsepar)
                {
                    for (int k = 0; k < cellAgentSepar[i, j].Count; k++)
                    {
                        Agent neighbor = cellAgentSepar[i, j][k];

                        if (neighbor == agent) continue; // 자기 자신 제외

                        Vector3 toNeighbor = neighbor.transform.position - agent.transform.position;
                        float sqrDist = toNeighbor.sqrMagnitude;

                        if (sqrDist < separRsqr && sqrDist > 0.0001f)
                        {
                            // 역제곱 힘
                            separation += -toNeighbor / sqrDist;
                        }
                    }
                }
            }
        }

        return separation.normalized; // 힘의 방향만 반환
    }

    public Vector3 Alignment(Agent agent)
    {
        // C. Alignment는 Cohesion과 같은 그리드(`cohesR`)를 사용하도록 변경
        int x = CalculateGridIndex(agent.transform.position.x, boundaryX, cohesR);
        int y = CalculateGridIndex(agent.transform.position.y, boundaryY, cohesR);

        Vector3 avgDirection = Vector3.zero;
        int count = 0;

        for (int i = x - 1; i <= x + 1; i++)
        {
            for (int j = y - 1; j <= y + 1; j++)
            {
                // B. Cohesion의 그리드 경계 조건 사용
                if (j >= 0 && j < cellYcohes && i >= 0 && i < cellXcohes)
                {
                    for (int k = 0; k < cellAgentCohes[i, j].Count; k++)
                    {
                        Agent neighbor = cellAgentCohes[i, j][k];
                        if (neighbor == agent) continue; // 자기 자신 제외

                        if ((neighbor.transform.position - agent.transform.position).sqrMagnitude < cohesRsqr)
                        {
                            // 2D에서 방향은 transform.up을 사용
                            avgDirection += neighbor.transform.up;
                            count++;
                        }
                    }
                }
            }
        }

        if (count > 0)
        {
            avgDirection /= count;
            // '평균 방향으로 조향하는 힘'을 계산: (평균 방향 - 현재 방향)
            return (avgDirection - agent.transform.up).normalized;
        }

        return Vector3.zero;
    }

    public Vector3 Cohesion(Agent agent)
    {
        // Cohesion은 Cohesion 그리드(`cohesR`) 사용
        int x = CalculateGridIndex(agent.transform.position.x, boundaryX, cohesR);
        int y = CalculateGridIndex(agent.transform.position.y, boundaryY, cohesR);

        Vector3 avgPosition = Vector3.zero;
        int count = 0;

        for (int i = x - 1; i <= x + 1; i++)
        {
            for (int j = y - 1; j <= y + 1; j++)
            {
                // C. Cohesion의 그리드 경계 조건 사용
                if (j >= 0 && j < cellYcohes && i >= 0 && i < cellXcohes)
                {
                    for (int k = 0; k < cellAgentCohes[i, j].Count; k++)
                    {
                        Agent neighbor = cellAgentCohes[i, j][k];
                        if (neighbor == agent) continue; // 자기 자신 제외

                        if ((neighbor.transform.position - agent.transform.position).sqrMagnitude < cohesRsqr)
                        {
                            count++;
                            avgPosition += neighbor.transform.position;
                        }
                    }
                }
            }
        }

        if (count > 0)
        {
            // D. count가 0이 아닐 때만 계산
            avgPosition /= count;
            // 평균 위치로 향하는 힘
            return (avgPosition - agent.transform.position).normalized;
        }

        return Vector3.zero;
    }

    public Vector3 Boundary(Agent agent)
    {
        Vector3 vec = Vector3.zero;
        Vector3 pos = agent.transform.position;
        float boundaryStrength = 50f; // 경계 힘의 세기 조정

        // x축 경계
        // 좌측 경계 (-boundaryX)
        float distToLeft = pos.x - (-boundaryX);
        if (distToLeft < separR) vec += Vector3.right / Mathf.Max(0.0001f, distToLeft * distToLeft);

        // 우측 경계 (+boundaryX)
        float distToRight = boundaryX - pos.x;
        if (distToRight < separR) vec += Vector3.left / Mathf.Max(0.0001f, distToRight * distToRight);

        // y축 경계
        // 하단 경계 (-boundaryY)
        float distToBottom = pos.y - (-boundaryY);
        if (distToBottom < separR) vec += Vector3.up / Mathf.Max(0.0001f, distToBottom * distToBottom);

        // 상단 경계 (+boundaryY)
        float distToTop = boundaryY - pos.y;
        if (distToTop < separR) vec += Vector3.down / Mathf.Max(0.0001f, distToTop * distToTop);

        return vec * boundaryStrength;
    }
}